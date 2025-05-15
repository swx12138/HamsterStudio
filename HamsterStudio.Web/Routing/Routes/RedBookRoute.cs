using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Web.DataModels.ReadBook;
using HamsterStudio.Web.Utilities;
using NetCoreServer;
using System.Text.Json;
using System.Threading.Tasks;

namespace HamsterStudio.Web.Routing.Routes
{
    public class RedBookRoute(string storageDir) : IRoute
    {
        private readonly string _storageDir = Path.Combine(storageDir, "xiaohongshu");
        public readonly List<string> _oldDir = [
            @"D:\HamsterStudioHome\_XHS_",
            @"D:\HamsterStudioHome\_XHS_\Download\葡萄糖_Glucose"
        ];

        public bool IsMyCake(string url)
        {
            return url.StartsWith("/xhs", StringComparison.CurrentCultureIgnoreCase);
        }

        public HttpResponse GetHttpResponse(HttpRequest request)
        {
            var resp = new HttpResponse();

            var body = JsonSerializer.Deserialize<PostBodyModel>(request.Body.Split("\r\n").ElementAt(1))!;
            if (body.Url == null) { return resp.MakeOkResponse(); }

            var data = RedBookHelper.GetNoteData(in body);
            if (data == null)
            {
                return resp.MakeOkResponse();
            }

            RedBookHelper.DumpJson(Path.Combine(_storageDir+"/.hamster/", $"note_{data.CurrentNoteId}.json"), data);

            var svrResp = RedBookHelper.Download(data, _storageDir, filename =>
            {
                return _oldDir.Any(x => File.Exists(Path.Combine(x, filename)));
            });
            resp.SetBody(JsonSerializer.Serialize(svrResp));

            return resp.MakeOkResponse();
        }
    }

}