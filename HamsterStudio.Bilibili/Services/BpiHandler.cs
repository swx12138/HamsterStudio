using HamsterStudio.Bilibili.Constants;

namespace HamsterStudio.Bilibili.Services;

class BpiHandler(HttpMessageHandler innerHandler) : DelegatingHandler(innerHandler)
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.RequestUri.AbsolutePath == SystemConsts.ConclusionPath)
        {
            var uriBuilder = new UriBuilder(request.RequestUri);
            uriBuilder.Query = BpiSign.Sign(request.RequestUri.Query);
            request.RequestUri = uriBuilder.Uri;
        }
        return await base.SendAsync(request, cancellationToken);
    }
}
