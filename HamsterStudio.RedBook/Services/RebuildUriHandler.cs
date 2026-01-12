using HamsterStudio.Barefeet.Logging;
using System.Diagnostics;

namespace HamsterStudio.RedBook.Services;

public class RebuildUriHandler : DelegatingHandler
{
    public RebuildUriHandler(HttpMessageHandler innerHandler)
        : base(innerHandler)
    {
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request.RequestUri.Query == string.Empty)
        {
            var uriBuilder = new UriBuilder(request.RequestUri!);
            uriBuilder.Query = "imageView2/format/png";
            request.RequestUri = uriBuilder.Uri;
        }

        // 输出请求地址
        Trace.TraceInformation($"[Request] {request}");

        var response = await base.SendAsync(request, cancellationToken);
        return response;
    }
}

