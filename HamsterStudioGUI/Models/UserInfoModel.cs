using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Bilibili.Models;
using HamsterStudio.Bilibili.Models.Sub;
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
    private uint _totalFollowers = 114514;

    public ICommand SaveAvatarCommand { get; }

    public UserInfoModel()
    {
        SaveAvatarCommand = new AsyncRelayCommand(SaveAvatar);
    }

    private async Task SaveAvatar() { throw new NotImplementedException(); }

    public void Copy(UserInfoModel userInfoModel)
    {
        if (userInfoModel is null) return;
        Username = userInfoModel.Username;
        AvatarUrl = userInfoModel.AvatarUrl;
        TotalFollowers = userInfoModel.TotalFollowers;
    }

    public void Copy(Owner owner)
    {
        Username = owner.Name;
        AvatarUrl = owner.Face;
    }

}
