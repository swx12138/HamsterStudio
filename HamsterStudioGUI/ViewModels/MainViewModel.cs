using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Bilibili.Services;
using HamsterStudio.RedBook.Services;
using HamsterStudioGUI.Models;
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
        private UserInfoModel _userInfoModel = new();

        [ObservableProperty]
        private PostSummaryModel _postSummary = new();

        public ICommand SaveCoverCommand { get; }


        private DownloadService downloadService = App.ResloveService<DownloadService>() ?? throw new NotSupportedException();
        private RedBookDownloadService redBookDownloadService = App.ResloveService<RedBookDownloadService>() ?? throw new NotSupportedException();
        private HamsterStudio.SinaWeibo.Services.DownloadService weiboDs = App.ResloveService<HamsterStudio.SinaWeibo.Services.DownloadService>() ?? throw new NotSupportedException();

        public MainViewModel()
        {
            SaveCoverCommand = new AsyncRelayCommand(async () => await downloadService.SaveFile(Title, CoverUrl));

            downloadService.OnVideoInfoUpdated += async (videoInfo) => await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Title = videoInfo.Title;
                CoverUrl = videoInfo.Pic;
                Body = videoInfo.Desc;
                UserInfoModel.Copy(videoInfo.Owner);
                PostSummary = PostSummary with
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

                UserInfoModel.Username = noteDetail.UserInfo.Nickname;
                UserInfoModel.AvatarUrl = noteDetail.UserInfo.Avatar;

                PostSummary = PostSummary with
                {
                    Like = noteDetail.InteractInfo.LikedCount,
                    Favorite = noteDetail.InteractInfo.CollectedCount,
                    Reply = noteDetail.InteractInfo.CommentCount,
                    Share = noteDetail.InteractInfo.ShareCount,
                };
            });
            weiboDs.OnShowInfoUpdated += async show => await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Title = show.Text;
                CoverUrl = show.PicInfos.First().Value.Large.Url ?? string.Empty;
                Body = show.TextRaw;

                UserInfoModel.Username = show.User.ScreenName;
                UserInfoModel.AvatarUrl = show.User.AvatarLarge;

                PostSummary = PostSummary with
                {
                    Like = show.AttitudesCount.ToString(),
                    Reply = show.CommentsCount.ToString(),
                    Share = show.RepostsCount.ToString(),
                };
            });
        }

    }
}
