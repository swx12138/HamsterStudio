using NetCoreServer;

namespace HamsterStudio.Web.Routing.Routes
{
    public class Index : IRoute
    {
        public bool IsMyCake(string url)
        {
            return url == "/index";
        }

        public async Task<HttpResponse> GetHttpResponse(HttpRequest request)
        {
            return new HttpResponse().MakeGetResponse("Hello world!");
        }
    }
}
