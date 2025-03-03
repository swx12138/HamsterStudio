using HamsterStudio.Web.Interfaces;
using System.Net;

namespace HamsterStudio.Web.Services.Routes
{
    internal class HoyoLab : IRoute
    {
        public bool IsMyCake(string url)
        {
            return url == "/miyoushe";
        }

        public void Response(HttpListenerRequest request, ref HttpListenerResponse response)
        {
            //using StreamReader sr = new(request.InputStream);
            //var obj = JsonSerializer.Deserialize<JsonNode>(sr.ReadToEnd())!;

            ////var id = obj["id"]!;
            ////var urls = obj["urls"]!.AsArray()!.Select(x => x?.GetValue<string>());
            //Data.HoyoLabData data = Data.HoyoLabData.FromJson(obj);
            //Log($"{data.UserId}[{data.ImageUrls.Count()}]");
            //var names = Task.WhenAll(data.ImageUrls.Select(async (x, idx) =>
            //{
            //    var name = $"{data.UserId}_{idx}_mys_{x?.Split("/").Last()}";
            //    try
            //    {
            //        await FileSaver.SaveFileFromUrl(x!, "miyoushe", name);
            //        Log($" - {name}");
            //    }
            //    catch (Arsenal.Base.ArException.FileExistsException)
            //    {
            //        Log($" - * {name}");
            //    }
            //    return name;
            //})).Result.ToList();

            //response.FromJson(JsonSerializer.Serialize(new
            //{
            //    code = 0,
            //    message = "succeed",
            //    total = names.Count,
            //}));
        }

        static void Log<T>(T msg)
        {
            Console.WriteLine(msg);
        }
    }

}
