namespace HamsterStudio.Web.Strategies.StreamCopy;

public interface IHttpContentCopyStrategy
{
    Task<Stream> CopyToStream(HttpContent content);
}
