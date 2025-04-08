using NetCoreServer;

namespace HamsterStudio.Web.Routing.Routes
{
    public class BilibiliRoute : IRoute
    {
        public bool IsMyCake(string url)
        {
            return url.StartsWith("/bilib");
        }

        public event EventHandler<(HttpRequest, HttpResponse)>? Crush;

        public HttpResponse GetHttpResponse(HttpRequest request)
        {
            var resp = new HttpResponse().MakeOkResponse();
            Crush?.Invoke(this, (request, resp));
            return resp;
        }
    }
}
