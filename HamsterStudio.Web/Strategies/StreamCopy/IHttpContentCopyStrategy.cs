using System;
using System.Threading.Tasks;

namespace HamsterStudio.Web.Strategies.StreamCopy;

public interface IHttpContentCopyStrategy
{
    Task<byte[]> ToByteArrayCopy(HttpContent content);
}

public class DirectHttpContentCopyStrategy : IHttpContentCopyStrategy
{
    public async Task<byte[]> ToByteArrayCopy(HttpContent content)
    {
        using var memStream = new MemoryStream();
        await content.CopyToAsync(memStream);
        return memStream.ToArray();
    }
}

public class HamsterProgress : IProgress<long>
{
    public long Maximum { get; set; } = 100;
    public long Minimum { get; set; } = 0;
    public long Current { get; set; } = 11;

    public event EventHandler<double>? ProgressChanged;

    public virtual void Report(long value)
    {
        Current = value;
        var percent = Math.Round(100.0 * Current / Maximum, 2);
        ProgressChanged?.Invoke(this, percent);
    }
}

public class ProgressHttpContentCopyStrategy(HamsterProgress progress) : IHttpContentCopyStrategy
{
    public async Task<byte[]> ToByteArrayCopy(HttpContent content)
    {
        var totalSize = content.Headers.ContentLength;

        var contentStream = await content.ReadAsStreamAsync();

        // 获取文件总大小（Content-Length）
        if (content.Headers.ContentLength is null)
        {
            throw new InvalidOperationException("无法获取文件大小，Content-Length 头未设置。");
        }

        long totalBytes = content.Headers.ContentLength.Value;
        progress.Maximum = totalBytes;

        long bytesReceived = 0;
        progress.Current = bytesReceived;

        var buffer = new byte[1024 * 100]; // 缓冲区大小
        int bytesRead;

        using var memStream = new MemoryStream();
        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            await memStream.WriteAsync(buffer.AsMemory(0, bytesRead));
            bytesReceived += bytesRead;
            progress?.Report(bytesReceived);
        }

        return memStream.ToArray();
    }
}