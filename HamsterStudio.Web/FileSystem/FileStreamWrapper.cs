using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudio.Web.FileSystem;

/// <summary>
/// 包装文件流，在释放时自动删除临时文件
/// </summary>
sealed class FileStreamWrapper : Stream
{
    private readonly FileStream _innerStream;
    private readonly string _filePath;
    private readonly bool _deleteFileOnDispose;
    private bool _disposed = false;

    public FileStreamWrapper(FileStream innerStream, string filePath, bool deleteFileOnDispose)
    {
        _innerStream = innerStream;
        _filePath = filePath;
        _deleteFileOnDispose = deleteFileOnDispose;
    }

    public override bool CanRead => _innerStream.CanRead;
    public override bool CanSeek => _innerStream.CanSeek;
    public override bool CanWrite => _innerStream.CanWrite;
    public override long Length => _innerStream.Length;
    public override long Position
    {
        get => _innerStream.Position;
        set => _innerStream.Position = value;
    }

    public override void Flush() => _innerStream.Flush();
    public override Task FlushAsync(CancellationToken cancellationToken) => _innerStream.FlushAsync(cancellationToken);

    public override int Read(byte[] buffer, int offset, int count) => _innerStream.Read(buffer, offset, count);

    // 使用 Memory<T> 重载提高性能
    public override int Read(Span<byte> buffer) => _innerStream.Read(buffer);

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        => _innerStream.ReadAsync(buffer, offset, count, cancellationToken);

    // 使用 Memory<byte> 重载提高性能（符合 CA1844）
    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        => _innerStream.ReadAsync(buffer, cancellationToken);

    public override long Seek(long offset, SeekOrigin origin) => _innerStream.Seek(offset, origin);
    public override void SetLength(long value) => _innerStream.SetLength(value);

    public override void Write(byte[] buffer, int offset, int count) => _innerStream.Write(buffer, offset, count);

    // 使用 ReadOnlySpan<T> 重载提高性能
    public override void Write(ReadOnlySpan<byte> buffer) => _innerStream.Write(buffer);

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        => _innerStream.WriteAsync(buffer, offset, count, cancellationToken);

    // 使用 ReadOnlyMemory<byte> 重载提高性能（符合 CA1844）
    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        => _innerStream.WriteAsync(buffer, cancellationToken);

    public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
        => _innerStream.BeginRead(buffer, offset, count, callback, state);

    public override int EndRead(IAsyncResult asyncResult) => _innerStream.EndRead(asyncResult);

    public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
        => _innerStream.BeginWrite(buffer, offset, count, callback, state);

    public override void EndWrite(IAsyncResult asyncResult) => _innerStream.EndWrite(asyncResult);

    public override void CopyTo(Stream destination, int bufferSize) => _innerStream.CopyTo(destination, bufferSize);

    public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        => _innerStream.CopyToAsync(destination, bufferSize, cancellationToken);

    ~FileStreamWrapper()
    {
        if (!_disposed)
        {
            Dispose(false);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _innerStream.Dispose();
                if (_deleteFileOnDispose && File.Exists(_filePath))
                {
                    try { File.Delete(_filePath); } catch { /* 忽略删除异常 */ }
                }
            }
            _disposed = true;
        }
        base.Dispose(disposing);
    }

    // 添加异步释放支持
    public override async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            await _innerStream.DisposeAsync();
            if (_deleteFileOnDispose && File.Exists(_filePath))
            {
                try { File.Delete(_filePath); } catch { /* 忽略删除异常 */ }
            }
            _disposed = true;
        }
    }
}

