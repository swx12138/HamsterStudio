using CommunityToolkit.Mvvm.ComponentModel;
using HamsterStudio.Bilibili.Models.Sub;

namespace HamsterStudio.Bilibili.Models
{
    public partial class VideoLocatorModel : ObservableObject
    {
        [ObservableProperty]
        private string title = string.Empty;

        [ObservableProperty]
        private string bvid = string.Empty;

        [ObservableProperty]
        private string cover = string.Empty;

        public VideoLocatorModel()
        {

        }

        public VideoLocatorModel(WatchLaterDat dat)
        {
            Title = dat.Title;
            bvid = dat.Bvid;
            cover = dat.Pic;
        }

    }
}
