using Microsoft.Extensions.Logging;
using NetCoreServer;
using System.Net;

namespace HamsterStudio.Web.Services.ftp;

/// <summary>
/// FTP 服务器
/// 基于 NetCoreServer 实现的完整 FTP 协议服务器
/// </summary>
public class FtpServer : TcpServer
{
    private readonly string _rootDirectory;
    private readonly ILogger _logger;

    /// <summary>
    /// 获取 FTP 服务器的根目录
    /// </summary>
    public string RootDirectory => _rootDirectory;

    /// <summary>
    /// 创建 FTP 服务器实例
    /// </summary>
    /// <param name="address">监听地址</param>
    /// <param name="port">监听端口，默认 2122</param>
    /// <param name="rootDirectory">FTP 根目录</param>
    /// <param name="logger">日志记录器</param>
    public FtpServer(string address, int port, string rootDirectory, ILogger logger)
        : base(address, port)
    {
        _rootDirectory = Path.GetFullPath(rootDirectory);
        _logger = logger;

        if (!Directory.Exists(_rootDirectory))
            Directory.CreateDirectory(_rootDirectory);

        // 允许端口复用，避免重启时 TIME_WAIT 问题
        OptionReuseAddress = true;
        OptionNoDelay = true;

        _logger.LogInformation("FTP 服务器初始化: {Address}:{Port}, 根目录: {Root}", address, port, _rootDirectory);
    }

    /// <summary>
    /// 创建 FTP 服务器实例，绑定到所有地址
    /// </summary>
    public FtpServer(int port, string rootDirectory, ILogger logger)
        : this(IPAddress.Any.ToString(), port, rootDirectory, logger)
    {
    }

    protected override TcpSession CreateSession()
    {
        return new FtpSession(this, _rootDirectory, _logger);
    }

    protected override void OnError(System.Net.Sockets.SocketError error)
    {
        _logger.LogWarning("FTP 服务器错误: {Error}", error);
    }
}
