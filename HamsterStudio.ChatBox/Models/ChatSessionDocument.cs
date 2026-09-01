namespace HamsterStudio.ChatBox.Models;

/// <summary>用于序列化的一条消息。</summary>
public class ChatMessageDto
{
    public ChatRole Role { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? Thinking { get; set; }
    public List<string>? Images { get; set; }
}

/// <summary>对话会话文档（导出到 .hchat 文件的数据结构）。</summary>
public class ChatSessionDocument
{
    public string? SystemPrompt { get; set; }
    public List<ChatMessageDto> Messages { get; set; } = [];
}
