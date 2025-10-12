using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudio.Web.Utilities;

public static class RedirectResolver
{
    public static async Task<string> GetFinalUrlAsync(string url, HttpClient httpClient)
    {
        try
        {
            var response = await httpClient.GetAsync(url);

            // 如果是重定向状态码
            if (response.StatusCode == System.Net.HttpStatusCode.Redirect ||
                response.StatusCode == System.Net.HttpStatusCode.MovedPermanently ||
                response.StatusCode == System.Net.HttpStatusCode.Found ||
                response.StatusCode == System.Net.HttpStatusCode.SeeOther ||
                response.StatusCode == System.Net.HttpStatusCode.TemporaryRedirect ||
                response.StatusCode == System.Net.HttpStatusCode.PermanentRedirect)
            {
                // 获取重定向地址
                if (response.Headers.Location != null)
                {
                    var redirectUrl = response.Headers.Location.ToString();

                    // 处理相对路径
                    if (!redirectUrl.StartsWith("http"))
                    {
                        var baseUri = new Uri(url);
                        redirectUrl = new Uri(baseUri, redirectUrl).ToString();
                    }

                    return redirectUrl;
                }
            }

            // 如果不是重定向，返回原URL
            return url;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"获取重定向地址时出错: {ex.Message}");
            return url;
        }
    }

    // 递归获取最终URL（处理多次重定向）
    public static async Task<string> GetFinalUrlRecursiveAsync(string url, HttpClient? httpClient = null, int maxRedirects = 15)
    {
        var currentUrl = url;
        int redirectCount = 0;

        httpClient ??= new HttpClient(new HttpClientHandler()
        {
            AllowAutoRedirect = false
        });

        while (redirectCount < maxRedirects)
        {
            var nextUrl = await GetFinalUrlAsync(currentUrl, httpClient);

            if (nextUrl == currentUrl)
            {
                // 没有重定向，返回当前URL
                return currentUrl;
            }

            currentUrl = nextUrl;
            redirectCount++;
        }

        throw new Exception($"重定向次数超过最大限制: {maxRedirects}");
    }

}
