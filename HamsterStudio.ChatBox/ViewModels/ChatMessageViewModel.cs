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

    [ObservableProperty]
    private string _content;

    public ChatMessageViewModel(ChatRole role, string content)
    {
        Role = role;
        _content = content;
    }
}
