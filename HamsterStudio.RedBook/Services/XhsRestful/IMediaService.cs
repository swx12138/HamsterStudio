using Refit;
namespace HamsterStudio.RedBook.Services.XhsRestful;


[Headers("referer: https://www.xiaohongshu.com/", "accept:image/png")]
public interface IPngService
{
    [Get("/{**token}?imageView2/format/png")]
    Task<Stream?> GetImageAsync(string token);
}

[Headers("referer: https://www.xiaohongshu.com/", "accept:image/webp")]
public interface IWebpService
{
    [Get("/{**token}")]
    Task<Stream?> GetImageAsync(string token);
}

[Headers("referer: https://www.xiaohongshu.com/", "accept:video/*")]
public interface IVideoService
{
    [Get("/{**token}")]
    Task<Stream?> GetVideoAsync(string token);
}