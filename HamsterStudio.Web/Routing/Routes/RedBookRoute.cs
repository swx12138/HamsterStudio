using NetCoreServer;

namespace HamsterStudio.Web.Routing.Routes
{
    public class RedBookRoute : IRoute
    {
        public bool IsMyCake(string url)
        {
            return url.StartsWith("/xhs", StringComparison.CurrentCultureIgnoreCase);
        }

        public HttpResponse GetHttpResponse(HttpRequest request)
        {
            var resp = new HttpResponse();
            resp.SetBegin(308);
            resp.SetHeader("Location", "http://127.0.0.1:8899/xhs");
            resp.SetHeader("Content-Length", "0");
            return new HttpResponse().MakeOkResponse();
        }
    }
}