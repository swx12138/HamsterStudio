namespace HamsterStudio.Web.Strategies.Request;

public delegate void HttpRequestSubstantiator(HttpRequestMessage request);

public class AuthenticRequestStrategy(HttpClient client, HttpRequestSubstantiator? substantiator = null) : IRequestStrategy
{
    public async Task<HttpResponseMessage> GetResponseAsync(Uri uri)
    {
        ArgumentNullException.ThrowIfNull(uri, nameof(uri));
        HttpRequestMessage request = new(HttpMethod.Get, uri);
        return await SendAsync(request);
    }

    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage message)
    {
        substantiator?.Invoke(message);
        var response = await client.SendAsync(message);
        response.EnsureSuccessStatusCode(); // 确保响应成功   
        return response;
    }
}
