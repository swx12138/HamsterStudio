using HamsterStudio.ChatBox.Models;

namespace HamsterStudio.ChatBox.Services;

/// <summary>
/// 聊天后端提供者抽象。
/// 后续要接入新的后端（DeepSeek / OpenAI / 其它兼容服务）时，实现此接口并注册即可。
/// </summary>
public interface IChatProvider
{
    /// <summary>获取可用模型列表。</summary>
    Task<IReadOnlyList<string>> GetModelsAsync(ChatProviderSettings settings, CancellationToken cancellationToken = default);

    /// <summary>以流式方式返回一段回复，并附带 token 使用统计。</summary>
    ChatStreamResult ChatAsync(ChatProviderSettings settings, IReadOnlyList<ChatMessage> messages, CancellationToken cancellationToken = default);
}
