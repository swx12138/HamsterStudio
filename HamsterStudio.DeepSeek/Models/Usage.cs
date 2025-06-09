using System.Text.Json.Serialization;

namespace HamsterStudio.DeepSeek.Models;

public class ReasonerChatRequest
{
    public string model { get; set; } = "deepseek-reasoner";
    public List<ChatMessage> messages { get; set; }
    public bool stream { get; set; } = true; // 必须设置为 true
    public bool reasoning { get; set; } = true; // 关键参数
}

public class ChatResponse
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("choices")]
    public List<ChatChoice> Choices { get; set; } = [];

    [JsonPropertyName("created")]
    public long Created { get; set; } // 时间戳

    [JsonPropertyName("model")]
    public string Model { get; set; }

    [JsonPropertyName("system_fingerprint")]
    public string SystemFingerprint { get; set; } = string.Empty;

    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;

    [JsonPropertyName("usage")]
    public Usage Usage { get; set; }
}

public class ChatChoice
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    public Delta delta { get; set; }

    public ChatMessage message { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; }


    [JsonPropertyName("finish_reason")]
    public string FinishReason { get; set; }
}

public class Delta
{
    public string role { get; set; } // 仅在第一个块出现
    public string content { get; set; } // 增量内容
    public string reasoning_content { get; set; } // 增量内容
}

public class ChatRequest
{
    public string model { get; set; } = "deepseek-chat";
    public List<ChatMessage> messages { get; set; }
    public bool stream { get; set; } = false;
}

public class DataItem
{
    /// <summary>
    /// 
    /// </summary>
    public string id { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string @object { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string owned_by { get; set; }
}

public class DeepSeekModels
{
    public string @object { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public List<DataItem> Models { get; set; } = [];
}

public class ChatMessage
{
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("role")]
    public string? Role { get; set; }
}

public class ResponseFormat
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }
}

public class ChatRequestRoot
{
    [JsonPropertyName("messages")]
    public List<ChatMessage> Messages { get; set; } = [];

    [JsonPropertyName("model")]
    public string Model { get; set; } = "deepseek-reasoner";

    [JsonPropertyName("frequency_penalty")]
    public int? FrequencyPenalty { get; set; }

    [JsonPropertyName("max_tokens")]
    public int? MaxTokens { get; set; }

    [JsonPropertyName("presence_penalty")]
    public int? PresencePenalty { get; set; }

    [JsonPropertyName("response_format")]
    public ResponseFormat? ResponseFormat { get; set; }

    [JsonPropertyName("stop")]
    public string? Stop { get; set; }

    [JsonPropertyName("stream")]
    public bool? Stream { get; set; }

    [JsonPropertyName("stream_options")]
    public string? StreamOptions { get; set; }

    [JsonPropertyName("temperature")]
    public int? Temperature { get; set; }

    [JsonPropertyName("top_p")]
    public int? TopP { get; set; }

    [JsonPropertyName("tools")]
    public string? Tools { get; set; }

    [JsonPropertyName("tool_choice")]
    public string? ToolChoice { get; set; }

    [JsonPropertyName("logprobs")]
    public string? Logprobs { get; set; }

    [JsonPropertyName("top_logprobs")]
    public string? TopLogprobs { get; set; }
}

public class FimChatRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = "deepseek-chat";

    [JsonPropertyName("prompt")]
    public required string Prompt { get; set; }

    [JsonPropertyName("max_tokens")]
    public int MaxTokens { get; set; } = 1024;

    [JsonPropertyName("stream")]
    public bool Stream { get; set; } = true;

    [JsonPropertyName("suffix")]
    public string? Suffix { get; set; } = null;

}

public class PromptTokensDetails
{
    [JsonPropertyName("cached_tokens")]
    public int CachedTokens { get; set; }
}

public class Usage
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }

    [JsonPropertyName("prompt_tokens_details")]
    public PromptTokensDetails PromptTokensDetails { get; set; } = new();

    [JsonPropertyName("prompt_cache_hit_tokens")]
    public int PromptCacheHitTokens { get; set; }

    [JsonPropertyName("prompt_cache_miss_tokens")]
    public int PromptCacheMissTokens { get; set; }
}
