using NetCoreServer;
using System.Net;

namespace HamsterStudio.Web.Routing;

internal class DefaultRoute : IRoute
{
    /*
        文本类型：

        "text/plain"：纯文本类型
        "text/html"：HTML 文档类型
        "text/css"：CSS 样式表类型
        "text/javascript"：JavaScript 脚本类型
        图像类型：

        "image/jpeg"：JPEG 图像类型
        "image/png"：PNG 图像类型
        "image/gif"：GIF 图像类型
        "image/svg+xml"：SVG 图像类型
        多媒体类型：

        "audio/mpeg"：MPEG 音频类型
        "audio/wav"：WAV 音频类型
        "video/mp4"：MP4 视频类型
        "video/quicktime"：QuickTime 视频类型
        应用程序类型：

        "application/json"：JSON 数据类型
        "application/xml"：XML 数据类型
        "application/pdf"：PDF 文档类型
        "application/octet-stream"：二进制数据类型
     */
    public void Response(HttpListenerRequest request, ref HttpListenerResponse response)
    {
        response.StatusCode = (int)HttpStatusCode.NotFound;
        response.FromHtml("<html><body><h1>404 Error,Page not found.</h1></body></html>");
    }

    public bool IsMyCake(string url)
    {
        return true;
    }

    public async Task<HttpResponse> GetHttpResponse(HttpRequest request)
    {
        var resp = new HttpResponse();
        resp.MakeErrorResponse(404);
        return resp;
    }
}
