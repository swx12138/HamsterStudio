using HamsterStudio.Barefeet.Logging;
using HamsterStudio.RedBook.Interfaces;
using HamsterStudio.RedBook.Models;
using HamsterStudio.RedBook.Services.Parsing;
using HamsterStudio.Web.Routing;
using System.Text.Json;
using HttpRequest = NetCoreServer.HttpRequest;
using HttpResponse = NetCoreServer.HttpResponse;

namespace HamsterStudio.RedBook.Services
{
    public class RedBookRoute : IRoute
    {
        public static readonly List<string> _oldDir = [
            @"D:\HamsterStudioHome\_XHS_",
            @"D:\HamsterStudioHome\_XHS_\Download\葡萄糖_Glucose"
        ];

        private readonly string _storageDir;

        public string StorageDir => _storageDir;
        private IRedBookParser Parser { get; } = new RedBookNoteParser();
        private RedBookDownloadService DownloadService { get; }

        public RedBookRoute(string rootDir)
        {
            _storageDir = Path.Combine(rootDir, "xiaohongshu");
            //DownloadService = new(new Download.MediaDownloader(), _storageDir);
        }

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

            var data = Parser.GetNoteData(body.Url);
            if (data == null)
            {
                return resp.MakeErrorResponse(502);
            }

            DumpJson(Path.Combine(_storageDir + "/.hamster/", $"note_{data.CurrentNoteId}.json"), data);

            var svrResp = await DownloadService.DownloadNoteAsync(data);
            resp.SetBody(JsonSerializer.Serialize(svrResp));

            return resp;
        }

        private static void DumpJson(string path, NoteDataModel noteData)
        {
            try
            {
                string text = JsonSerializer.Serialize(noteData);
                File.WriteAllText(path, text);
            }
            catch (Exception ex)
            {
                Logger.Shared.Warning($"Dump json failed.{ex.Message}\n{ex.StackTrace}");
            }
        }
    }

}