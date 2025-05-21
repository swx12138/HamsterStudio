using HamsterStudio.Web.DataModels.ReadBook;
using HamsterStudio.Web.Utilities;
using NetCoreServer;
using System.Text.Json;

namespace HamsterStudio.Web.Routing.Routes
{
    public class RedBookRoute(string storageDir) : IRoute
    {
        private readonly string _storageDir = Path.Combine(storageDir, "xiaohongshu");
        public string StorageDir => _storageDir;

        public readonly List<string> _oldDir = [
            @"D:\HamsterStudioHome\_XHS_",
            @"D:\HamsterStudioHome\_XHS_\Download\葡萄糖_Glucose"
        ];

        public bool IsMyCake(string url)
        {
            return url.StartsWith("/xhs", StringComparison.CurrentCultureIgnoreCase);
        }

        public async Task<HttpResponse> GetHttpResponse(HttpRequest request)
        {
            var resp = new HttpResponse();
            resp.SetBegin(200);

            var body = JsonSerializer.Deserialize<PostBodyModel>(request.Body.Split("\r\n").ElementAt(1))!;
            if (body.Url == null) { return resp.MakeErrorResponse(406); }

            var data = RedBookParser.GetNoteData(body.Url);
            if (data == null)
            {
                return resp.MakeErrorResponse(502);
            }

            RedBookHelper.DumpJson(Path.Combine(_storageDir + "/.hamster/", $"note_{data.CurrentNoteId}.json"), data);

            var downloader = new RedBookDownloader(_storageDir);
            var svrResp = await downloader.Download(data);
            resp.SetBody(JsonSerializer.Serialize(svrResp));

            return resp;
        }
    }

}