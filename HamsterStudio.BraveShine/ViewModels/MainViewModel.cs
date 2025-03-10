using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Barefeet.Logging;
using HamsterStudio.BraveShine.Models;
using HamsterStudio.BraveShine.Models.Bilibili;
using HamsterStudio.BraveShine.Models.Bilibili.SubStruct;
using HamsterStudio.BraveShine.Services;
using HamsterStudio.BraveShine.Views;
using HamsterStudio.Toolkits.Logging;
using HamsterStudio.Web.Services;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows.Input;

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

        private string BvId => Location.Bvid.Split("?")[0].Split("/").First(x => x.StartsWith("BV", StringComparison.CurrentCultureIgnoreCase));

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
            Logger.Shared.AddTarget(NlogTarget);

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

            SaveCoverCommand = new AsyncRelayCommand(async () => await (new AvDownloader(client)).SaveCover(VideoInfo));
            SaveOwnerFaceCommand = new AsyncRelayCommand(async () => await FileSaver.SaveFileFromUrl((VideoInfo)?.Owner.Face, AvDownloader.BVCoverHome));

            SaveFirstFrameCommand = new AsyncRelayCommand<PagesItem>(async page => await (new AvDownloader(client)).SaveCover(BvId, page));
            SaveVideoCommand = new AsyncRelayCommand<int>(async idx =>
            {
                idx = Math.Max(idx, 0);
                await Task.Run(async () =>
                {
                    var resp = await client.GetVideoStream(BvId, VideoInfo!.Pages[idx]);
                    if (resp == null)
                    {
                        Logger.Shared.Warning($"GetVideoStream Failed.");
                        return;
                    }

                    //foreach()
                    BilibiliVideoPage vpage = new(VideoInfo!.Pages[idx], resp!);
                    BilibiliVideoTask bilibiliVideoTask = new(idx, vpage, resp!, (VideoInfo), client);
                    bilibiliVideoTask.Run();
                    Logger.Shared.Information("SaveVideoCommand Finish.");
                });
            });

            RedirectLocationCommand = new RelayCommand(() => RedirectLocation());
            RedirectCommand = new RelayCommand<string>(loc => RedirectLocation(loc));

            LoadWatchLaterCommand = new AsyncRelayCommand(async () =>
            {
                try
                {
                    var watchLaters = await client.GetWatchLater();
                    if (watchLaters == null)
                    {
                        return;
                    }
                    QuickList = [.. watchLaters.List.Select(x => new VideoLocatorModel(x))];
                }
                catch (Exception ex)
                {
                    Logger.Shared.Critical(ex);
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
            return RedirectLocation(BvId);
        }

        public void DownloadVideoByBvid(string bvid)
        {
            if (!RedirectLocation(bvid))
            {
                Logger.Shared.Error($"Can't load info from {bvid}.");
                return;
            }
            SaveCoverCommand?.Execute(null);
            SaveVideoCommand?.Execute(0);
        }

    }
}
