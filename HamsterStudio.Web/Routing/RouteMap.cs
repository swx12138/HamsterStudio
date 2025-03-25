using HamsterStudio.Barefeet.Logging;
using NetCoreServer;

namespace HamsterStudio.Web.Routing
{
    public class RouteMap
    {
        public List<IRoute> Routes { get; } = [];

        private readonly DefaultRoute defaultRoute = new();

        public RouteMap()
        {
            Routes.Add(new Routes.Index());
        }

        public HttpResponse Response(HttpRequest request)
        {
            Logger.Shared.Trace($"Received {request.Url}({request.Method})");
            foreach (var route in Routes)
            {
                if (route.IsMyCake(request.Url))
                {
                    var resp = route.GetHttpResponse(request);
                    return resp;
                }
            }
            return defaultRoute.GetHttpResponse(request);
        }

        public void RegisterRoute(IRoute route)
        {
            if (!Routes.Contains(route))
            {
                Routes.Add(route);
            }
        }

    }
}
