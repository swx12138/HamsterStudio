using HamsterStudio.Barefeet.Extensions;
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
    public string PostId { get; set; }
    
    [JsonPropertyName("userId")]
    public string UserId { get; set; }
    
    [JsonPropertyName("urls")]
    public string[] ImageUrls { get; set; }

    [JsonPropertyName("name")]
    public string UserName { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

}

public class HoyoLabRoute(string storageDir) : IRoute
{
    private readonly string _storageDir = Path.Combine(storageDir, "miyoushe");

    public bool IsMyCake(string url)
    {
        return url == "/miyoushe";
    }

    public async Task<HttpResponse> GetHttpResponse(HttpRequest request)
    {
        var data = JsonSerializer.Deserialize<HoyoLabData>(request.Body);
        Logger.Shared.Information($"HoyoLabRoute processing: {data.Title}");
        var semaphore = new SemaphoreSlim(5);
        var names = Task.WhenAll(data.ImageUrls.Select(async (imgUrl, idx) =>
        {
            var name = FileNameUtil.SanitizeFileName($"{data.Title.Trim()}_{idx}_{data.UserName.Trim()}_mys_{data.PostId.Trim()}_{data.UserId.Trim()}_{imgUrl.Split("/").Last()}");
            try
            {
                await semaphore.WaitAsync();
                await FileSaver.SaveFileFromUrl(imgUrl, _storageDir, name);
                Logger.Shared.Information($" # {name}");
            }
            catch (Exception ex)
            {
                Logger.Shared.Warning($" ! {name} ex:({ex.Message})");
            }
            finally
            {
                semaphore.Release(); // 释放信号量
            }
            return name;
        })).Result.ToList();

        Logger.Shared.Information($"[{data.UserName}]{data.Title}({data.ImageUrls.Length}) Done.");

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
