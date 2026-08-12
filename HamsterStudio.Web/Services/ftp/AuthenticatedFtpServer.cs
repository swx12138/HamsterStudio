using Microsoft.Extensions.Logging;
using NetCoreServer;
using System.Net;

namespace HamsterStudio.Web.Services.ftp;

/// <summary>
/// 带身份验证的 FTP 服务器
/// 派生自 FtpServer，注入用户列表并创建 AuthenticatedFtpSession
/// </summary>
public class AuthenticatedFtpServer : FtpServer
{
    private readonly IReadOnlyList<FtpUser> _users;

    public AuthenticatedFtpServer(
        string address,
        int port,
        string rootDirectory,
        ILogger logger,
        IReadOnlyList<FtpUser> users)
        : base(address, port, rootDirectory, logger)
    {
        _users = users;
    }

    public AuthenticatedFtpServer(
        int port,
        string rootDirectory,
        ILogger logger,
        IReadOnlyList<FtpUser> users)
        : this(IPAddress.Any.ToString(), port, rootDirectory, logger, users)
    {
    }

    protected override TcpSession CreateSession()
    {
        return new AuthenticatedFtpSession(this, RootDirectory, _logger, _users);
    }
}
