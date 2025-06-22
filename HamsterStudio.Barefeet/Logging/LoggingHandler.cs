namespace HamsterStudio.Barefeet.Logging;

public class LoggingHandler(HttpMessageHandler innerHandler, Action<HttpRequestMessage>? action = null) : DelegatingHandler(innerHandler)
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


    private static void HandleRequest(HttpRequestMessage msg)
    {
        Logger.Shared.Debug(msg.ToString());
    }
}
