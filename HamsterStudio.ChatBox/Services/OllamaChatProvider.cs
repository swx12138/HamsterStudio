using System.Runtime.CompilerServices;
using HamsterStudio.ChatBox.Models;
using OllamaSharp;
using OllamaSharpChatRequest = OllamaSharp.Models.Chat.ChatRequest;
using OllamaSharpChatRole = OllamaSharp.Models.Chat.ChatRole;
using OllamaSharpMessage = OllamaSharp.Models.Chat.Message;

namespace HamsterStudio.ChatBox.Services;

/// <summary>Ollama 本地后端，基于 OllamaSharp 实现。</summary>
public class OllamaChatProvider : IChatProvider
{
    public async Task<IReadOnlyList<string>> GetModelsAsync(ChatProviderSettings settings, CancellationToken cancellationToken = default)
    {
        using var ollama = new OllamaApiClient(settings.BaseUrl);
        var models = await ollama.ListLocalModelsAsync(cancellationToken);
        return models.Select(m => m.Name).ToList();
    }

    public async IAsyncEnumerable<string> ChatAsync(
        ChatProviderSettings settings,
        IReadOnlyList<ChatMessage> messages,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var ollama = new OllamaApiClient(settings.BaseUrl);

        var request = new OllamaSharpChatRequest
        {
            Model = settings.Model,
            Stream = true,
            Messages = messages.Select(ToOllamaMessage).ToArray(),
        };

        await foreach (var response in ollama.ChatAsync(request, cancellationToken))
        {
            var text = response?.Message?.Content;
            if (!string.IsNullOrEmpty(text))
                yield return text;
        }
    }

    private static OllamaSharpMessage ToOllamaMessage(ChatMessage message) => new(
        message.Role switch
        {
            ChatRole.System => OllamaSharpChatRole.System,
            ChatRole.User => OllamaSharpChatRole.User,
            ChatRole.Assistant => OllamaSharpChatRole.Assistant,
            _ => OllamaSharpChatRole.User,
        },
        message.Content);
}
