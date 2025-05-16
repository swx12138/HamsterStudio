using NetCoreServer;

namespace HamsterStudio.Web.Routing
{
    public interface IRoute
    {
        bool IsMyCake(string url);

        public Task<HttpResponse> GetHttpResponse(HttpRequest request);

    }
}
