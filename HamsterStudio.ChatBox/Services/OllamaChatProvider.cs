using System.Runtime.CompilerServices;
using HamsterStudio.ChatBox.Models;
using OllamaSharp;
using OllamaSharpChatDone = OllamaSharp.Models.Chat.ChatDoneResponseStream;
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

    public ChatStreamResult ChatAsync(ChatProviderSettings settings, IReadOnlyList<ChatMessage> messages, CancellationToken cancellationToken = default)
    {
        var usage = new TaskCompletionSource<ChatTokenUsage?>(TaskCreationOptions.RunContinuationsAsynchronously);
        return new ChatStreamResult(StreamAsync(settings, messages, usage, cancellationToken), usage.Task);
    }

    private async IAsyncEnumerable<string> StreamAsync(
        ChatProviderSettings settings,
        IReadOnlyList<ChatMessage> messages,
        TaskCompletionSource<ChatTokenUsage?> usage,
        [EnumeratorCancellation] CancellationToken cancellationToken)
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
            if (response is OllamaSharpChatDone done)
                usage.TrySetResult(new ChatTokenUsage(done.PromptEvalCount, done.EvalCount));

            var text = response?.Message?.Content;
            if (!string.IsNullOrEmpty(text))
                yield return text;
        }

        usage.TrySetResult(null);
    }

    public async IAsyncEnumerable<ModelPullProgress> PullModelAsync(
        ChatProviderSettings settings,
        string model,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var ollama = new OllamaApiClient(settings.BaseUrl);

        await foreach (var status in ollama.PullModelAsync(model, cancellationToken))
        {
            if (status is null)
                continue;
            yield return new ModelPullProgress(status.Status, status.Percent);
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
        message.Content,
        message.Images?.Select(ImageFileHelper.ReadAsBase64).ToArray());
}
