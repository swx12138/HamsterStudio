using HamsterStudio.ChatBox.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace HamsterStudio.ChatBox.Services;

/// <summary>聊天服务：根据配置把请求路由到 Ollama 或 OpenAI 兼容后端。</summary>
public class ChatService
{
    private readonly IChatProvider _ollama;
    private readonly IChatProvider _openAi;
    private readonly ILogger<ChatService>? _logger;

    public ChatService(HttpClient http, ILogger<ChatService>? logger)
    {
        // 流式回复耗时可能较长，放宽超时时间。
        http.Timeout = TimeSpan.FromMinutes(30);

        _ollama = new OllamaChatProvider();
        _openAi = new OpenAiChatProvider(http);
        _logger = logger;
    }

    private IChatProvider Resolve(ChatProviderKind kind) => kind switch
    {
        ChatProviderKind.Ollama => _ollama,
        // OpenAI 与 DeepSeek 共用 OpenAI 兼容接口
        _ => _openAi,
    };

    public Task<IReadOnlyList<string>> GetModelsAsync(ChatProviderSettings settings, CancellationToken cancellationToken = default)
        => Resolve(settings.Kind).GetModelsAsync(settings, cancellationToken);

    public IAsyncEnumerable<string> ChatAsync(ChatProviderSettings settings, IReadOnlyList<ChatMessage> messages, CancellationToken cancellationToken = default)
        => Resolve(settings.Kind).ChatAsync(settings, messages, cancellationToken);
}
