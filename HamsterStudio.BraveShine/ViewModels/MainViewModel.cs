using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Barefeet.Logging;
using HamsterStudio.BraveShine.Models;
using HamsterStudio.BraveShine.Models.Bilibili;
using HamsterStudio.BraveShine.Models.Bilibili.SubStruct;
using HamsterStudio.BraveShine.Services;
using HamsterStudio.BraveShine.Views;
using HamsterStudio.Toolkits.Logging;
using System.Collections.ObjectModel;
using System.Windows.Input;
using HamsterStudio.Barefeet.Extensions;
using HamsterStudio.Web.Utilities;
using HamsterStudio.BraveShine.Constants;
using HamsterStudio.Barefeet.Interfaces;

namespace HamsterStudio.BraveShine.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private VideoLocatorModel _location = new();

        [ObservableProperty]
        private VideoInfo _videoInfo = new();

        [ObservableProperty]
        private ObservableCollection<VideoLocatorModel> _quickList = [];

        [ObservableProperty]
        private string lastInfomation = "No message.";

        public ICommand SaveCoverCommand { get; }
        public ICommand SaveOwnerFaceCommand { get; }
        public ICommand SaveFirstFrameCommand { get; }
        public ICommand SaveVideoCommand { get; }
        public ICommand RedirectCommand { get; }
        public ICommand RedirectLocationCommand { get; }
        public ICommand LoadWatchLaterCommand { get; }
        public ICommand SelectVideoCommad { get; }

        private BiliApiClient client = new(null);

        [ObservableProperty]
        private bool _topmost = false;

        public ObservableCollectionTarget NlogTarget { get; } = new("Brave Shine");

        public MainViewModel()
        {
            Logger.Shared.AddTarget(NlogTarget, NLog.LogLevel.Info, NLog.LogLevel.Fatal);

#if DEBUG
            //string text = File.ReadAllText(@"D:\Code\HamsterStudio\HamsterStudio.BraveShine\BV1Mb9tYKEHF_VideoInfo.json");
            //VideoInfo = JsonSerializer.Deserialize<Response<VideoInfo>>(text).Data!;
            Location = new() { Bvid = VideoInfo.Bvid };
#else
            VideoInfo = new()
            {
                Pic = "https://i1.hdslb.com/bfs/archive/07854a9b2e4de91abbd482216ba5ff35d8b772ef.jpg", // "https://i1.hdslb.com/bfs/archive/8f3a0a264f46bb681655676764e0bc0d37ff7650.jpg"
            };
#endif

            SaveCoverCommand = new AsyncRelayCommand(async () => await AvDownloader.SaveCover(VideoInfo));
            SaveOwnerFaceCommand = new AsyncRelayCommand(async () => await FileSaver.SaveFileFromUrl(VideoInfo?.Owner.Face ?? throw new NotImplementedException(), SystemConsts.BVCoverHome));

            SaveFirstFrameCommand = new AsyncRelayCommand<PagesItem>(async page => await AvDownloader.SaveCover(GetBvid(), page));
            SaveVideoCommand = new AsyncRelayCommand<int>(DownloadVideo);

            RedirectLocationCommand = new RelayCommand(() => RedirectLocation());
            RedirectCommand = new RelayCommand<string>(loc => RedirectLocation(loc));

            LoadWatchLaterCommand = new AsyncRelayCommand(async () =>
            {
                try
                {
                    var watchLaters = await client.GetWatchLater();
                    if (watchLaters == null)
                    {
                        LastInfomation = "Get watch laters falied";
                        return;
                    }
                    Logger.Shared.Debug($"[WatchLater] Got {watchLaters.Count} video.");
                    QuickList = [.. watchLaters.List.Select(x => new VideoLocatorModel(x))];
                }
                catch (Exception ex)
                {
                    Logger.Shared.Debug(ex);
                }
            });

            SelectVideoCommad = new RelayCommand(() =>
            {
                VideoSelectorWindow instance = new();
                instance.DataContext = QuickList;
                instance.ShowDialog();
                if (instance.Selected is not null and VideoLocatorModel vlm)
                {
                    Location = vlm;
                    RedirectLocationCommand.Execute(null);
                }
            });

        }

        private string GetBvid()
        {
            try
            {
                return Location.Bvid
                    .Split("?")[0]
                    .Split("/")
                    .First(x => x.StartsWith("BV", StringComparison.CurrentCultureIgnoreCase));
            }
            catch (Exception ex)
            {
                Logger.Shared.Debug(ex);
                return string.Empty;
            }
        }

        private async Task<BilibiliVideoDownloadResult?> DownloadVideo(int idx = -1)
        {
            string bvid = GetBvid();
            if (bvid.IsNullOrEmpty())
            {
                LastInfomation = "Empty bvid!";
                return null;
            }

            idx = Math.Max(idx, 0);
            var resp = await client.GetVideoStream(bvid, VideoInfo!.Pages[idx]);
            if (resp == null)
            {
                Logger.Shared.Warning($"GetVideoStream Failed.");
                LastInfomation = "获取视频流失败【resp is null】";
                return null;
            }

            BilibiliVideoPage vpage = new(VideoInfo!.Pages[idx], resp!);
            BilibiliVideoTask bilibiliVideoTask = new(idx, vpage, resp ?? throw new NotImplementedException(), (VideoInfo), client);
            var rslt = await bilibiliVideoTask.Run2();
            if (rslt.State == FileDownlaodState.Failed)
            {
                if (rslt.Exception != null)
                {
                    LastInfomation = rslt.Exception.Message;
                }
                else
                {
                    LastInfomation = "下载失败";
                }
            }
            else
            {
                LastInfomation = $"{rslt.VideoName} 下载成功。";
            }

            return rslt;
        }

        private bool RedirectLocation(string bvid)
        {
            Location.Bvid = bvid;
            if (!client.TryGetVideoInfo(bvid, out VideoInfo? videoInfo))
            {
                return false;
            }

            VideoInfo = videoInfo!;
            Location.Title = VideoInfo.Title;
            return true;
        }

        private bool RedirectLocation()
        {
            return RedirectLocation(GetBvid());
        }

        public async Task<(BilibiliVideoDownloadResult? rslt, string msg)> DownloadVideoByBvid(string bvid)
        {
            if (!RedirectLocation(bvid))
            {
                Logger.Shared.Error($"Can't load info from {bvid}.");
                return (null, LastInfomation);
            }
            SaveCoverCommand?.Execute(null);
            var rslt = await DownloadVideo();
            return (rslt, LastInfomation);
        }

    }
}
