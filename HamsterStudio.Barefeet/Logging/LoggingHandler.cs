using Microsoft.Extensions.Logging;

namespace HamsterStudio.Barefeet.Logging;

public class LoggingHandler(HttpMessageHandler innerHandler, Action<HttpRequestMessage>? action = null, ILogger? logger = null) : DelegatingHandler(innerHandler)
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (action is null)
            HandleRequest(request);
        else
        {
            action?.Invoke(request);
        }

        var response = await base.SendAsync(request, cancellationToken);
        return response;
    }


    private void HandleRequest(HttpRequestMessage msg)
    {
        logger?.LogDebug(msg.ToString());
    }
}
