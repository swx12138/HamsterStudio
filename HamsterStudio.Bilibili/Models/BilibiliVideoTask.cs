using CommunityToolkit.Mvvm.ComponentModel;
using HamsterStudio.Barefeet.Interfaces;
using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Barefeet.Task;
using HamsterStudio.Bilibili.Services;

namespace HamsterStudio.Bilibili.Models
{
    public partial class BilibiliVideoTask(
        int vps,
        BilibiliVideoPage page,
        VideoStreamInfo vsi,
        VideoInfo videoInfo, BiliApiClient biliApiClient) : ObservableObject, IHamsterTask
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

        public async Task<BilibiliVideoDownloadResult> Run2()
        {   
            throw new NotImplementedException("This method is not implemented yet. Please use the Run method instead.");

            State = HamsterTaskState.Running;

            try
            {
                var accept = vsi.AcceptQuality.Zip(vsi.AcceptFormat.Split(','), vsi.AcceptDescription).First(x => x.First == page.AcceptQuality);
                Logger.Shared.Information($"Selected quality {accept.Second}({accept.Third}, {accept.First})");

                string vBaseUrl = getVideoBaseUrl(page, vsi);
                ArgumentException.ThrowIfNullOrEmpty(vBaseUrl, nameof(vBaseUrl));

                string aBaseUrl = getAudioBaseUrl(vsi);
                ArgumentException.ThrowIfNullOrEmpty(aBaseUrl, nameof(aBaseUrl));

                //AvDownloader downloader = new();
                //AvMeta meta = new()
                //{
                //    title = page.PartTitle,
                //    artist = videoInfo.Owner.Name!,
                //    album = videoInfo.Title!,
                //    copyright = videoInfo.Bvid!
                //};

                //string wish_filename = $"{videoInfo.Cid!}-{vps}_{videoInfo.Bvid}.mp4";
                //var result = await downloader.Download(meta, aBaseUrl, vBaseUrl, wish_filename);
                
                //State = result.State == FileDownloadState.Failed ? HamsterTaskState.Failed:  HamsterTaskState.Succeed;
                //return result;

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
                    if(vsi == null ) return string.Empty;

                    var dash = (vsi?.Dash.Flac?.Audio ?? vsi?.Dash.Audio.OrderBy(x => x.Bandwidth).Last())!.Value;
                    Logger.Shared.Information($"Audio dash info : {dash.Bandwidth}");
                    return dash.BaseUrl ?? string.Empty;
                }
            }
            catch (Exception ex)
            {
                State = HamsterTaskState.Failed;
                Logger.Shared.Critical(ex);
                return new() { State = FileDownloadState.Failed, Exception = ex };
            }
            finally
            {
                _NotifySuccess.Set();
                TaskComplete?.Invoke(this, this);
            }
        }

    }
}
