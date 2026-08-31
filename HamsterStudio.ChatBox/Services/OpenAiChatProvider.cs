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

    public async IAsyncEnumerable<string> ChatAsync(
        ChatProviderSettings settings,
        IReadOnlyList<ChatMessage> messages,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            model = settings.Model,
            messages = messages.Select(m => new { role = m.RoleText, content = m.Content }).ToArray(),
            stream = true,
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
                yield break;
            if (string.IsNullOrWhiteSpace(line))
                continue;
            if (!line.StartsWith("data:", StringComparison.Ordinal))
                continue;

            var data = line["data:".Length..].Trim();
            if (data == "[DONE]")
                yield break;

            string text;
            try
            {
                text = ExtractDeltaContent(data);
            }
            catch (JsonException)
            {
                // 忽略无法解析的行（如 SSE 的 keep-alive 注释）
                continue;
            }

            if (!string.IsNullOrEmpty(text))
                yield return text;
        }
    }

    private static string ExtractDeltaContent(string data)
    {
        var builder = new StringBuilder();
        using var doc = JsonDocument.Parse(data);
        if (doc.RootElement.TryGetProperty("choices", out var choices))
        {
            foreach (var choice in choices.EnumerateArray())
            {
                if (choice.TryGetProperty("delta", out var delta) &&
                    delta.TryGetProperty("content", out var content) &&
                    content.GetString() is { } text)
                {
                    builder.Append(text);
                }
            }
        }

        return builder.ToString();
    }
}
