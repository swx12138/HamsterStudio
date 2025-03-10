using HamsterStudio.Web.Interfaces;
using HamsterStudio.Web.Services.Routes;
using System.Diagnostics;
using System.Net;

namespace HamsterStudio.Web.Services
{
    public class RouteService : IRouteService
    {
        private readonly List<IRoute> routes = [];
        private readonly DefaultRoute defaultRoute = new();

        public bool Response(HttpListenerRequest request, ref HttpListenerResponse response)
        {
            foreach (var route in routes)
            {
                if (route.IsMyCake(request.RawUrl))
                {
                    Trace.WriteLine($"{request.HttpMethod} {request.Url}");
                    route.Response(request, ref response);
                    return true;
                }
            }
            defaultRoute.Response(request, ref response);
            return false;
        }

        public void RegisterRoute(IRoute route)
        {
            if (!routes.Contains(route))
            {
                routes.Add(route);
            }
        }

    }
}
