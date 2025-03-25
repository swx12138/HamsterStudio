using HamsterStudio.Barefeet.Task;
using HamsterStudio.Web.Interfaces;
using HamsterStudio.Web.Routing.Routes;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;

namespace HamsterStudio.Web
{
    public class EasyHttpServer 
    {
        public int TotalRequest { get; private set; }

        public string Host => _Host;

        public ushort Port => _Port;

        public void Run()
        {
            try
            {
                _listener.Start();
                _worker.RunWorkerAsync();

                Trace.TraceInformation($"服务器已启动，正在监听 {Host}:{Port}/");
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message + "\n" + ex.StackTrace);
            }
        }

        private HttpListener _listener;

        private readonly string _Host;
        private readonly ushort _Port;
        private readonly BackgroundWorker _worker = new() { WorkerSupportsCancellation = true };
        private readonly IRouteService _RouteService;
        private readonly HamsterTaskManager _HamsterTaskManager;

        public EasyHttpServer(string host, ushort port,IRouteService routeService, HamsterTaskManager hamsterTaskManager)
        {
            _Host = host;
            _Port = port;
            _RouteService = routeService;
            _HamsterTaskManager = hamsterTaskManager;   

            string url = $"http://{Host}:{Port}/";
            _listener = new();
            _listener.Prefixes.Add(url);

            _RouteService.RegisterRoute(new BoundhubRoute(hamsterTaskManager));

            _worker.DoWork += Worker_DoWork;
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

                Trace.TraceInformation($"[{nameof(EasyHttpServer)}] <= [{request.HttpMethod}] {request.Url}");
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
                    _RouteService.Response(request, ref response);
                }
                response.Close();    // 关闭响应
            }
        }
    }
}
