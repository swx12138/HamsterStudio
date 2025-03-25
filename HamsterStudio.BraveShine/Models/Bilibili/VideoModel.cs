using HamsterStudio.BraveShine.Modelss.Bilibili.SubStruct;
using HamsterStudio.BraveShine.Services;

namespace HamsterStudio.BraveShine.Models.Bilibili
{
    class VideoModel
    {
        private Lazy<VideoInfo?> _info;
        public  VideoInfo VideoInfo => _info.Value;

        public string Bvid { get;}
        public string Title { get;}

        public VideoModel(string bvid, BiliApiClient client)
        {
            _info = new Lazy<VideoInfo?>(() => client.GetVideoInfo(bvid).Result);
            Bvid = bvid;
            Title = string.Empty;
        }
        
        public VideoModel(VideoInfo videoInfo)
        {
            _info = new Lazy<VideoInfo?>(() => videoInfo);
            Bvid = videoInfo.Bvid;
            Title = videoInfo.Title;
        }

        public VideoModel(WatchLaterDat dat, BiliApiClient client) {
            _info = new Lazy<VideoInfo?>(() => client.GetVideoInfo(dat.Bvid).Result);
            Bvid = dat.Bvid;
            Title = dat.Title;
        }

    }
}
