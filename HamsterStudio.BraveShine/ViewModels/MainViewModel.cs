using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HamsterStudio.BraveShine.Models;
using HamsterStudio.BraveShine.Models.Bilibili;
using HamsterStudio.BraveShine.Models.Bilibili.SubStruct;
using HamsterStudio.BraveShine.Services;
using HamsterStudio.Web.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HamsterStudio.BraveShine.ViewModels
{
    partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _location = string.Empty;

        [ObservableProperty]
        private VideoInfo _videoInfo = new();

        public ICommand SaveCoverAsCommand { get; }
        public ICommand SaveOwnerFaceAsCommand { get; }
        public ICommand SaveFirstFrameAsCommand { get; }
        public ICommand RedirectLocationCommand { get; }

        private string cookies = "buvid3=CAA6FA10-FD19-913F-7483-E74D33B1E12C17757infoc; b_nut=1723375817; _uuid=587F478E-BF45-F3D1-C9A6-3519BD4A8BAE19615infoc; enable_web_push=DISABLE; home_feed_column=5; buvid4=3E0EC837-88AF-B4F8-6AA9-1D7F6E2E612D18448-024081111-QzNk1L6zDArUWxNI%2Fzea2g%3D%3D; buvid_fp=43b513f34c69b163e5ea5943ab13f464; b_lsid=98F2F985_1956165D403; header_theme_version=CLOSE; enable_feed_channel=DISABLE; browser_resolution=2560-1271; bili_ticket=eyJhbGciOiJIUzI1NiIsImtpZCI6InMwMyIsInR5cCI6IkpXVCJ9.eyJleHAiOjE3NDEzNTUwMTgsImlhdCI6MTc0MTA5NTc1OCwicGx0IjotMX0.CSLf4rvt3yvpRshr5-jehv5ddkWg5y1zbFNkOqcztY8; bili_ticket_expires=1741354958; SESSDATA=a260a95e%2C1756647853%2C72cb5%2A31CjDLik2y7iEy4QaAaWKgZ9iUbf40hKERI8BifVnEkd1lxxKnd6ZvIXNXHKoKzJ_ETpISVkNRc2ZlN1B2aHV3M0NaTzJwRVN3azY4MHZRY180U181WC1ta2xwNndsazl5bHM0eExXajVjdDliZFhrYk5qdXhLWGNTSmNYVTI3NFpZaThWbXhrZDlnIIEC; bili_jct=0a74bcef1a6d1d9f54d10a5f308252c0; DedeUserID=286884672; DedeUserID__ckMd5=b2f1150da1c3b569; sid=57osfa7o; CURRENT_FNVAL=4048; rpdid=|(u)~lmkJkuR0J'u~Ru|ml)Yl";

        public MainViewModel()
        {
#if DEBUG
            string text = File.ReadAllText(@"D:\Code\HamsterStudio\HamsterStudio.BraveShine\BV1Mb9tYKEHF_VideoInfo.json");
            VideoInfo = JsonSerializer.Deserialize<Response<VideoInfo>>(text).Data;
#endif

            SaveCoverAsCommand = new AsyncRelayCommand(async () => await FileSaver.SaveFileFromUrl(VideoInfo?.Pic, Environment.CurrentDirectory));
            SaveOwnerFaceAsCommand = new AsyncRelayCommand(async () => await FileSaver.SaveFileFromUrl(VideoInfo?.Owner.Face, Environment.CurrentDirectory));
            SaveFirstFrameAsCommand = new AsyncRelayCommand<PagesItem>(async page => await FileSaver.SaveFileFromUrl(page.FirstFrame, Environment.CurrentDirectory));
            RedirectLocationCommand = new RelayCommand(() =>
            {
                BiliApiClient client = new(cookies, null);
                if (client.TryGetVideoInfo(Location, out VideoInfo? videoInfo)) // resp.Code = -400
                {
                    VideoInfo = videoInfo!;
                }
            });

        }

    }
}
