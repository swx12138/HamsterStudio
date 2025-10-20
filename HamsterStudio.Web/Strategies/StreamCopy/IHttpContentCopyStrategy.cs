namespace HamsterStudio.Web.Strategies.StreamCopy;

public interface IHttpContentCopyStrategy
{
    Task<Stream> CopyToStream(HttpContent content);
}

public class DirectHttpContentCopyStrategy : IHttpContentCopyStrategy
{
    public async Task<Stream> CopyToStream(HttpContent content)
    {
        return await content.ReadAsStreamAsync();
    }
}
