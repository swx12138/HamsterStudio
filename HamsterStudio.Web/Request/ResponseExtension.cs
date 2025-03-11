using System.Net;
using System.Text;

namespace HamsterStudio.Web.Request
{
    public static class ResponseExtension
    {
        public static void FromPlain(this HttpListenerResponse response, string plain)
        {
            response.ContentType = "text/plain";

            byte[] buffer = Encoding.UTF8.GetBytes(plain);
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
        }

        public static void FromHtml(this HttpListenerResponse response, string html)
        {
            response.ContentType = "text/html";

            byte[] buffer = Encoding.UTF8.GetBytes(html);
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
        }

        public static void FromJson(this HttpListenerResponse response, string json)
        {
            response.ContentType = "application/json";

            byte[] buffer = Encoding.UTF8.GetBytes(json);
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
        }

        public static void FromPng(this HttpListenerResponse response, byte[] pngData)
        {
            response.ContentType = "image/png";

            response.ContentLength64 = pngData.Length;
            response.OutputStream.Write(pngData, 0, pngData.Length);
        }
    }
}
