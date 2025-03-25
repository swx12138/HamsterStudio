using HamsterStudio.Barefeet.Logging;
using NetCoreServer;
using System.Net.Sockets;

namespace HamsterStudio.Web.Routing
{
    public class RouteHttpSession(RouteHttpServer server) : HttpsSession(server)
    {
        protected override void OnReceivedRequest(HttpRequest request)
        {
            var resp = server.RouteMap.Response(request);
            SendResponse(resp);
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

}
