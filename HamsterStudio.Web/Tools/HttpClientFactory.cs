namespace HamsterStudio.Web.Tools;

public class HttpClientFactory
{
    public HttpClient CreateHttpClient(Dictionary<string, string> headers)
    {
        var client = new HttpClient();
        foreach (var header in headers)
        {
            client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
        }
        return client;
    }

}


