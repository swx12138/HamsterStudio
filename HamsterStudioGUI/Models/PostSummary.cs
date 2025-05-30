using CommunityToolkit.Mvvm.ComponentModel;

namespace HamsterStudioGUI.Models;

internal partial class PostSummary : ObservableObject
{
    [ObservableProperty]
    private int _like = 0;  // 点赞数

    [ObservableProperty]
    private int _reply = 0; // 回复数

    [ObservableProperty]
    private int _favorite;  // 收藏数

    [ObservableProperty]
    private int _view = 0;  // 浏览数
}