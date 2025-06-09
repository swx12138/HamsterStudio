using HamsterStudio.DeepSeek.Models;
using HamsterStudio.DeepSeek.Services.Restful;
using Refit;
using System.Data.SqlTypes;
using System.Net.Http.Headers;
using System.Text.Json;

namespace HamsterStudio.DeepSeek.Services;

public class ReasoningFlag
{
    private bool flag = true;
    public bool Flag { get => flag; set { flag = value; } }
}

class MyHttpMessageHandler(HttpMessageHandler innerHandler) : DelegatingHandler(innerHandler)
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        //// 打印请求方法和URL
        //Console.WriteLine($"Request: {request.Method} {request.RequestUri}");

        //// 打印请求头
        //foreach (var header in request.Headers)
        //{
        //    Console.WriteLine($"Header: {header.Key} = {string.Join(", ", header.Value)}");
        //}

        //// 打印请求体（如果有）
        //if (request.Content != null)
        //{
        //    var body = await request.Content.ReadAsStringAsync();
        //    Console.WriteLine($"Body: {body}");
        //}

        return await base.SendAsync(request, cancellationToken);
    }
}

public class DeepSeekClient
{
    private IDeepSeekApi deepSeekApi;
    private IDeepSeekBetaApi deepSeekBetaApi;

    public DeepSeekClient(string apiKey, string baseUrl = "https://api.deepseek.com")
    {
        {
            var httpClient2 = new HttpClient(new MyHttpMessageHandler(new HttpClientHandler()))
            {
                BaseAddress = new Uri(baseUrl + "/beta"),
                Timeout = Timeout.InfiniteTimeSpan
            };

            // 设置默认请求头
            httpClient2.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            deepSeekBetaApi = RestService.For<IDeepSeekBetaApi>(httpClient2);
        }

        {
            var httpClient = new HttpClient(new MyHttpMessageHandler(new HttpClientHandler()))
            {
                BaseAddress = new Uri(baseUrl),
                Timeout = Timeout.InfiniteTimeSpan
            };

            // 设置默认请求头
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            deepSeekApi = RestService.For<IDeepSeekApi>(httpClient);
        }
    }

    public async Task<ChatResponse> GetChatCompletionAsync(List<ChatMessage> messages)
    {
        var request = new ChatRequestRoot
        {
            Messages = messages,
            Stream = false
        };
        return await deepSeekApi.GetResponseAsync(request);
    }

    public async IAsyncEnumerable<string> GetStreamingResponse(List<ChatMessage> messages, ReasoningFlag reasoning)
    {
        var request = new ChatRequestRoot
        {
            Messages = messages,
            Stream = true
        };

        using var stream = await deepSeekApi.GetStreamingResponseAsync(request);
        await foreach (var resp in ProcessReasoningStream(stream, reasoning))
        {
            yield return resp;
        }
    }

    public async IAsyncEnumerable<string> FillInTheMiddle(string prompt, string? s = null)
    {
        var request = new FimChatRequest
        {
            Prompt = prompt,
            Suffix = s,
            MaxTokens = 200,
        };

        using var stream = await deepSeekBetaApi.QuestFillInTheMiddle(request);
        await foreach (var resp in ProcessStream(stream))
        {
            yield return resp;
        }
    }

    private async IAsyncEnumerable<string> ProcessReasoningStream(Stream stream, ReasoningFlag reasoning)
    {
        await foreach (var chunk in ProcessStreamAbstract<ChatResponse>(stream))
        {
            var delta = chunk?.Choices?.FirstOrDefault()?.delta;

            // 输出思维链标记内容
            if (delta?.reasoning_content != null)
            {
                reasoning.Flag = true;
                yield return delta.reasoning_content;
            }

            if (delta?.content != null)
            {
                reasoning.Flag = false;
                yield return delta.content;
            }

            // 处理结束原因
            if (!string.IsNullOrEmpty(chunk?.Choices?.FirstOrDefault()?.FinishReason))
            {
                reasoning.Flag = true;
                yield return $"\n[结束原因] {chunk.Choices[0].FinishReason}";
            }

        }
    }

    private async IAsyncEnumerable<string> ProcessStream(Stream stream)
    {
        await foreach (var chunk in ProcessStreamAbstract<ChatResponse>(stream))
        {
            string? text = chunk?.Choices?.FirstOrDefault()?.Text;
            if (text != null)
            {
                yield return text;
            }

            // 处理结束原因
            string? finishReason = chunk?.Choices?.FirstOrDefault()?.FinishReason;
            if (!string.IsNullOrEmpty(finishReason))
            {
                yield return $"\n[结束原因] {finishReason}";
            }
        }
    }

    private async IAsyncEnumerable<T> ProcessStreamAbstract<T>(Stream stream)
    {
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;

            // 处理 SSE 格式：data: {...}
            if (line.StartsWith("data:"))
            {
                var json = line["data:".Length..].Trim();

                // 流结束标记
                if (json == "[DONE]") yield break;

                var chunk = JsonSerializer.Deserialize<T>(json);
                if (chunk == null)
                {
                    throw new FormatException(json);
                }
                else
                {
                    yield return chunk;
                }
            }
        }
    }

}