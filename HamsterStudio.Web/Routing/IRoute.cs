using NetCoreServer;

namespace HamsterStudio.Web.Routing
{
    public interface IRoute
    {
        bool IsMyCake(string url);

        public HttpResponse GetHttpResponse(HttpRequest request);

    }
}
