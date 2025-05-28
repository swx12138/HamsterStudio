namespace HamsterStudio.Barefeet.Logging;

public class LoggingHandler(HttpMessageHandler innerHandler, Action<HttpRequestMessage> action) : DelegatingHandler(innerHandler)
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        action?.Invoke(request);
        var response = await base.SendAsync(request, cancellationToken);
        return response;
    }
}
