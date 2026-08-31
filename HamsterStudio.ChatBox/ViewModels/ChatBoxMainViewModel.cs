using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Barefeet.MVVM;
using HamsterStudio.ChatBox.Models;
using HamsterStudio.ChatBox.Services;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace HamsterStudio.ChatBox.ViewModels;

public partial class ChatBoxMainViewModel : ViewModel
{
    private readonly ChatService _chatService;
    private readonly ChatSessionService _chatSessionService;
    private CancellationTokenSource? _cts;
    private CancellationTokenSource? _pullCts;

    public ObservableCollection<ChatMessageViewModel> Messages { get; } = [];
    public ObservableCollection<string> Models { get; } = [];

    /// <summary>待发送的图片 data URL 列表（多模态）。</summary>
    public ObservableCollection<string> PendingImageDataUrls { get; } = [];

    public IReadOnlyList<ChatProviderKind> ProviderKinds { get; } = Enum.GetValues<ChatProviderKind>();

    [ObservableProperty]
    private ChatProviderKind _selectedProvider = ChatProviderKind.Ollama;

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

    public ChatBoxMainViewModel(ChatService chatService, ChatSessionService chatSessionService, ILogger<ChatBoxMainViewModel>? logger) : base(logger)
    {
        _chatService = chatService;
        _chatSessionService = chatSessionService;
    }

    private ChatProviderSettings BuildSettings() => new()
    {
        Kind = SelectedProvider,
        BaseUrl = BaseUrl?.Trim() ?? string.Empty,
        ApiKey = ApiKey?.Trim() ?? string.Empty,
        Model = SelectedModel?.Trim() ?? string.Empty,
    };

    partial void OnSelectedProviderChanged(ChatProviderKind value)
    {
        (BaseUrl, ApiKey, SelectedModel) = value switch
        {
            ChatProviderKind.Ollama => ("http://localhost:11434", string.Empty, "llama3.2"),
            ChatProviderKind.OpenAI => ("https://api.openai.com/v1", string.Empty, "gpt-4o-mini"),
            ChatProviderKind.DeepSeek => ("https://api.deepseek.com", string.Empty, "deepseek-chat"),
            _ => (string.Empty, string.Empty, string.Empty),
        };

        Models.Clear();
        _ = RefreshModelsAsync();
    }

    partial void OnInputTextChanged(string value) => SendCommand.NotifyCanExecuteChanged();

    partial void OnSelectedModelChanged(string value) => PullModelCommand.NotifyCanExecuteChanged();

    partial void OnIsBusyChanged(bool value)
    {
        SendCommand.NotifyCanExecuteChanged();
        StopCommand.NotifyCanExecuteChanged();
        PullModelCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsPullingChanged(bool value)
    {
        PullModelCommand.NotifyCanExecuteChanged();
        CancelPullCommand.NotifyCanExecuteChanged();
    }

    private bool CanSend() => !IsBusy && (!string.IsNullOrWhiteSpace(InputText) || PendingImageDataUrls.Count > 0);

    [RelayCommand(CanExecute = nameof(CanSend))]
    private async Task SendAsync()
    {
        var text = InputText?.Trim();
        if ((string.IsNullOrEmpty(text) && PendingImageDataUrls.Count == 0) || IsBusy)
            return;

        if (string.IsNullOrWhiteSpace(BaseUrl))
        {
            StatusText = "请先在“连接设置”中填写服务地址";
            return;
        }

        InputText = string.Empty;

        var images = PendingImageDataUrls.Count > 0 ? PendingImageDataUrls.ToList() : null;
        Messages.Add(new ChatMessageViewModel(ChatRole.User, text ?? string.Empty, images));
        PendingImageDataUrls.Clear();

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

        _cts = new CancellationTokenSource();
        IsBusy = true;
        StatusText = "思考中…";

        var builder = new StringBuilder();
        try
        {
            await foreach (var chunk in _chatService.ChatAsync(BuildSettings(), history, _cts.Token))
            {
                builder.Append(chunk);
                assistant.Content = builder.ToString();
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
        }
        finally
        {
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

    /// <summary>选择图片后，将其转为 data URL 加入待发送队列。</summary>
    public void AttachImages(IEnumerable<string> paths)
    {
        foreach (var path in paths)
        {
            var dataUrl = BuildDataUrl(path);
            if (dataUrl is not null)
                PendingImageDataUrls.Add(dataUrl);
        }

        SendCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void RemovePendingImage(string dataUrl)
    {
        PendingImageDataUrls.Remove(dataUrl);
        SendCommand.NotifyCanExecuteChanged();
    }

    private static string? BuildDataUrl(string path)
    {
        try
        {
            var bytes = File.ReadAllBytes(path);
            var mime = Path.GetExtension(path).ToLowerInvariant() switch
            {
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".bmp" => "image/bmp",
                _ => "image/jpeg",
            };
            return $"data:{mime};base64,{Convert.ToBase64String(bytes)}";
        }
        catch
        {
            return null;
        }
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
                Messages.Add(new ChatMessageViewModel(message.Role, message.Content, message.Images));
            }

            StatusText = $"已导入 {document.Messages.Count} 条消息";
        }
        catch (Exception ex)
        {
            StatusText = $"导入失败：{ex.Message}";
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
            foreach (var model in models)
                Models.Add(model);

            if (Models.Count > 0 && !Models.Contains(SelectedModel))
                SelectedModel = Models[0];

            StatusText = Models.Count > 0 ? $"共 {Models.Count} 个模型" : "未获取到模型，可手动输入模型名";
        }
        catch (Exception ex)
        {
            StatusText = $"获取模型列表失败：{ex.Message}";
        }
    }

    private static string LoadSystemPrompt()
    {
        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Prompts", "base.txt");
            if (File.Exists(path))
                return File.ReadAllText(path);
        }
        catch
        {
            // 忽略提示词加载失败
        }

        return string.Empty;
    }
}
