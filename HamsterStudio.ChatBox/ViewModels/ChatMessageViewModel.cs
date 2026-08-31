using CommunityToolkit.Mvvm.ComponentModel;
using HamsterStudio.ChatBox.Models;

namespace HamsterStudio.ChatBox.ViewModels;

/// <summary>用于界面展示的一条消息。</summary>
public partial class ChatMessageViewModel : ObservableObject
{
    public ChatRole Role { get; }

    public bool IsUser => Role == ChatRole.User;

    public string Author => Role switch
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

    [ObservableProperty]
    private string _content;

    public ChatMessageViewModel(ChatRole role, string content, IReadOnlyList<string>? images = null)
    {
        Role = role;
        _content = content;
        Images = images ?? Array.Empty<string>();
    }
}
