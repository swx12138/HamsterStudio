using HamsterStudio.Barefeet.Logging;
using NetCoreServer;
using System.Threading.Tasks;

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

        public async Task<HttpResponse> Response(HttpRequest request)
        {
            Logger.Shared.Trace($"Received {request.Url}({request.Method})");
            foreach (var route in Routes)
            {
                if (route.IsMyCake(request.Url))
                {
                    var resp = await route.GetHttpResponse(request);
                    return resp;
                }
            }
            return await defaultRoute.GetHttpResponse(request);
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
