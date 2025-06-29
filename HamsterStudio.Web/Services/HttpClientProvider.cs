using HamsterStudio.Barefeet.Constants;
using HamsterStudio.Barefeet.Logging;

namespace HamsterStudio.Web.Services;

public class HttpClientProvider
{
    private Lazy<HttpClient> _lazyClient = new(() =>
    {
        var cli = new HttpClient(new LoggingHandler(new HttpClientHandler()));
        cli.DefaultRequestHeaders.Add("User-Agent", BrowserConsts.EdgeUserAgent);
        return cli;
    });
    public HttpClient HttpClient => _lazyClient.Value;
}
