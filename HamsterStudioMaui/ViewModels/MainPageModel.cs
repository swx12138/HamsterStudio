using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Bilibili.Services.Restful;
using HamsterStudio.RedBook.Services.XhsRestful;
using HamsterStudio.Web.DataModels;
using HamsterStudio.Web.Services.Restful;
using HamsterStudioMaui.Services;
using Refit;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace HamsterStudioMaui.ViewModels;

partial class MainPageModel : ObservableObject
{
    [ObservableProperty]
    private string shareInfo = string.Empty;

    [ObservableProperty]
    private string _localIpAddress = GetLocalIpAddress();

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
    private bool _serverOffline = true;

    private readonly ProcessChain ProcessChain;
    private readonly IStaticFilesClient staticFilesClient;
    private readonly Lazy<OfflineProcessService> offlineProcessService;

    public MainPageModel() : this(null!) { }

    public MainPageModel(OfflineProcessService? offlineProcessService)
    {
        this.offlineProcessService = new Lazy<OfflineProcessService>(() =>
            offlineProcessService ?? Application.Current?.Handler?.MauiContext?.Services.GetRequiredService<OfflineProcessService>()
            ?? throw new InvalidOperationException("OfflineProcessService not available"));

        // TBD:考虑移到AebApiClients里面
        string server = $"http://{HostName}:{Port}";
        ProcessChain = new XiaohongshuProcess(RestService.For<IRedBookClient>(server),
            new BilibiliProcess(RestService.For<IBilibiliClient>(server), null));
        staticFilesClient = RestService.For<IStaticFilesClient>(server);
    }

    [RelayCommand]
    private async Task Extract()
    {
        Log = $"Extracting...";
        if (ServerOffline)
        {
            await ExtractShareLinkOfflineAsync();
        }
        else {
            await ExtractShareLinkAsync();
        }
    }

    private async Task ExtractShareLinkOfflineAsync()
    {
        Log += "\n[离线模式] 暂不可用...";
        return;

        try
        {
            Log += "\n[离线模式] 正在处理...";
            var resp = await offlineProcessService.Value.ProcessAsync(ShareInfo);
            if (resp == null || resp.Status != 0)
            {
                Log += $"\n处理失败：{resp?.Message ?? "未知错误"}";
                return;
            }

            Log += $"\nAuthor:{resp.Data.AuthorNickName}\nTitle:{resp.Data.Title}\nDesc:{resp.Data.Description}";

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

    private async Task ExtractShareLinkAsync()
    {
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

    private static string GetLocalIpAddress()
    {
        foreach (var netInterface in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (netInterface.OperationalStatus == OperationalStatus.Up &&
                netInterface.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            {
                foreach (var addr in netInterface.GetIPProperties().UnicastAddresses)
                {
                    if (addr.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return addr.Address.ToString();
                    }
                }
            }
        }
        return "127.0.0.1";
    }

    private async Task SaveFiles(ServerRespModel resp)
    {
#if ANDROID
        if (SaveToPhone)
        {
            await Task.Run(async () =>
            {
                var results = new List<string>();
                Log += "\n -*- Saving static files...";
                foreach (var static_file_url in resp.Data.StaticFiles)
                {
                    string filename = Path.GetFileName(static_file_url);
                    if (Platforms.Android.Utils.FileUtils.ExistsInDCIM(filename))
                    {
                        Log += $"\n[跳过] {filename} 已存在";
                        continue;
                    }

                    Stream stream;
                    if (File.Exists(static_file_url))
                    {
                        // 离线模式：本地文件直接读取
                        stream = File.OpenRead(static_file_url);
                    }
                    else
                    {
                        // 在线模式：从服务器下载
                        stream = await staticFilesClient.GetStaticFile(static_file_url);
                    }

                    using (stream)
                    {
                        string result = Platforms.Android.Utils.FileUtils.WriteFileToDCIM(filename, stream);
                        Log += "\n" + result;
                        results.Add(result);
                    }
                }
                Platforms.Android.Utils.FileUtils.NotifyGalleryOfNewImage([.. results]);
            }); // 让UI线程继续运行，不阻塞
        }
#endif
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "HostName")
        {
            Trace.TraceInformation($"[{e.PropertyName}] -> {HostName}");
        }
        base.OnPropertyChanged(e);
    }

}
