using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HamsterStudio.BraveShine.Models;
using HamsterStudio.BraveShine.Models.Bilibili;
using HamsterStudio.BraveShine.Models.Bilibili.SubStruct;
using HamsterStudio.BraveShine.Services;
using HamsterStudio.Toolkits.Logging;
using HamsterStudio.Web.Services;
using System.IO;
using System.Text.Json;
using System.Windows.Input;

namespace HamsterStudio.BraveShine.ViewModels
{
    partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _location = string.Empty;

        [ObservableProperty]
        private VideoInfo _videoInfo = new();

        [ObservableProperty]
        private WatchLaterData _watchLaters;

        private string BvId => Location.Split("?")[0].Split("/").First(x => x.StartsWith("BV", StringComparison.CurrentCultureIgnoreCase));

        public ICommand SaveCoverCommand { get; }
        public ICommand SaveOwnerFaceCommand { get; }
        public ICommand SaveFirstFrameCommand { get; }
        public ICommand SaveVideoCommand { get; }
        public ICommand RedirectCommand { get; }
        public ICommand RedirectLocationCommand { get; }
        public ICommand LoadWatchLaterCommand { get; }

        private BiliApiClient client = new(null);

        [ObservableProperty]
        private bool _topmost = false;

        public ObservableCollectionTarget NlogTarget { get; } = new("Brave Shine");

        public MainViewModel()
        {
            Logger.Shared.AddTarget(NlogTarget);

#if DEBUG
            string text = File.ReadAllText(@"D:\Code\HamsterStudio\HamsterStudio.BraveShine\BV1Mb9tYKEHF_VideoInfo.json");
            VideoInfo = JsonSerializer.Deserialize<Response<VideoInfo>>(text).Data!;
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

            RedirectLocationCommand = new RelayCommand(() =>
            {           
                if (client.TryGetVideoInfo(BvId, out VideoInfo? videoInfo)) 
                {
                    VideoInfo = videoInfo!;
                }
            });
            RedirectCommand = new RelayCommand<string>(loc =>
            {
                Location = loc;
                RedirectLocationCommand.Execute(null);
            });

            LoadWatchLaterCommand = new AsyncRelayCommand(async () =>
            {
                try
                {
                    WatchLaters = await client.GetWatchLater();
                }
                catch (Exception ex)
                {
                    Logger.Shared.Critical(ex);
                }
            });


        }

    }
}
