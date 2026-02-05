using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Barefeet.SysCall;
using HamsterStudio.Bilibili.Services;
using HamsterStudio.RedBook.Services;
using HamsterStudio.SinaWeibo.Services;
using HamsterStudioGUI.Constants;
using HamsterStudioGUI.Models;
using Microsoft.Extensions.Logging;
using System.Windows;
using System.Windows.Input;

namespace HamsterStudioGUI.ViewModels
{
    internal partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _coverUrl = Defaults.CoverUrl;

        [ObservableProperty]
        private string _title = Defaults.Title;

        [ObservableProperty]
        private string _body = Defaults.Body;

        [ObservableProperty]
        private UserInfoModel _userInfoModel = new();

        [ObservableProperty]
        private PostSummaryModel _postSummary = new();

        [ObservableProperty]
        private string _lastSavedFileFolder = string.Empty;

        private Lazy<ILogger> logger = new Lazy<ILogger>(() => App.ResloveService<ILogger<MainViewModel>>() ?? throw new NotSupportedException());

        public ICommand SaveCoverCommand { get; }
        public ICommand OpenFolderCommand { get; }

        private BangumiDownloadService downloadService = App.ResloveService<BangumiDownloadService>() ?? throw new NotSupportedException();
        private NoteDownloadService redBookDownloadService = App.ResloveService<NoteDownloadService>() ?? throw new NotSupportedException();
        private WeiboDownloadService weiboDs = App.ResloveService<WeiboDownloadService>() ?? throw new NotSupportedException();

        public MainViewModel()
        {
            SaveCoverCommand = new AsyncRelayCommand(async () => await downloadService.SaveFile(Title, CoverUrl));
            OpenFolderCommand = new RelayCommand(() =>
            {
                try
                {
                    ShellApi.OpenFolder(LastSavedFileFolder);
                }
                catch (Exception ex)
                {
                    logger.Value.LogWarning(ex.Message);
                }
            });

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
            redBookDownloadService.OnNoteDetailUpdated += async (pack) => await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var (noteDetail, di) = pack;

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

                _lastSavedFileFolder = di.FullName;
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
