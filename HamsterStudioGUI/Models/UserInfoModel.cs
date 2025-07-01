using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Bilibili.Models;
using HamsterStudio.Bilibili.Models.Sub;
using HamsterStudio.HandyUtil.PropertyEditors;
using HamsterStudioGUI.Constants;
using HandyControl.Controls;
using System.ComponentModel;
using System.Windows.Input;

namespace HamsterStudioGUI.Models;

internal partial class UserInfoModel : ObservableObject
{
    [ObservableProperty]
    private string _username = Defaults.UserName;

    [ObservableProperty]
    [property: Editor(typeof(ImageViewOnlyEditor), typeof(PropertyEditorBase))]
    private string _avatarUrl = Defaults.AvatarUrl;

    [ObservableProperty]
    private uint _totalFollowers = Defaults.TotalFollowers;

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
