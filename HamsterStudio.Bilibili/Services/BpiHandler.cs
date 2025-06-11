using HamsterStudio.Bilibili.Constants;

namespace HamsterStudio.Bilibili.Services;

class BpiHandler(HttpMessageHandler innerHandler) : DelegatingHandler(innerHandler)
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.RequestUri.AbsolutePath == SystemConsts.ConclusionPath)
        {
            var uriBuilder = new UriBuilder(request.RequestUri);
            uriBuilder.Query = "?bvid=BV1L94y1H7CV&cid=1335073288&up_mid=297242063&wts=1701546363&w_rid=1073871926b3ccd99bd790f0162af634" ?? BpiSign.Sign(request.RequestUri.Query);
            request.RequestUri = uriBuilder.Uri;
        }
        return await base.SendAsync(request, cancellationToken);
    }
}
