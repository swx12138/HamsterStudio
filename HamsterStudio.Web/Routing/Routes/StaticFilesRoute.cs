using NetCoreServer;
using System.Text.Json;

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
            resp.SetHeader("Access-Control-Allow-Origin", "*");

            if (request.Url == Prefix)

            {
                resp.SetHeader("Content-Type", "text/html");
                resp.SetBody("<html><body>HamsterStudio Web Server</body></html>");
                return resp;
            }
            else if (request.Url == Prefix + "image_file_list")
            {
                string resp_text = JsonSerializer.Serialize(new List<string> {
                    "/static/xiaohongshu/怎么民生路渡口也有姬子啊_1_xhs_柒个吉他_1040g3k831gnn41d9j86g5nqdthc091epagsi2s8.png",
                    "/static/xiaohongshu/怎么民生路渡口也有姬子啊_2_xhs_柒个吉他_1040g3k831gnn41d9j8605nqdthc091epsa8ohu0.png",
                    "/static/xiaohongshu/怎么民生路渡口也有姬子啊_3_xhs_柒个吉他_1040g3k831gnn41d9j83g5nqdthc091epb1cqve0.png",
                    "/static/xiaohongshu/怎么民生路渡口也有姬子啊_4_xhs_柒个吉他_1040g3k831gnn41d9j8405nqdthc091epn55f0ug.png",
                    "/static/xiaohongshu/怎么民生路渡口也有姬子啊_5_xhs_柒个吉他_1040g3k831gnn41d9j84g5nqdthc091epr5evur0.png",
                    "/static/xiaohongshu/怎么民生路渡口也有姬子啊_6_xhs_柒个吉他_1040g3k831gnn41d9j82g5nqdthc091ephc0uuu0.png",
                    "/static/xiaohongshu/怎么民生路渡口也有姬子啊_7_xhs_柒个吉他_1040g3k831gnn41d9j85g5nqdthc091epoluso00.png"
                });
                resp.SetBody(resp_text);
                return resp;
            }

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
