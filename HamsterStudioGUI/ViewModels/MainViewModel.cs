using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Bilibili;
using HamsterStudio.Web;
using HamsterStudio.Web.Utilities;
using HamsterStudioGUI.Models;
using System.Windows.Input;

namespace HamsterStudioGUI.ViewModels
{
    internal partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _coverUrl = "https://i1.hdslb.com/bfs/archive/4f3bdda12690df02a5e2b957d6e2e2c4dd953d00.jpg";

        [ObservableProperty]
        private string _title = "Title";

        [ObservableProperty]
        private string _body = "Body";

        [ObservableProperty]
        private UserInfoModel _userInfoModel = new UserInfoModel();

        [ObservableProperty]
        private PostSummary _postSummary = new PostSummary();

        public ICommand SaveCoverCommand { get; }

        public MainViewModel()
        {
            SaveCoverCommand = new AsyncRelayCommand(async () => await FilenameUtils.Downlaod(CoverUrl, Title));
        }
    }
}
