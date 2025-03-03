using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace HamsterStudio.Web.Request
{
    public class FakeBrowser
    {
        private readonly HttpClient _Client;

        public string? Referer { get; set; } = null;

        public string? Cookies { get; set; } = null;

        #region Singleton

        private static readonly Lazy<FakeBrowser> CommonClient_ = new(() => new FakeBrowser());     // 线程安全的懒加载

        public static FakeBrowser CommonClient => CommonClient_.Value;

        #endregion

        public FakeBrowser(bool includeCredentials = false)
        {
            _Client = new HttpClient
            {
                //Timeout = Timeout.InfiniteTimeSpan
            };
            _Client.DefaultRequestHeaders.UserAgent.ParseAdd(Constants.USER_AGENT_Edge);
        }

        public HttpRequestMessage CreateRequest(HttpMethod method, string api, HttpContent? content = null, RangeHeaderValue? range = null)
        {
            //Logger.Inst.Info($"[{method}] {api}");

            HttpRequestMessage request = new(method, api);

            // 设置User-Agent
            request.Headers.Add("User-Agent", Constants.USER_AGENT_Edge);

            if (Cookies != null) { request.Headers.Add("Cookie", Cookies); }
            if (Referer != null) { request.Headers.Add("Referer", Referer); }
            if (content != null) { request.Content = content; }
            if (range != null) { request.Headers.Range = range; }

            return request;
        }

        private HttpContent HandleResponse(HttpResponseMessage resp)
        {
            Debug.WriteLine($"[StatusCode] {resp.StatusCode}");
            resp.EnsureSuccessStatusCode();
            return resp.Content;
        }

        public async Task<HttpContent> FetchAsync(HttpMethod method, string api, HttpContent? content = null, RangeHeaderValue? range = null)
        {
            Debug.WriteLine($"async [{method}] {api}");
            HttpRequestMessage httpRequest = CreateRequest(method, api, content, range);
            HttpResponseMessage response = await _Client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);
            return HandleResponse(response);
        }

        /// <summary>
        /// 单独获取响应头
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<HttpContentHeaders> GetHeadersAsync(string url)
        {
            var request = CreateRequest(HttpMethod.Head, url);
            HttpResponseMessage response = await _Client.SendAsync(request);
            return response.Content.Headers;
        }

        public async Task<bool> DoServerSupportContentRange(string url)
        {
            var request = CreateRequest(HttpMethod.Head, url, range: new(0, 0));
            HttpResponseMessage response = await _Client.SendAsync(request);
            return response.StatusCode == HttpStatusCode.PartialContent;
        }

        /// <summary>
        /// GET一个字符串
        /// </summary>
        /// <param name="api"></param>
        /// <returns></returns>
        public async Task<string> GetStringAsync(string api)
        {
            var resp = await FetchAsync(HttpMethod.Get, api);
            return await resp.ReadAsStringAsync();
        }

        public HttpResponseMessage Fetch(HttpMethod method, string api)
        {
            Debug.WriteLine($"[{method}] {api}");
            HttpRequestMessage httpRequest = CreateRequest(method, api);
            HttpResponseMessage response = _Client.SendAsync(httpRequest).Result;
            return response;
        }

        public async Task<string> GetString(string api)
        {
            var resp = Fetch(HttpMethod.Get, api);
            return await HandleResponse(resp).ReadAsStringAsync();
        }

        /// <summary>
        /// Get一个Stream
        /// </summary>
        /// <param name="api"></param>
        /// <returns></returns>
        public async Task<Stream> GetStreamAsync(string api)
        {
            var resp = await FetchAsync(HttpMethod.Get, api);
            return await resp.ReadAsStreamAsync();
        }

        public async Task<Stream> GetStream(string api)
        {
            var resp = Fetch(HttpMethod.Get, api);
            return await HandleResponse(resp).ReadAsStreamAsync();
        }

        /// <summary>
        /// 获取某个段
        /// </summary>
        /// <param name="api"></param>
        /// <returns></returns>
        public async Task<Stream> GetStreamAsync(string api, int start, int length)
        {
            var resp = await FetchAsync(HttpMethod.Get, api, range: new(start, start + length - 1));
            return await resp.ReadAsStreamAsync();
        }

        /// <summary>
        /// Post
        /// </summary>
        /// <param name="api"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<string> PostAsync(string api, object data)
        {
            var resp = await FetchAsync(HttpMethod.Post, api, JsonContent.Create(data));
            return await resp.ReadAsStringAsync();
        }
    }
}
