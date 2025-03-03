using HamsterStudio.Models;
using HamsterStudio.Web.Interfaces;
using HamsterStudio.Web.Request;
using System.Net;

namespace HamsterStudio.Web.Services.Routes
{
    public class Index : IRoute
    {
        public bool IsMyCake(string url)
        {
            return url == "/index";
        }

        public void Response(HttpListenerRequest request, ref HttpListenerResponse response)
        {
            response.StatusCode = (int)HttpStatusCode.OK;
            response.FromPlain("welcome to index.");
        }

    }
}
