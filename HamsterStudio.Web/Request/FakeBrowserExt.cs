using System.Net.Http.Headers;

namespace HamsterStudio.Web.Request
{
    public static class FakeBrowserExt
    {
        public static async Task<long> GetFileSize(this FakeBrowser browser, string url)
        {
            HttpContentHeaders headers = await browser.GetHeadersAsync(url);
            return headers.ContentLength ?? 0;
        }
    }
}
