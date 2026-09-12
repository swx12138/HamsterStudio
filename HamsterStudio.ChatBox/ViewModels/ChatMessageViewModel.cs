using CommunityToolkit.Mvvm.ComponentModel;
using HamsterStudio.ChatBox.Models;

namespace HamsterStudio.ChatBox.ViewModels;

/// <summary>用于界面展示的一条消息。</summary>
public partial class ChatMessageViewModel : ObservableObject
{
    private readonly string? _authorName;

    public ChatRole Role { get; }

    public bool IsUser => Role == ChatRole.User;

    public bool IsAssistant => Role == ChatRole.Assistant;

    public string Author => _authorName ?? Role switch
    {
        ChatRole.User => "我",
        ChatRole.Assistant => "AI",
        _ => "系统",
    };

    /// <summary>图片文件路径列表（多模态）。</summary>
    public IReadOnlyList<string> Images { get; }

    /// <summary>请求是否失败（失败时显示重试按钮）。</summary>
    [ObservableProperty]
    private bool _isFailed;

    /// <summary>是否处于编辑状态。</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotEditing))]
    private bool _isEditing;

    /// <summary>是否未处于编辑状态（用于显示内容与操作按钮）。</summary>
    public bool IsNotEditing => !IsEditing;

    /// <summary>编辑中的文本。</summary>
    [ObservableProperty]
    private string _editText = string.Empty;

    /// <summary>思考（推理）过程内容。</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasThinking))]
    private string _thinking = string.Empty;

    public bool HasThinking => !string.IsNullOrEmpty(Thinking);

    [ObservableProperty]
    private string _content;

    /// <summary>回复用时。</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MetaText))]
    private TimeSpan? _elapsed;

    /// <summary>字数（字符数）。</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MetaText))]
    private int _wordCount;

    /// <summary>消息下方的元信息：用时与字数。</summary>
    public string MetaText
    {
        get
        {
            var parts = new List<string>();
            if (Elapsed is { } elapsed)
                parts.Add($"用时 {FormatElapsed(elapsed)}");
            if (WordCount > 0)
                parts.Add($"{WordCount} 字");
            return string.Join(" · ", parts);
        }
    }

    private static string FormatElapsed(TimeSpan t) => t.TotalSeconds < 60
        ? $"{t.TotalSeconds:0.0} 秒"
        : $"{(int)t.TotalMinutes} 分 {t.Seconds} 秒";

    public ChatMessageViewModel(ChatRole role, string content, IReadOnlyList<string>? images = null, string? authorName = null, string? thinking = null)
    {
        Role = role;
        _content = content;
        Images = images ?? Array.Empty<string>();
        _authorName = authorName;
        _thinking = thinking ?? string.Empty;
    }
}
