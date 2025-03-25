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

        private RouteHttpServer server;

        public MainWindowModel()
        {
            //Logger.Shared.AddTarget(new DiagnosticsTraceTarget("Trace"), NLog.LogLevel.Trace, NLog.LogLevel.Fatal);

            TabPages.Add(TabPageModel.Incubator<HamsterStudio.BraveShine.Views.MainView>("BraveShine", "BraveShine"));
            TabPages.Add(TabPageModel.Incubator<HamsterStudio.ImageTool.Views.MainView>("ImageTool", "ImageTool"));

            var context = new SslContext(SslProtocols.Tls12, new X509Certificate2("https/server.pfx", "qwerty"));
            server = new(context, 8898);
            {
                var bRoute = new BilibiliRoute();
                bRoute.Crush += BiliRoute_Crush;
                server.RouteMap.Routes.Add(bRoute);

                var xhsRoute = new RedBookRoute();
                server.RouteMap.Routes.Add(xhsRoute);
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
