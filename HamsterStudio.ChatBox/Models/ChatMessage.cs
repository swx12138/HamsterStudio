namespace HamsterStudio.ChatBox.Models;

/// <summary>消息角色。</summary>
public enum ChatRole
{
    System,
    User,
    Assistant,
}

/// <summary>一条对话消息。Images 为可选的图片 data URL 列表（多模态）。</summary>
public record ChatMessage(ChatRole Role, string Content, IReadOnlyList<string>? Images = null)
{
    /// <summary>转换为后端 API 使用的角色名。</summary>
    public string RoleText => Role switch
    {
        ChatRole.System => "system",
        ChatRole.User => "user",
        ChatRole.Assistant => "assistant",
        _ => "user",
    };
}
