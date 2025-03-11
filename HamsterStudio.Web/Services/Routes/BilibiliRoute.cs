using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Web.Interfaces;
using HamsterStudio.Web.Request;
using System.IO;
using System.Net;
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

        public event EventHandler<(HttpListenerRequest, HttpListenerResponse)>? Crush;

        public void Response(HttpListenerRequest request, ref HttpListenerResponse response)
        {
            //using StreamReader streamReader = new(request.InputStream);
            //Logger.Shared.Information(streamReader.ReadToEnd());

            Crush?.Invoke(this, (request, response));

        }
    }
}
