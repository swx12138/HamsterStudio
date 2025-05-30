using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HamsterStudio.HandyUtil.PropertyEditors;
using HandyControl.Controls;
using System.ComponentModel;
using System.Windows.Input;

namespace HamsterStudioGUI.Models;

internal partial class UserInfoModel : ObservableObject
{
    [ObservableProperty]
    private string _username = "Guest User";

    [ObservableProperty]
    [property: Editor(typeof(ImageViewOnlyEditor), typeof(PropertyEditorBase))]
    private string _avatarUrl = "https://i1.hdslb.com/bfs/face/a71d50b0fa790be9646f976c1c99ddacc76ca9e6.jpg";

    [ObservableProperty]
    private int _totalFollowers = 114514;

    public ICommand SaveAvatarCommand { get; }

    public UserInfoModel()
    {
        SaveAvatarCommand = new AsyncRelayCommand(SaveAvatar);
    }

    private async Task SaveAvatar() { }

}
