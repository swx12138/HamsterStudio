using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HamsterStudio.RedBook.Services.XhsRestful;
using Refit;
using System.Diagnostics;
using System.Windows.Input;

namespace HamsterStudioMaui.ViewModels;

public class LoggingHandler(HttpMessageHandler innerHandler, Action<string> mouth) : DelegatingHandler(innerHandler)
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // 输出请求地址
        mouth($"[Request] {request}");

        var response = await base.SendAsync(request, cancellationToken);
        return response;
    }
}

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

    public ICommand ExtractCommand { get; }

    private IRedBookClient hamsterClient;

    public MainPageModel()
    {
        hamsterClient = RestService.For<IRedBookClient>($"http://{HostName}:{Port}");   // TBD:考虑移到AebApiClients里面
        ExtractCommand = new AsyncRelayCommand(ExtractShareLinkAsyncasync);
    }

    private async Task ExtractShareLinkAsyncasync()
    {
        try
        {
            if (shareInfo.StartsWith("BV"))
            {
                //var resp_text = await browser.PostAsync($"/bilib", shareInfo);
            }
            else
            {
                var url = shareInfo.Split().FirstOrDefault(x => x.StartsWith("http"))?.Split("，").First();
                if (url == null || url == string.Empty) { Trace.WriteLine("解析Url失败！"); return; }
                else { Trace.TraceInformation($"Loading url {url}"); }

                var noteData = await hamsterClient.PostXhsShareLink(new() { Download = true, Url = url });
                var resp = await hamsterClient.DownloadXhsNote(noteData);

                try
                {
                    Log = $"Process {url} finished.\nAuthor:{resp.Data.AuthorNickName}\nTitle:{resp.Data.Title}\nDesc:{resp.Data.Description}";

#if ANDROID
                    if (saveToPhone)
                    {
                        await Task.Run(async () =>
                        {
                            var results = new List<string>();
                            Log += "\n -*- Downloading static files...";
                            foreach (var static_file_url in resp.Data.StaticFiles)
                            {
                                string filename = Path.GetFileName(static_file_url);
                                var stream = await hamsterClient.GetStaticFile(static_file_url);
                                string result = Platforms.Android.Utils.FileUtils.WriteFileToDCIM(filename, stream);
                                Log += "\n" + result;
                                results.Add(result);
                            }
                            Platforms.Android.Utils.FileUtils.NotifyGalleryOfNewImage([.. results]);
                        }); // 让UI线程继续运行，不阻塞
                    }
#endif
                }
                catch (Exception ex)
                {
                    Log = ex.Message + "\n" + ex.StackTrace;
                }

                ShareInfo = string.Empty;
            }
        }
        catch (Exception ex)
        {
            Log = ex.Message + "\n" + ex.StackTrace;
        }
    }

}
