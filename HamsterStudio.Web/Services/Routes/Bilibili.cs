using HamsterStudio.Web.Interfaces;
using HamsterStudio.Web.Request;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace HamsterStudio.Web.Services.Routes
{
    internal class Bilibili : IRoute
    {
        public bool IsMyCake(string url)
        {
            return url == "/bilib";
        }

        public void Response(HttpListenerRequest request, ref HttpListenerResponse response)
        {
            using StreamReader sr = new(request.InputStream);
            var obj = JsonSerializer.Deserialize<JsonNode>(sr.ReadToEnd())!;



            response.FromJson(JsonSerializer.Serialize(new
            {
                code = 0,
                message = "succeed",
                total = -1,
            }));
        }
    }
}
