﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Web;
using HamsterStudio.Web.DataModels.XiaoHs;
using System.Diagnostics;
using System.Text.Json;
using System.Windows.Input;

namespace HamsterStudioMaui.ViewModels
{
    partial class MainPageModel : ObservableObject
    {
        [ObservableProperty]
        private string shareInfo = string.Empty;

        [ObservableProperty]
        private string _hostName = "192.168.0.101";

        [ObservableProperty]
        private string _port = "8899";

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

        public MainPageModel()
        {
            ExtractCommand = new AsyncRelayCommand(async () =>
            {
                FakeBrowser browser = new();
                try
                {
                    if (shareInfo.StartsWith("BV"))
                    {
                        var resp_text = await browser.PostAsync($"http://{HostName}:{Port}/bilib", shareInfo);
                    }
                    else
                    {
                        var url = shareInfo.Split().FirstOrDefault(x => x.StartsWith("http"))?.Split("，").First();
                        if (url == null || url == string.Empty) { Trace.WriteLine("解析Url失败！"); return; }
                        else { Trace.TraceInformation($"Loading url {url}"); }

                        var resp_text = await browser.PostAsync($"http://{HostName}:{Port}/xhs", new { download = true, url });

                        var resp = JsonSerializer.Deserialize<ServerResp>(resp_text);
                        Log = $"Process {url} finished.\nAuthor:{resp.Data.AuthorNickName}\nTitle:{resp.Data.Title}\nDesc:{resp.Data.Description}";
                        ShareInfo = string.Empty;

#if ANDROID
                        if (saveToPhone)
                        {
                            var results = new List<string>();
                            Log += "\n -*- Downloading static files...";
                            foreach (var file in resp.Data.StaticFiles)
                            {
                                string static_file_url = $"http://{HostName}:{Port}{file}";
                                //string result = await FileSaver.SaveFileFromUrl(static_file_url, "/sdacrd/dcim/xhsd", Path.GetFileName(file));
                                string result = Platforms.Android.Utils.FileUtils.WriteFileToDCIM(Path.GetFileName(file), await browser.GetStreamAsync(static_file_url));
                                Log += "\n" + result;
                                results.Add(result);
                            }
                            Platforms.Android.Utils.FileUtils.NotifyGalleryOfNewImage([.. results]);
                        }
#endif
                    }
                }
                catch (Exception ex)
                {
                    Log = ex.Message + "\n" + ex.StackTrace;
                }
            });
        }

    }

}
