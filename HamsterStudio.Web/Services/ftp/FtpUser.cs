namespace HamsterStudio.Web.Services.ftp;

/// <summary>
/// FTP 用户模型，密码以 SHA256 哈希存储
/// </summary>
/// <param name="Username">用户名</param>
/// <param name="PasswordHash">密码的 SHA256 哈希（Base64 编码）</param>
/// <param name="HomeDirectory">用户专属虚拟目录，null 则使用根目录</param>
public record FtpUser(string Username, string PasswordHash, string? HomeDirectory = null)
{
    /// <summary>
    /// 从明文密码创建用户（仅在配置阶段调用）
    /// </summary>
    public static FtpUser Create(string username, string plainPassword, string? homeDirectory = null)
    {
        var hash = HashPassword(plainPassword);
        return new FtpUser(username, hash, homeDirectory);
    }

    /// <summary>
    /// 验证密码是否匹配
    /// </summary>
    public bool VerifyPassword(string plainPassword)
    {
        return PasswordHash == HashPassword(plainPassword);
    }

    private static string HashPassword(string password)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}
