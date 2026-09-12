using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Barefeet.MVVM;
using HamsterStudio.ChatBox.Models;
using HamsterStudio.ChatBox.Services;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace HamsterStudio.ChatBox.ViewModels;

public partial class ChatBoxMainViewModel : KnownViewModel
{
    private readonly ChatService _chatService;
    private readonly ChatSessionService _chatSessionService;
    private CancellationTokenSource? _cts;
    private CancellationTokenSource? _pullCts;

    public ObservableCollection<ChatMessageViewModel> Messages { get; } = [];
    public ObservableCollection<string> Models { get; } = [];

    /// <summary>待发送的图片文件路径列表（多模态）。</summary>
    public ObservableCollection<string> PendingImagePaths { get; } = [];

    public IReadOnlyList<ChatProviderKind> ProviderKinds { get; } = Enum.GetValues<ChatProviderKind>();
    public IReadOnlyList<ThinkLevel> ThinkLevels { get; } = Enum.GetValues<ThinkLevel>();

    [ObservableProperty]
    private ChatProviderKind _selectedProvider = ChatProviderKind.Ollama;

    [ObservableProperty]
    private ThinkLevel _selectedThinkLevel = ThinkLevel.Off;

    [ObservableProperty]
    private string _selectedModel = string.Empty;

    [ObservableProperty]
    private string _inputText = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isPulling;

    [ObservableProperty]
    private string _baseUrl = "http://localhost:11434";

    [ObservableProperty]
    private string _apiKey = string.Empty;

    [ObservableProperty]
    private string _statusText = "就绪";

