using HamsterStudio.Barefeet.Logging;
using NetCoreServer;
using System.Net.Sockets;

namespace HamsterStudio.Web.Routing
{
    public class RouteHttpsSession(RouteHttpsServer server) : HttpsSession(server)
    {
        protected override void OnReceivedRequest(HttpRequest request)
        {
            //Logger.Shared.Trace($"method:{request.Method}\tbody:{request.Body}");

            if (request.Method == "HEAD")
            {
                SendResponseAsync(Response.MakeHeadResponse());
            }
            else if (request.Method == "OPTIONS")
            {
                var resp = Response.MakeOptionsResponse();
                SendResponseAsync(resp);
            }
            else
            {
                var resp = server.RouteMap.Response(request).Result;
                SendResponse(resp);
            }
        }

        protected override void OnReceivedRequestError(HttpRequest request, string error)
        {
            //Logger.Shared.Warning($"Request error: {error}");
        }

        protected override void OnError(SocketError error)
        {
            //Logger.Shared.Warning($"HTTP session caught an error: {error}");
        }

    }

}
