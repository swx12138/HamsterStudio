using NetCoreServer;

namespace HamsterStudio.Web.Routing.Routes
{
    public class Index : IRoute
    {
        public bool IsMyCake(string url)
        {
            return url == "/index";
        }

        public HttpResponse GetHttpResponse(HttpRequest request)
        {
            return new HttpResponse().MakeGetResponse("Hello world!");
        }
    }
}
