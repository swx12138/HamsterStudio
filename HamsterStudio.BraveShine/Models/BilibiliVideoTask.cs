using CommunityToolkit.Mvvm.ComponentModel;
using HamsterStudio.Barefeet.Logging;
using HamsterStudio.BraveShine.Models.Bilibili;
using HamsterStudio.BraveShine.Services;
using HamsterStudio.Web.Interfaces;
using System.IO;
using System.Windows.Documents;

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
            _ = await Run2();
        }

        public async Task<string> Run2()
        {
            try
            {
                State = HamsterTaskState.Running;

                var accept = vsi.Value.AcceptQuality.Zip(vsi.Value.AcceptFormat.Split(','), vsi.Value.AcceptDescription).First(x => x.First == page.AcceptQuality);
                Logger.Shared.Information($"Selected quality {accept.Second}({accept.Third}, {accept.First})");

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

                string wish_filename = $"{videoInfo.Cid!}-{vps}_{videoInfo.Bvid}.mp4";
                string output = await downloader.Download(meta, aBaseUrl, vBaseUrl,wish_filename                     );
                
                State = HamsterTaskState.Succeed;
                if (output == "")
                {
                    return $"File {wish_filename} already exists.";
                }
                else
                {
                    return $"{Path.GetFullPath(output)} Succeed.";
                }

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
                return ex.Message;
            }
            finally
            {
                _NotifySuccess.Set();
                TaskComplete?.Invoke(this, this);
            }
        }

    }
}
