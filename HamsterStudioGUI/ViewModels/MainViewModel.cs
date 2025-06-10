using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Bilibili.Services;
using HamsterStudio.RedBook.Services;
using HamsterStudioGUI.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Input;

namespace HamsterStudioGUI.ViewModels
{
    internal partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _coverUrl = "https://i1.hdslb.com/bfs/archive/4f3bdda12690df02a5e2b957d6e2e2c4dd953d00.jpg";

        [ObservableProperty]
        private string _title = "Title";

        [ObservableProperty]
        private string _body = "Body";

        [ObservableProperty]
        private UserInfoModel _userInfoModel = new UserInfoModel();

        [ObservableProperty]
        private PostSummary _postSummary = new PostSummary();

        public ICommand SaveCoverCommand { get; }


        private DownloadService downloadService = App.ServiceProvider.GetService<DownloadService>() ?? throw new NotSupportedException();
        private RedBookDownloadService redBookDownloadService = App.ServiceProvider.GetService<RedBookDownloadService>() ?? throw new NotSupportedException();

        public MainViewModel()
        {
            SaveCoverCommand = new AsyncRelayCommand(async () => await downloadService.SaveFile(Title, CoverUrl));

            downloadService.OnVideoInfoUpdated += async (videoInfo) => await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Title = videoInfo.Title;
                CoverUrl = videoInfo.Pic;
                Body = videoInfo.Desc;
                UserInfoModel = new UserInfoModel
                {
                    Username = videoInfo.Owner.Name,
                    AvatarUrl = videoInfo.Owner.Face
                };
                PostSummary = new PostSummary
                {
                    View = videoInfo.Stat.View.ToString(),
                    Danmaku = videoInfo.Stat.Danmaku.ToString(),
                    Reply = videoInfo.Stat.Reply.ToString(),
                    Favorite = videoInfo.Stat.Favorite.ToString(),
                    Coin = videoInfo.Stat.Coin.ToString(),
                    Share = videoInfo.Stat.Share.ToString(),
                    Like = videoInfo.Stat.Like.ToString()
                };
            });
            redBookDownloadService.OnNoteDetailUpdated += async (noteDetail) => await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Title = noteDetail.Title;
                CoverUrl = noteDetail.ImageList.FirstOrDefault()?.DefaultUrl ?? string.Empty;
                Body = noteDetail.Description;
                UserInfoModel = new UserInfoModel
                {
                    Username = noteDetail.UserInfo.Nickname,
                    AvatarUrl = noteDetail.UserInfo.Avatar
                };
                PostSummary = new PostSummary
                {
                    Like = noteDetail.InteractInfo.LikedCount,
                    Favorite = noteDetail.InteractInfo.CollectedCount,
                    Reply = noteDetail.InteractInfo.CommentCount,
                    Share = noteDetail.InteractInfo.ShareCount,
                };
            });

        }

    }
}
