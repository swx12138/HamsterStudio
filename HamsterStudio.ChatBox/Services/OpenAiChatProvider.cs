using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using HamsterStudio.ChatBox.Models;

namespace HamsterStudio.ChatBox.Services;

/// <summary>
/// OpenAI 兼容后端。OpenAI 与 DeepSeek 均使用 OpenAI 风格的 Chat Completions 接口，
/// 仅服务地址与模型名不同。
/// </summary>
public class OpenAiChatProvider(HttpClient http) : IChatProvider
{
    private static HttpRequestMessage BuildRequest(ChatProviderSettings settings, HttpMethod method, string path)
    {
        var url = $"{settings.BaseUrl.TrimEnd('/')}/{path.TrimStart('/')}";
        var request = new HttpRequestMessage(method, url);

        if (!string.IsNullOrWhiteSpace(settings.ApiKey))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);

        return request;
    }

    public async Task<IReadOnlyList<string>> GetModelsAsync(ChatProviderSettings settings, CancellationToken cancellationToken = default)
    {
        using var request = BuildRequest(settings, HttpMethod.Get, "models");
        using var response = await http.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        var result = new List<string>();
        if (doc.RootElement.TryGetProperty("data", out var data))
        {
            foreach (var item in data.EnumerateArray())
            {
                if (item.TryGetProperty("id", out var id) && id.GetString() is { } n)
                    result.Add(n);
            }
        }

        return result;
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
        var payload = new
        {
            model = settings.Model,
            messages = messages.Select(m => new { role = m.RoleText, content = BuildContent(m) }).ToArray(),
            stream = true,
            stream_options = new { include_usage = true },
        };

        using var request = BuildRequest(settings, HttpMethod.Post, "chat/completions");
        request.Content = JsonContent.Create(payload);

        using var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (!cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (line is null)
                break;
            if (string.IsNullOrWhiteSpace(line))
                continue;
            if (!line.StartsWith("data:", StringComparison.Ordinal))
                continue;

            var data = line["data:".Length..].Trim();
            if (data == "[DONE]")
                break;

            string? text;
            try
            {
                text = ParseChunk(data, usage);
            }
            catch (JsonException)
            {
                // 忽略无法解析的行（如 SSE 的 keep-alive 注释）
                continue;
            }

            if (!string.IsNullOrEmpty(text))
                yield return text;
        }

        usage.TrySetResult(null);
    }

    private static string? ParseChunk(string data, TaskCompletionSource<ChatTokenUsage?> usage)
    {
        using var doc = JsonDocument.Parse(data);
        var root = doc.RootElement;

        // usage 信息（需要 stream_options.include_usage）
        if (root.TryGetProperty("usage", out var usageEl))
        {
            var prompt = usageEl.TryGetProperty("prompt_tokens", out var p) ? p.GetInt64() : 0L;
            var completion = usageEl.TryGetProperty("completion_tokens", out var c) ? c.GetInt64() : 0L;
            usage.TrySetResult(new ChatTokenUsage(prompt, completion));
        }

        // 增量文本（默认单 choice）
        if (root.TryGetProperty("choices", out var choices))
        {
            foreach (var choice in choices.EnumerateArray())
            {
                if (choice.TryGetProperty("delta", out var delta) &&
                    delta.TryGetProperty("content", out var content) &&
                    content.GetString() is { } text &&
                    !string.IsNullOrEmpty(text))
                {
                    return text;
                }
            }
        }

        return null;
    }

    private static object BuildContent(ChatMessage message)
    {
        // OpenAI 视觉接口仅在 user 消息中支持 content 数组格式
        if (message.Images is { Count: > 0 } && message.Role == ChatRole.User)
        {
            var parts = new List<object>
            {
                new { type = "text", text = message.Content },
            };
            parts.AddRange(message.Images.Select(path => new
            {
                type = "image_url",
                image_url = new { url = ImageFileHelper.ReadAsDataUrl(path) },
            }));
            return parts;
        }

        return message.Content;
    }
}
