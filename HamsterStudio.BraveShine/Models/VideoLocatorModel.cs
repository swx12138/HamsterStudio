using CommunityToolkit.Mvvm.ComponentModel;
using HamsterStudio.BraveShine.Modelss.Bilibili.SubStruct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudio.BraveShine.Models
{
    public partial class VideoLocatorModel : ObservableObject
    {
        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private string bvid;

        [ObservableProperty]
        private string cover;

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
