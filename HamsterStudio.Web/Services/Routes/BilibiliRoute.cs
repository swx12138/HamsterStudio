using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Web.Interfaces;
using HamsterStudio.Web.Request;
using NetCoreServer;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace HamsterStudio.Web.Services.Routes
{
    public class BilibiliRoute : IRoute
    {
        public bool IsMyCake(string url)
        {
            return url.StartsWith("/bilib");
        }

        public event EventHandler<(HttpRequest, HttpResponse)>? Crush;

        public void Response(HttpListenerRequest request, ref HttpListenerResponse response)
        {
            //using StreamReader streamReader = new(request.InputStream);
            //Logger.Shared.Information(streamReader.ReadToEnd());

            //Crush?.Invoke(this, (request, response));

        }

        public HttpResponse GetHttpResponse(HttpRequest request)
        {
            var resp = new HttpResponse();
            Crush?.Invoke(this, (request, resp));
            return resp.MakeOkResponse();
        }
    }
}
