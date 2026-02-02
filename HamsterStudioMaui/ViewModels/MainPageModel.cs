using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Barefeet.Extensions;
using HamsterStudio.Bilibili.Services.Restful;
using HamsterStudio.RedBook.Services.XhsRestful;
using HamsterStudio.Web.DataModels;
using HamsterStudio.Web.Services.Restful;
using HamsterStudioMaui.Services;
using Refit;
using System.Diagnostics;
using System.Windows.Input;

namespace HamsterStudioMaui.ViewModels;

partial class MainPageModel : ObservableObject
{
    [ObservableProperty]
    private string shareInfo = string.Empty;

    [ObservableProperty]
    private string _hostName = "192.168.0.101";

    [ObservableProperty]
    private string _port = "5000";

    private string _log = string.Empty;
    public string Log
    {
        get => _log;
        set
        {
            SetProperty(ref _log, value);
            Trace.WriteLine(Log);
        }
    }

    [ObservableProperty]
    private bool saveToPhone = false;

    [ObservableProperty]
    private bool serverOffline = true;

    public ICommand ExtractCommand { get; }

    private readonly ProcessChain ProcessChain;
    private readonly IStaticFilesClient staticFilesClient;

    public MainPageModel()
    {
        // TBD:考虑移到AebApiClients里面
        string server = $"http://{HostName}:{Port}";
        ProcessChain = new XiaohongshuProcess(RestService.For<IRedBookClient>(server),
            new BilibiliProcess(RestService.For<IBilibiliClient>(server), null));
        staticFilesClient = RestService.For<IStaticFilesClient>(server);
        ExtractCommand = new AsyncRelayCommand(ExtractShareLinkAsyncasync);
    }

    private async Task ExtractShareLinkAsyncasync()
    {
        Log = string.Empty;
        try
        {
            var resp = await ProcessChain.Process(ShareInfo);
            if (resp == null)
            {
                Log += $"/n没有匹配的处理模块。";
                return;
            }

            Log = $"Author:{resp.Data.AuthorNickName}\nTitle:{resp.Data.Title}\nDesc:{resp.Data.Description}";

            await SaveFiles(resp);
        }
        catch (Exception ex)
        {
            Log += "\n" + ex.Message + "\n" + ex.StackTrace;
        }
        finally
        {
            ShareInfo = string.Empty;
        }
    }

    private async Task SaveFiles(ServerRespModel resp)
    {
#if ANDROID
        if (SaveToPhone)
        {
            await Task.Run(async () =>
            {
                var results = new List<string>();
                Log += "\n -*- Downloading static files...";
                foreach (var static_file_url in resp.Data.StaticFiles)
                {
                    string filename = Path.GetFileName(static_file_url);
                    var stream = await staticFilesClient.GetStaticFile(static_file_url);
                    string result = Platforms.Android.Utils.FileUtils.WriteFileToDCIM(filename, stream);
                    Log += "\n" + result;
                    results.Add(result);
                }
                Platforms.Android.Utils.FileUtils.NotifyGalleryOfNewImage([.. results]);
            }); // 让UI线程继续运行，不阻塞
        }
#endif
    }

}
