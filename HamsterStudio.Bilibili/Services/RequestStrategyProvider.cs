using HamsterStudio.Web.Services;
using HamsterStudio.Web.Strategies.Request;

namespace HamsterStudio.Bilibili.Services;

public class RequestStrategyProvider(HttpClientProvider httpClientProvider)
{
    private AuthenticRequestStrategy _strategy = new(httpClientProvider.HttpClient, msg =>
    {
        msg.Headers.Referrer = new Uri($"https://www.bilibili.com");
    });
    public AuthenticRequestStrategy Strategy => _strategy;
}
