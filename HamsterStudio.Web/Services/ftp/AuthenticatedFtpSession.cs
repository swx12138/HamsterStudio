using Microsoft.Extensions.Logging;
using NetCoreServer;

namespace HamsterStudio.Web.Services.ftp;

/// <summary>
/// 带身份验证的 FTP 会话
/// 派生自 FtpSession，重写 HandlePass 实现用户列表校验
/// </summary>
public class AuthenticatedFtpSession : FtpSession
{
    private readonly IReadOnlyList<FtpUser> _authUsers;

    public AuthenticatedFtpSession(
        TcpServer server,
        string rootDirectory,
        ILogger logger,
        IReadOnlyList<FtpUser> authUsers)
        : base(server, rootDirectory, logger)
    {
        _authUsers = authUsers;
    }

    protected override void HandlePass(string password)
    {
        if (string.IsNullOrEmpty(_userName))
        {
            SendResponse(503, "Login with USER first.");
            return;
        }

        var user = _authUsers.FirstOrDefault(
            u => u.Username.Equals(_userName, StringComparison.OrdinalIgnoreCase));

        if (user == null)
        {
            _logger.LogWarning("FTP 认证: 用户 {UserName} 不在允许列表中", _userName);
            SendResponse(530, "Login incorrect.");
            return;
        }

        if (!user.VerifyPassword(password))
        {
            _logger.LogWarning("FTP 认证: 用户 {UserName} 密码错误", _userName);
            SendResponse(530, "Login incorrect.");
            return;
        }

        // 认证成功：设置用户专属目录
        if (user.HomeDirectory != null)
        {
            _currentDirectory = user.HomeDirectory;
        }

        _state = FtpSessionState.LoggedIn;
        _logger.LogInformation("FTP 认证: 用户 {UserName} 登录成功 (目录: {Dir})", _userName, _currentDirectory);
        SendResponse(230, "User logged in, proceed.");
    }
}
