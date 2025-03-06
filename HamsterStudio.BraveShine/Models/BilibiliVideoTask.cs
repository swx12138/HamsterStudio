using CommunityToolkit.Mvvm.ComponentModel;
using HamsterStudio.BraveShine.Models.Bilibili;
using HamsterStudio.BraveShine.Services;
using HamsterStudio.Toolkits.Logging;
using HamsterStudio.Web.Interfaces;
using System.IO;

namespace HamsterStudio.BraveShine.Models
{
    public partial class BilibiliVideoTask(
        int vps,
        BilibiliVideoPage page,
        VideoStreamInfo? vsi,
        VideoInfo videoInfo,
        BiliApiClient client) : ObservableObject, IHamsterTask
    {
        public string Name => page.PartTitle;

        public string Description => videoInfo.Bvid ?? " -bvid- ";
        public long ProgressValue => 0;

        public long ProgressMaximum => 100;

        public AutoResetEvent NotifySuccess => _NotifySuccess;

        public event EventHandler<IHamsterTask>? TaskComplete;

        private AutoResetEvent _NotifySuccess = new(false);

        [ObservableProperty]
        private HamsterTaskState _State;

        public async void Run()
        {
            try
            {
                State = HamsterTaskState.Running;

                string vBaseUrl = getVideoBaseUrl(page, vsi);
                Logger.Shared.Information("Video:" + vBaseUrl);

                string aBaseUrl = getAudioBaseUrl(vsi);
                Logger.Shared.Information("Audio:" + aBaseUrl);

                AvDownloader downloader = new(client);
                AvMeta meta = new()
                {
                    title = page.PartTitle,
                    artist = videoInfo.Owner.Name!,
                    album = videoInfo.Title!,
                    copyright = videoInfo.Bvid!
                };
                string output = await downloader.Download(meta, aBaseUrl, vBaseUrl,
                     $"{videoInfo.Cid!}-{vps}_{videoInfo.Bvid}.mp4");

                if(output == "")
                {
                    return;
                }
                Logger.Shared.Information($"{Path.GetFullPath(output)} Succeed.");

                State = HamsterTaskState.Succeed;

                static string getVideoBaseUrl(BilibiliVideoPage page, VideoStreamInfo? vsi)
                {
                    var lst = vsi?.Dash.Video.Where(x => x.Id == page.AcceptQuality)
                        .OrderBy(x => x.Bandwidth);
                    if (lst == null || !lst.Any())
                    {
                        lst = vsi?.Dash.Video.OrderBy(x => x.Bandwidth);
                    }

                    Logger.Shared.Information($"Video dash info : {lst?.Last()!.Width}*{lst?.Last()!.Height} bandw:{lst?.Last()!.Bandwidth}");
                    return lst?.Last().BaseUrl ?? string.Empty;
                }

                static string getAudioBaseUrl(VideoStreamInfo? vsi)
                {
                    return vsi?.Dash.Audio.OrderBy(x => x.Bandwidth)
                         .Last().BaseUrl ?? string.Empty;
                }
            }
            catch (Exception ex)
            {
                State = HamsterTaskState.Failed;
                Logger.Shared.Critical(ex);
            }
            finally
            {
                _NotifySuccess.Set();
                TaskComplete?.Invoke(this, this);
            }
        }

    }
}
