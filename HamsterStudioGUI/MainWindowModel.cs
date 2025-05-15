using HamsterStudio.Barefeet.Interfaces;
using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Web.Routing;
using HamsterStudio.Web.Routing.Routes;
using NetCoreServer;
using System.Collections.ObjectModel;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Controls;

namespace HamsterStudioGUI
{
    class TabPageModel
    {
        public required string Title { get; set; }
        public required string Desc { get; set; }
        public required UserControl Element { get; set; }

        public static TabPageModel Incubator<T>(string title, string desc) where T : UserControl, new()
        {
            return new TabPageModel() { Title = title, Desc = desc, Element = new T() };
        }
    }

    class MainWindowModel
    {
        public ObservableCollection<TabPageModel> TabPages { get; } = [];

        private RouteHttpsServer server;

        public MainWindowModel()
        {
            //Logger.Shared.AddTarget(new DiagnosticsTraceTarget("Trace"), NLog.LogLevel.Trace, NLog.LogLevel.Fatal);

            TabPages.Add(TabPageModel.Incubator<HamsterStudio.BraveShine.Views.MainView>("BraveShine", "BraveShine"));
            TabPages.Add(TabPageModel.Incubator<HamsterStudio.ImageTool.Views.MainView>("ImageTool", "ImageTool"));
            TabPages.Add(TabPageModel.Incubator<HamsterStudio.Gallery.Views.GalleryView>("Gallery", "Gallery"));

            InitServer();

            //AppDomain.CurrentDomain.FirstChanceException += (sender, e) =>
            //{
            //    // 按异常类型筛选
            //    if (e.Exception is Exception)
            //    {
            //        Debug.WriteLine($"异常捕获: {e.Exception}");
            //        Debugger.Break(); // 强制中断调试器
            //    }
            //};
        }

        private void InitServer()
        {
            var cert = new X509Certificate2("https/localhost.pfx", "qwerty");
            if (cert.NotAfter < DateTime.Now)
            {
                Logger.Shared.Error("证书已过期！");
                return;
            }

            if (cert.NotBefore > DateTime.Now)
            {
                Logger.Shared.Error("证书尚未生效！");
                return;
            }

            var chain = new X509Chain();
            chain.Build(cert); // 检查链是否完整
            if (chain.ChainStatus.Length > 0)
            {
                foreach (var status in chain.ChainStatus)
                {
                    Logger.Shared.Error($"证书链错误: {status.StatusInformation}");
                }
                return;
            }

            var context = new SslContext(SslProtocols.Tls12, cert);
            server = new(context, 8898);
            server.OptionKeepAlive = true;

            {
                var bRoute = new BilibiliRoute();
                bRoute.Crush += BiliRoute_Crush;
                server.RouteMap.Routes.Add(bRoute);

                var xhsRoute = new RedBookRoute();
                server.RouteMap.Routes.Add(xhsRoute);

                var hyl = new HoyoLabRoute((Application.Current as IHamsterApp)!.FileStorageHome);
                server.RouteMap.Routes.Add(hyl);
            }

            server.Start();

        }

        private void BiliRoute_Crush(object? sender, (HttpRequest, HttpResponse) rr)
        {
            var (requ, resp) = rr;
            string raw = requ.Body;

            var braveShine = TabPages.First(x => x.Element is HamsterStudio.BraveShine.Views.MainView);
            if (braveShine != null)
            {
                Application.Current.Dispatcher.InvokeAsync(async () =>
                {
                    if (braveShine.Element.DataContext is not HamsterStudio.BraveShine.ViewModels.MainViewModel viewModel)
                    {
                        Logger.Shared.Error($"braveShine.Element.DataContext is not HamsterStudio.BraveShine.ViewModels.MainViewModel");
                        return;
                    }
                    string result = await viewModel.DownloadVideoByBvid(raw);
                    resp.SetBody(result);
                });
            }
        }

    }

}
