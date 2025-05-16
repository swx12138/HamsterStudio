using NetCoreServer;

namespace HamsterStudio.Web.Routing.Routes
{
    public class StaticFilesRoute : IRoute
    {
        public string Prefix { get; private set; }
        private string StaticFilesLocation { get; set; }

        public StaticFilesRoute(string local_path, string prefix)
        {
            Prefix = prefix;
            if (!Prefix.EndsWith('/'))
            {
                Prefix += '/';
            }
            StaticFilesLocation = local_path;
        }

        public bool IsMyCake(string url)
        {
            return url.StartsWith(Prefix);
        }

        public async Task<HttpResponse> GetHttpResponse(HttpRequest request)
        {

            var resp = new HttpResponse();
            resp.SetBegin(200);
            resp.SetHeader("Content-Type", "application/octet-stream");

            string filePath = Path.Combine(StaticFilesLocation, Uri.UnescapeDataString(request.Url.Substring(Prefix.Length)));
            if (!File.Exists(filePath))
            {
                return resp.MakeErrorResponse(410);
            }

            resp.SetHeader("content-type", "image/png");
            //resp.SetHeader("Content-Disposition", $"attachment; filename=\"{Path.GetFileName(filePath)}\"");  //  attachment意味着内容应该被下载到本地，大多数浏览器会呈现一个“保存为”的对话框，并将 filename 的值预填为下载后的文件名

            // 读取文件内容
            byte[] fileBytes = await File.ReadAllBytesAsync(filePath);
            resp.SetBody(fileBytes);

            return resp;
        }


    }
}
