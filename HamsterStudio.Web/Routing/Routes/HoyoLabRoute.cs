using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Web.Utilities;
using NetCoreServer;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;

namespace HamsterStudio.Web.Routing.Routes;

struct HoyoLabData
{
    [JsonPropertyName("id")]
    public string UserId { get; set; }
    
    [JsonPropertyName("urls")]
    public string[] ImageUrls { get; set; }

}

public class HoyoLabRoute(string storageDir) : IRoute
{
    private readonly string _storageDir = Path.Combine(storageDir, "miyoushe");

    public bool IsMyCake(string url)
    {
        return url == "/miyoushe";
    }

    public HttpResponse GetHttpResponse(HttpRequest request)
    {
        var data = JsonSerializer.Deserialize<HoyoLabData>(request.Body);
        Logger.Shared.Information($"HoyoLabRoute processing:{data.UserId}[{data.ImageUrls.Count()}]");

        var names = Task.WhenAll(data.ImageUrls.Select(async (imgUrl, idx) =>
        {
            var name = $"{data.UserId}_{idx}_mys_{imgUrl.Split("/").Last()}";
            try
            {
                await FileSaver.SaveFileFromUrl(imgUrl, _storageDir, name);
                Logger.Shared.Information($" - {name}");
            }
            catch (Exception)
            {
                Logger.Shared.Warning($" - * {name}");
            }
            return name;
        })).Result.ToList();

        return new HttpResponse()
            .SetBegin(200)
            .SetBody(JsonSerializer.Serialize(new
            {
                code = 0,
                message = "succeed",
                total = names.Count,
            }));
    }
}
