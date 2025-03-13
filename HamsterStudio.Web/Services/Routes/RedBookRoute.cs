using HamsterStudio.Web.Interfaces;
using NetCoreServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudio.Web.Services.Routes
{
    public class RedBookRoute : IRoute
    {
        public event EventHandler<HttpListenerRequest>? Crush;

        public bool IsMyCake(string url)
        {
            return url.StartsWith("/xhs", StringComparison.CurrentCultureIgnoreCase);
        }

        public void Response(HttpListenerRequest request, ref HttpListenerResponse response)
        {
            response.Redirect("http://127.0.0.1:8899");
        }

        public HttpResponse GetHttpResponse(HttpRequest request)
        {
            return new HttpResponse().MakeOkResponse();
        }
    }
}