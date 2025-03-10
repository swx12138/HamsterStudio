using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Web.Services.Routes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
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

        public MainWindowModel()
        {
            TabPages.Add(TabPageModel.Incubator<HamsterStudio.BraveShine.Views.MainView>("BraveShine", "BraveShine"));
            TabPages.Add(TabPageModel.Incubator<HamsterStudio.ImageTool.Views.MainView>("ImageTool", "ImageTool"));

            var httpServer = TabPageModel.Incubator<HamsterStudio.HttpServer.Views.MainView>("HttpServer", "HttpServer");
            {
                if (httpServer.Element.DataContext is HamsterStudio.HttpServer.ViewModels.MainViewModel viewModel) {

                    var bRoute = new BilibiliRoute();
                    bRoute.Crush += BiliRoute_Crush;
                    viewModel.RouteService.RegisterRoute(bRoute);                

                    var xhsRoute = new RedBookRoute();
                    viewModel.RouteService.RegisterRoute(xhsRoute);

                    viewModel.StartServe();}
            }
            TabPages.Add(httpServer);

        }

        private void BiliRoute_Crush(object? sender, HttpListenerRequest request)
        {
            StreamReader stream = new(request.InputStream);
            string raw = stream.ReadToEnd();

            var braveShine = TabPages.First(x => x.Element is HamsterStudio.BraveShine.Views.MainView);
            if (braveShine != null )
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (braveShine.Element.DataContext is not HamsterStudio.BraveShine.ViewModels.MainViewModel viewModel)
                    {
                        Logger.Shared.Error($"braveShine.Element.DataContext is not HamsterStudio.BraveShine.ViewModels.MainViewModel");
                        return;
                    }
                    viewModel.DownloadVideoByBvid(raw);
                });
            }
        }

    }

}