    [ObservableProperty]
    private string _systemPrompt = LoadSystemPrompt();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TokenUsageText))]
    private long _promptTokens;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TokenUsageText))]
    private long _completionTokens;

    /// <summary>token 统计文本（↑输入 ↓输出）。</summary>
    public string TokenUsageText => $"↑{PromptTokens:N0}  ↓{CompletionTokens:N0}";

    [ObservableProperty]
    private string _dualTopic = string.Empty;

    [ObservableProperty]
    private string _dualArgumentA = string.Empty;

    [ObservableProperty]
    private string _dualArgumentB = string.Empty;

    [ObservableProperty]
    private string _dualModelA = string.Empty;

    [ObservableProperty]
    private string _dualModelB = string.Empty;

    [ObservableProperty]
    private int _dualRounds = 3;

    public ChatBoxMainViewModel(ChatService chatService, ChatSessionService chatSessionService, ILogger<ChatBoxMainViewModel>? logger) : base(logger)
    {
        _chatService = chatService;
        _chatSessionService = chatSessionService;
        DisplayName = "ChatBox";
    }

    private ChatProviderSettings BuildSettings() => new()
    {
        Kind = SelectedProvider,
        BaseUrl = BaseUrl?.Trim() ?? string.Empty,
        ApiKey = ApiKey?.Trim() ?? string.Empty,
        Model = SelectedModel?.Trim() ?? string.Empty,
        ThinkLevel = SelectedThinkLevel,
    };

    partial void OnSelectedProviderChanged(ChatProviderKind value)
    {
        (BaseUrl, ApiKey, SelectedModel) = value switch
        {
            ChatProviderKind.Ollama => ("http://localhost:11434", string.Empty, "llama3.2"),
            ChatProviderKind.OpenAI => ("https://api.openai.com/v1", string.Empty, "gpt-4o-mini"),
            ChatProviderKind.DeepSeek => ("https://api.deepseek.com", LoadDeepSeekKey(), "deepseek-chat"),
            _ => (string.Empty, string.Empty, string.Empty),
        };

        Models.Clear();
        _ = RefreshModelsAsync();
    }

    partial void OnInputTextChanged(string value) => SendCommand.NotifyCanExecuteChanged();

    partial void OnDualTopicChanged(string value) => StartDualChatCommand.NotifyCanExecuteChanged();

    partial void OnSelectedModelChanged(string value) => PullModelCommand.NotifyCanExecuteChanged();

    partial void OnIsBusyChanged(bool value)
    {
        SendCommand.NotifyCanExecuteChanged();
        StopCommand.NotifyCanExecuteChanged();
        PullModelCommand.NotifyCanExecuteChanged();
        RetryCommand.NotifyCanExecuteChanged();
        EditMessageCommand.NotifyCanExecuteChanged();
        StartDualChatCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsPullingChanged(bool value)
    {
        PullModelCommand.NotifyCanExecuteChanged();
        CancelPullCommand.NotifyCanExecuteChanged();
    }

    private bool CanSend() => !IsBusy && (!string.IsNullOrWhiteSpace(InputText) || PendingImagePaths.Count > 0);

    [RelayCommand(CanExecute = nameof(CanSend))]
    private Task SendAsync()
    {
        var text = InputText?.Trim();
        if ((string.IsNullOrEmpty(text) && PendingImagePaths.Count == 0) || IsBusy)
            return Task.CompletedTask;

        if (string.IsNullOrWhiteSpace(BaseUrl))
        {
            StatusText = "请先在“连接设置”中填写服务地址";
            return Task.CompletedTask;
        }

        var images = PendingImagePaths.Count > 0 ? PendingImagePaths.ToList() : null;
        InputText = string.Empty;
        PendingImagePaths.Clear();

        return SendCoreAsync(text ?? string.Empty, images);
    }

    private bool CanRetry() => !IsBusy;

    private bool CanEdit() => !IsBusy;

    [RelayCommand(CanExecute = nameof(CanRetry))]
    private async Task RetryAsync(ChatMessageViewModel? message)
    {
        if (message is null || IsBusy)
            return;

        var index = Messages.IndexOf(message);
        if (index < 0)
            return;

        // 找到该消息之前最近的一条用户消息
        ChatMessageViewModel? userMessage = null;
        for (var i = index - 1; i >= 0; i--)
        {
            if (Messages[i].Role == ChatRole.User)
            {
                userMessage = Messages[i];
                break;
            }
        }

        if (userMessage is null)
            return;

        var text = userMessage.Content;
        var images = userMessage.Images.Count > 0 ? userMessage.Images.ToList() : null;

        // 截断该消息及其之后的所有消息，再移除对应的用户消息，重新发送
        while (Messages.Count > index)
            Messages.RemoveAt(Messages.Count - 1);
        Messages.Remove(userMessage);

        await SendCoreAsync(text, images);
    }

    [RelayCommand(CanExecute = nameof(CanEdit))]
    private void EditMessage(ChatMessageViewModel? message)
    {
        if (message is null || IsBusy)
            return;

        message.EditText = message.Content;
        message.IsEditing = true;
    }

    [RelayCommand]
    private void CancelEdit(ChatMessageViewModel? message)
    {
        if (message is not null)
            message.IsEditing = false;
    }

    [RelayCommand]
    private async Task SaveEditAsync(ChatMessageViewModel? message)
    {
        if (message is null || IsBusy)
            return;

        var newContent = message.EditText?.Trim() ?? string.Empty;
        message.IsEditing = false;

        if (string.IsNullOrWhiteSpace(newContent))
            return;

        message.Content = newContent;

        var index = Messages.IndexOf(message);
        if (index < 0)
            return;

        // 截断该消息之后的所有消息
        while (Messages.Count > index + 1)
            Messages.RemoveAt(Messages.Count - 1);

        if (message.Role == ChatRole.User)
        {
            // 从该 user 消息重新生成
            var images = message.Images.Count > 0 ? message.Images.ToList() : null;
            await SendCoreAsync(message.Content, images, addUserMessage: false);
        }
        else
        {
            StatusText = "已更新消息";
        }
    }

    private bool CanStartDual() => !IsBusy && !string.IsNullOrWhiteSpace(DualTopic);

    [RelayCommand(CanExecute = nameof(CanStartDual))]
    private async Task StartDualChatAsync()
    {
        var topic = DualTopic?.Trim();
        if (string.IsNullOrEmpty(topic) || IsBusy)
            return;

        var rounds = Math.Clamp(DualRounds, 1, 10);

        _cts = new CancellationTokenSource();
        IsBusy = true;
        StatusText = "双 AI 对话中…";

        try
        {
            var promptA = BuildDualPrompt(topic, DualArgumentA, rounds);
            var promptB = BuildDualPrompt(topic, DualArgumentB, rounds);

            var settingsA = BuildSettings() with { Model = string.IsNullOrWhiteSpace(DualModelA) ? SelectedModel : DualModelA.Trim() };
            var settingsB = BuildSettings() with { Model = string.IsNullOrWhiteSpace(DualModelB) ? SelectedModel : DualModelB.Trim() };

            // 话题作为用户消息展示
            Messages.Add(new ChatMessageViewModel(ChatRole.User, topic));

            // 共享对话历史（不含系统角色）
            var history = new List<ChatMessage> { new(ChatRole.User, topic) };

            for (var round = 0; round < rounds; round++)
            {
                if (_cts.IsCancellationRequested)
                    break;

                var replyA = await GenerateReplyAsync(settingsA, promptA, history, "AI A", _cts.Token);
                if (replyA is null)
                    break;
                history.Add(new ChatMessage(ChatRole.Assistant, replyA));

                if (_cts.IsCancellationRequested)
                    break;

                var replyB = await GenerateReplyAsync(settingsB, promptB, history, "AI B", _cts.Token);
                if (replyB is null)
                    break;
                history.Add(new ChatMessage(ChatRole.Assistant, replyB));
            }

            StatusText = _cts.IsCancellationRequested ? "已停止" : "双 AI 对话结束";
        }
        catch (OperationCanceledException)
        {
            StatusText = "已停止";
        }
        catch (Exception ex)
        {
            StatusText = $"双 AI 对话失败：{ex.Message}";
            logger?.LogError(ex.Message + "\n" + ex.StackTrace);
        }
        finally
        {
            IsBusy = false;
            _cts.Dispose();
            _cts = null;
        }
    }

    private static string BuildDualPrompt(string topic, string argument, int rounds)
    {
        var position = string.IsNullOrWhiteSpace(argument) ? "（未指定，请基于主题自行确定立场）" : argument.Trim();
        return $"你正在参加一场辩论。\n辩论主题：{topic}\n你的立场与论据：{position}\n\n请围绕你的立场展开论述，并回应对方观点。辩论共进行 {rounds} 轮，每轮发言简明扼要、避免重复。";
    }

    /// <summary>生成一条 AI 回复（流式），返回完整文本；无回复返回 null，失败抛异常。</summary>
    private async Task<string?> GenerateReplyAsync(ChatProviderSettings settings, string systemRole, List<ChatMessage> history, string authorName, CancellationToken cancellationToken)
    {
        var request = new List<ChatMessage> { new(ChatRole.System, systemRole) };
        request.AddRange(history);

        var assistant = new ChatMessageViewModel(ChatRole.Assistant, string.Empty, authorName: authorName);
        Messages.Add(assistant);

        var stopwatch = Stopwatch.StartNew();

        var builder = new StringBuilder();
        var thinkingBuilder = new StringBuilder();
        try
        {
            var result = _chatService.ChatAsync(settings, request, cancellationToken);

            await foreach (var chunk in result.Chunks)
            {
                if (chunk.Kind == ChatChunkKind.Thinking)
                {
                    thinkingBuilder.Append(chunk.Text);
                    assistant.Thinking = thinkingBuilder.ToString();
                }
                else
                {
                    builder.Append(chunk.Text);
                    assistant.Content = builder.ToString();
                }
            }

            var usage = await result.Usage;
            if (usage is not null)
            {
                PromptTokens += usage.PromptTokens;
                CompletionTokens += usage.CompletionTokens;
            }

            if (builder.Length == 0)
            {
                assistant.Content = "（无回复）";
                return null;
            }

            return builder.ToString();
        }
        catch
        {
            if (builder.Length == 0)
                assistant.Content = "[请求失败]";
            assistant.IsFailed = true;
            throw;
        }
        finally
        {
            stopwatch.Stop();
            assistant.Elapsed = stopwatch.Elapsed;
            assistant.WordCount = assistant.Content.Length;
        }
    }

    private async Task SendCoreAsync(string text, IReadOnlyList<string>? images, bool addUserMessage = true)
    {
        if (addUserMessage)
            Messages.Add(new ChatMessageViewModel(ChatRole.User, text, images));

        var assistant = new ChatMessageViewModel(ChatRole.Assistant, string.Empty);
        Messages.Add(assistant);

        // 组装上下文：系统提示词 + 历史消息（跳过正在生成的占位消息）
        var history = new List<ChatMessage>();
        if (!string.IsNullOrWhiteSpace(SystemPrompt))
            history.Add(new ChatMessage(ChatRole.System, SystemPrompt));
        foreach (var message in Messages)
        {
            if (message.Role == ChatRole.Assistant && string.IsNullOrEmpty(message.Content))
                continue;
            history.Add(new ChatMessage(message.Role, message.Content, message.Images));
        }

        var stopwatch = Stopwatch.StartNew();

        _cts = new CancellationTokenSource();
        IsBusy = true;
        StatusText = "思考中…";

        var builder = new StringBuilder();
        var thinkingBuilder = new StringBuilder();
        try
        {
            var result = _chatService.ChatAsync(BuildSettings(), history, _cts.Token);

            await foreach (var chunk in result.Chunks)
            {
                if (chunk.Kind == ChatChunkKind.Thinking)
                {
                    thinkingBuilder.Append(chunk.Text);
                    assistant.Thinking = thinkingBuilder.ToString();
                }
                else
                {
                    builder.Append(chunk.Text);
                    assistant.Content = builder.ToString();
                }
            }

            var usage = await result.Usage;
            if (usage is not null)
            {
                PromptTokens += usage.PromptTokens;
                CompletionTokens += usage.CompletionTokens;
            }

            if (builder.Length == 0)
                assistant.Content = "（无回复）";
            StatusText = _cts.IsCancellationRequested ? "已停止" : "完成";
        }
        catch (OperationCanceledException)
        {
            StatusText = "已停止";
        }
        catch (Exception ex)
        {
            StatusText = $"请求失败：{ex.Message}";
            if (builder.Length == 0)
                assistant.Content = $"[请求失败] {ex.Message}";
            assistant.IsFailed = true;
            logger?.LogError(ex.Message + "\n" + ex.StackTrace);
        }
        finally
        {
            stopwatch.Stop();
            assistant.Elapsed = stopwatch.Elapsed;
            assistant.WordCount = assistant.Content.Length;

            IsBusy = false;
            _cts.Dispose();
            _cts = null;
        }
    }

    private bool CanStop() => IsBusy;

    [RelayCommand(CanExecute = nameof(CanStop))]
    private void Stop() => _cts?.Cancel();

    private bool CanPull() => !IsBusy && !IsPulling && !string.IsNullOrWhiteSpace(SelectedModel);

    [RelayCommand(CanExecute = nameof(CanPull))]
    private async Task PullModelAsync()
    {
        var model = SelectedModel?.Trim();
        if (string.IsNullOrEmpty(model) || IsBusy || IsPulling)
            return;

        if (string.IsNullOrWhiteSpace(BaseUrl))
        {
            StatusText = "请先在“连接设置”中填写服务地址";
            return;
        }

        _pullCts = new CancellationTokenSource();
        IsPulling = true;
        StatusText = $"开始下载模型 {model} …";

        try
        {
            await foreach (var progress in _chatService.PullModelAsync(BuildSettings(), model, _pullCts.Token))
            {
                StatusText = $"下载 {model}：{progress.Percent:0.0}% — {progress.Status}";
            }

            StatusText = $"模型 {model} 下载完成";
            await RefreshModelsAsync();
        }
        catch (OperationCanceledException)
        {
            StatusText = "已取消下载";
        }
        catch (Exception ex)
        {
            StatusText = $"下载失败：{ex.Message}";
            logger?.LogError(ex.Message + "\n" + ex.StackTrace);
        }
        finally
        {
            IsPulling = false;
            _pullCts.Dispose();
            _pullCts = null;
        }
    }

    private bool CanCancelPull() => IsPulling;

    [RelayCommand(CanExecute = nameof(CanCancelPull))]
    private void CancelPull() => _pullCts?.Cancel();

    [RelayCommand]
    private void Clear()
    {
        Messages.Clear();
        StatusText = "已清空对话";
    }

    /// <summary>选择图片后，将文件路径加入待发送队列（内存中只保存路径）。</summary>
    public void AttachImages(IEnumerable<string> paths)
    {
        foreach (var path in paths)
        {
            if (File.Exists(path))
                PendingImagePaths.Add(path);
        }

        SendCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void RemovePendingImage(string path)
    {
        PendingImagePaths.Remove(path);
        SendCommand.NotifyCanExecuteChanged();
    }

    /// <summary>导出当前对话到指定 .hchat 文件。</summary>
    public void ExportChat(string path)
    {
        try
        {
            var document = new ChatSessionDocument
            {
                SystemPrompt = SystemPrompt,
                Messages = Messages
                    .Where(m => !(m.Role == ChatRole.Assistant && string.IsNullOrEmpty(m.Content)))
                    .Select(m => new ChatMessageDto
                    {
                        Role = m.Role,
                        Content = m.Content,
                        Thinking = string.IsNullOrEmpty(m.Thinking) ? null : m.Thinking,
                        Images = m.Images.Count > 0 ? m.Images.ToList() : null,
                    })
                    .ToList(),
            };

            _chatSessionService.Save(document, path);
            StatusText = $"已导出 {document.Messages.Count} 条消息";
        }
        catch (Exception ex)
        {
            StatusText = $"导出失败：{ex.Message}";
            logger?.LogError(ex.Message + "\n" + ex.StackTrace);
        }
    }

    /// <summary>从指定 .hchat 文件导入对话。</summary>
    public void ImportChat(string path)
    {
        try
        {
            var document = _chatSessionService.Load(path);
            if (document is null)
            {
                StatusText = "导入失败：文件内容无效";
                return;
            }

            Messages.Clear();
            if (!string.IsNullOrWhiteSpace(document.SystemPrompt))
                SystemPrompt = document.SystemPrompt;

            foreach (var message in document.Messages)
            {
                Messages.Add(new ChatMessageViewModel(message.Role, message.Content, message.Images, thinking: message.Thinking));
            }

            StatusText = $"已导入 {document.Messages.Count} 条消息";
        }
        catch (Exception ex)
        {
            StatusText = $"导入失败：{ex.Message}";
            logger?.LogError(ex.Message + "\n" + ex.StackTrace);
        }
    }

    [RelayCommand]
    private async Task RefreshModelsAsync()
    {
        if (string.IsNullOrWhiteSpace(BaseUrl))
        {
            StatusText = "请先在“连接设置”中填写服务地址";
            return;
        }

        try
        {
            StatusText = "正在获取模型列表…";
            var models = await _chatService.GetModelsAsync(BuildSettings());

            Models.Clear();
            foreach (var model in models.Order())
                Models.Add(model);

            if (Models.Count > 0 && !Models.Contains(SelectedModel))
                SelectedModel = Models[0];

            StatusText = Models.Count > 0 ? $"共 {Models.Count} 个模型" : "未获取到模型，可手动输入模型名";
        }
        catch (Exception ex)
        {
            StatusText = $"获取模型列表失败：{ex.Message}";
            logger?.LogError(ex.Message + "\n" + ex.StackTrace);
        }
    }

    private static string LoadSystemPrompt()
    {
        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Prompts", "base.md");
            if (File.Exists(path))
                return File.ReadAllText(path);
        }
        catch
        {
            // 忽略提示词加载失败
        }

        return string.Empty;
    }

    private static string LoadDeepSeekKey()
    {
        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Prompts", "deepsek.sk");
            if (File.Exists(path))
                return File.ReadAllText(path);
        }
        catch
        {
            // 忽略SK加载失败
        }

        return string.Empty;
    }
}
