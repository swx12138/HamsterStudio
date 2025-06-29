namespace HamsterStudio.Web.Strategies.Request;

public interface IRequestStrategy
{
    Task<HttpResponseMessage> GetResponseAsync(Uri uri);
    Task<HttpResponseMessage> SendAsync(HttpRequestMessage message);
}
