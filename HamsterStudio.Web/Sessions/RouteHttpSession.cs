using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Web.Interfaces;
using NetCoreServer;
using System.Net.Sockets;

namespace HamsterStudio.Web.Sessions
{

    public class RouteHttpSession(RouteHttpServer server) : HttpSession(server)
    {
        protected override void OnReceivedRequest(HttpRequest request)
        {
            Logger.Shared.Trace(request.ToString());

            foreach (var route in server.Routes)
            {
                if (route.IsMyCake(request.Url))
                {
                    SendResponse(route.GetHttpResponse(request));
                }
            }
        }

        protected override void OnReceivedRequestError(HttpRequest request, string error)
        {
            Logger.Shared.Warning($"Request error: {error}");
        }

        protected override void OnError(SocketError error)
        {
            Logger.Shared.Warning($"HTTP session caught an error: {error}");
        }

    }

    public class RouteHttpServer(int port) : HttpServer("192.168.0.101", port)
    {
        public List<IRoute> Routes { get; private set; } = [];

        protected override TcpSession CreateSession()
        {
            return new RouteHttpSession(this);
        }

        protected override void OnError(SocketError error)
        {
            Logger.Shared.Warning($"HTTP server caught an error: {error}");
        }

    }

}
