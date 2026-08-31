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
    private CancellationTokenSource? _cts;

    public ObservableCollection<ChatMessageViewModel> Messages { get; } = [];
    public ObservableCollection<string> Models { get; } = [];
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
    private string _baseUrl = "http://127.0.0.1:11434";

    [ObservableProperty]
    private string _apiKey = string.Empty;

    [ObservableProperty]
    private string _statusText = "就绪";

    [ObservableProperty]
    private string _systemPrompt = LoadSystemPrompt();

    public ChatBoxMainViewModel(ChatService chatService, ILogger<ChatBoxMainViewModel>? logger) : base(logger)
    {
        _chatService = chatService;
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

    partial void OnIsBusyChanged(bool value)
    {
        SendCommand.NotifyCanExecuteChanged();
        StopCommand.NotifyCanExecuteChanged();
    }

    private bool CanSend() => !IsBusy && !string.IsNullOrWhiteSpace(InputText);

    [RelayCommand(CanExecute = nameof(CanSend))]
    private async Task SendAsync()
    {
        var text = InputText?.Trim();
        if (string.IsNullOrEmpty(text) || IsBusy)
            return;

        if (string.IsNullOrWhiteSpace(BaseUrl))
        {
            StatusText = "请先在“连接设置”中填写服务地址";
            return;
        }

        InputText = string.Empty;

        Messages.Add(new ChatMessageViewModel(ChatRole.User, text));
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
            history.Add(new ChatMessage(message.Role, message.Content));
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

    [RelayCommand]
    private void Clear()
    {
        Messages.Clear();
        StatusText = "已清空对话";
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
