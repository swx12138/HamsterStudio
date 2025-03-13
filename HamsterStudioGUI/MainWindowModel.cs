using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Toolkits.Logging;
using HamsterStudio.Web.Services.Routes;
using HamsterStudio.Web.Sessions;
using NetCoreServer;
using System.Collections.ObjectModel;
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
            Logger.Shared.AddTarget(new DiagnosticsTraceTarget("Trace"), NLog.LogLevel.Trace, NLog.LogLevel.Fatal);

            TabPages.Add(TabPageModel.Incubator<HamsterStudio.BraveShine.Views.MainView>("BraveShine", "BraveShine"));
            TabPages.Add(TabPageModel.Incubator<HamsterStudio.ImageTool.Views.MainView>("ImageTool", "ImageTool"));

            //var httpServer = TabPageModel.Incubator<HamsterStudio.HttpServer.Views.MainView>("HttpServer", "HttpServer");
            //{
            //    if (httpServer.Element.DataContext is HamsterStudio.HttpServer.ViewModels.MainViewModel viewModel)
            //    {

            //        var bRoute = new BilibiliRoute();
            //        bRoute.Crush += BiliRoute_Crush;
            //        viewModel.RouteService.RegisterRoute(bRoute);

            //        var xhsRoute = new RedBookRoute();
            //        viewModel.RouteService.RegisterRoute(xhsRoute);

            //        viewModel.StartServe();
            //    }
            //}
            //TabPages.Add(httpServer);

            server = new(8898);

            var bRoute = new BilibiliRoute();
            bRoute.Crush += BiliRoute_Crush;
            server.Routes.Add(bRoute);

            var xhsRoute = new RedBookRoute();
            server.Routes.Add(xhsRoute);

            server.Start();
        }

        private void BiliRoute_Crush(object? sender, (HttpRequest, HttpResponse) rr)
        {
            var (requ, resp) = rr;
            //StreamReader stream = new(requ.InputStream);
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
