namespace HamsterStudio.ChatBox.Models;

/// <summary>支持的聊天后端类型。</summary>
public enum ChatProviderKind
{
    Ollama,
    OpenAI,
    DeepSeek,
}

/// <summary>聊天后端的连接配置。</summary>
public record ChatProviderSettings
{
    public ChatProviderKind Kind { get; init; } = ChatProviderKind.Ollama;

    /// <summary>服务地址，例如 http://localhost:11434 或 https://api.deepseek.com。</summary>
    public string BaseUrl { get; init; } = "http://localhost:11434";

    /// <summary>OpenAI / DeepSeek 等云服务的 API Key，Ollama 可留空。</summary>
    public string ApiKey { get; init; } = string.Empty;

    public string Model { get; init; } = string.Empty;
}
