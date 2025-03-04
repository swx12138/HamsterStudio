using CommunityToolkit.Mvvm.ComponentModel;
using HamsterStudio.BraveShine.Models.Bilibili;
using HamsterStudio.BraveShine.Models.Bilibili.SubStruct;

namespace HamsterStudio.BraveShine.Models
{
    public class BilibiliVideoPage(PagesItem page, VideoStreamInfo? info) : ObservableObject
    {
        public string PartTitle => page.Part;

        public string CoverImage => page.FirstFrame;

        public IEnumerable<string> Accepts => info?.SupportFormats.Select(x => $"[{x.Quality}]{x.NewDescription}({x.Format})") ?? [];

        public int AcceptQuality => info?.AcceptQuality[AcceptQualityIndex] ?? 120;

        private int AcceptQualityIndex_;
        public int AcceptQualityIndex
        {
            get { return AcceptQualityIndex_; }
            set { AcceptQualityIndex_ = value; }
        }
    }
}
