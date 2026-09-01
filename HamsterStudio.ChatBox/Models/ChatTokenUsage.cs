namespace HamsterStudio.ChatBox.Models;

/// <summary>一次对话请求的 token 使用统计。</summary>
public record ChatTokenUsage(long PromptTokens, long CompletionTokens)
{
    public long TotalTokens => PromptTokens + CompletionTokens;
}

/// <summary>流式块的类型。</summary>
public enum ChatChunkKind
{
    /// <summary>正常回复文本。</summary>
    Text,

    /// <summary>模型的思考（推理）过程。</summary>
    Thinking,
}

/// <summary>流式对话的一个块。</summary>
public record ChatStreamChunk(ChatChunkKind Kind, string Text);

/// <summary>流式对话结果：文本/思考流 + 完成后可用的 token 统计。</summary>
public record ChatStreamResult(IAsyncEnumerable<ChatStreamChunk> Chunks, Task<ChatTokenUsage?> Usage);
