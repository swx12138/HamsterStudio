using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Web.FileSystem;

namespace HamsterStudio.Web.Strategies.StreamCopy;

public class FileStreamHttpContentCopyStrategy(string? tempfile = null, bool deleteFileOnDispose = true) : IHttpContentCopyStrategy
{
    private readonly bool _deleteFileOnDispose = deleteFileOnDispose;

    public async Task<Stream> CopyToStream(HttpContent content)
    {
        string filePath = tempfile ?? Path.GetTempFileName();
        Logger.Shared.Trace($"Created {filePath} for temp");

        FileStream fileStream = null;
        bool success = false;

        try
        {
            // 创建文件流，使用异步写入模式
            fileStream = new FileStream(
                filePath,
                FileMode.Create,
                FileAccess.ReadWrite,
                FileShare.None,
                bufferSize: 81920, // 使用较大的缓冲区提高性能
                useAsync: true
            );

            using var httpStream = await content.ReadAsStreamAsync();
            // 将 HTTP 内容写入文件流
            await httpStream.CopyToAsync(fileStream);

            // 重置流位置到开头，以便读取
            fileStream.Seek(0, SeekOrigin.Begin);

            success = true;
            return new FileStreamWrapper(fileStream, filePath, _deleteFileOnDispose);
        }
        catch
        {
            // 发生异常时清理资源
            fileStream?.Dispose();
            if (!success && File.Exists(filePath) && _deleteFileOnDispose)
            {
                try { File.Delete(filePath); } catch { /* 忽略删除异常 */ }
            }
            throw;
        }
    }

}
