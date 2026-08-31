namespace HamsterStudio.ChatBox.Models;

/// <summary>一次对话请求的 token 使用统计。</summary>
public record ChatTokenUsage(long PromptTokens, long CompletionTokens)
{
    public long TotalTokens => PromptTokens + CompletionTokens;
}

/// <summary>流式对话结果：文本流 + 完成后可用的 token 统计。</summary>
public record ChatStreamResult(IAsyncEnumerable<string> Chunks, Task<ChatTokenUsage?> Usage);
