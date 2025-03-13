using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Toolkits.Logging;
using HamsterStudio.Web.Interfaces;
using HamsterStudio.Web.Services;
using HamsterStudio.Web.Services.Routes;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows.Input;

namespace HamsterStudio.HttpServer.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _hostName = "127.0.0.1";

        [ObservableProperty]
        private string _portNumber = "8898";

        public IRouteService RouteService { get; private set; }

        private HttpListener _listener;

        public bool IsServerAlive => _listener?.IsListening ?? false;

        public ICommand StartServeCommand { get; }
        public ICommand StopServeCommand { get; }

        private readonly BackgroundWorker _worker = new() { WorkerSupportsCancellation = true };

        [ObservableProperty]
        private long _totalRequest = 0;

        public MainViewModel() 
        {
            RouteService = new RouteService();

            string url = $"http://{HostName}:{PortNumber}/";
            _listener = new();
            _listener.Prefixes.Add(url);
            StartServeCommand = new RelayCommand(() => StartServe());
            StopServeCommand = new RelayCommand(() => StopServe());

            _worker.DoWork += Worker_DoWork;
        }

        public bool StartServe()
        {
            try
            {
                if (_listener.IsListening) { Logger.Shared.Warning("Server is already running."); }
                else
                {
                    _listener.Start();
                    string paths = string.Join(" & ", _listener.Prefixes.First());
                    Logger.Shared.Information($"Server running @ {paths}");
                }

                if (_worker.IsBusy) { Logger.Shared.Warning("Server is already running.-"); }
                else { _worker.RunWorkerAsync(); }
            }
            finally { OnPropertyChanged(nameof(IsServerAlive)); }

            return true;
        }

        public bool StopServe()
        {
            try
            {
                if (!_listener.IsListening)
                {
                    Logger.Shared.Warning("Server was not satarted.");
                    return false;
                }
                _listener.Stop();
                return true;
            }
            finally
            {
                OnPropertyChanged(nameof(IsServerAlive));
            }
        }

        private void Worker_DoWork(object? sender, DoWorkEventArgs e)
        {
            // 处理传入的请求
            while (!e.Cancel)
            {
                // 等待请求
                HttpListenerContext context = _listener.GetContext();

                // 获取请求对象和响应对象
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                // 设置允许跨域请求
                response.Headers.Add("Access-Control-Allow-Origin", "*");

                Trace.TraceInformation($"[{nameof(HttpServer)}] <= [{request.HttpMethod}] {request.Url}");
                if (request.HttpMethod == "OPTIONS")
                {
                    // 处理OPTIONS请求
                    response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE");
                    response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
                    response.Headers.Add("Access-Control-Max-Age", "86400"); // 预检结果的缓存时间，单位为秒
                }
                else
                {
                    TotalRequest++;
                    RouteService.Response(request, ref response);
                }
                response.Close();    // 关闭响应
            }
        }
    }

}
