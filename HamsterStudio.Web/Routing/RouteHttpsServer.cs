using HamsterStudio.Barefeet.Logging;
using NetCoreServer;
using System.Net.Sockets;

namespace HamsterStudio.Web.Routing
{
    public class RouteHttpsServer(SslContext context, int port) : HttpsServer(context, "192.168.0.101", port)
    {
        public RouteMap RouteMap { get; private set; } = new();

        protected override SslSession CreateSession()
        {
            return new RouteHttpsSession(this);
        }

        protected override void OnError(SocketError error)
        {
            //Logger.Shared.Warning($"HTTP server caught an error: {error}");
        }

    }

}
