using HamsterStudio.Barefeet.Services;
using HamsterStudio.Web.DataModels;
using HamsterStudio.Web.Services.ftp;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace HamsterStudio.Web.Services;

/// <summary>
/// FTP 服务器状态码
/// </summary>
public enum FileTransferProtocolStatusCode
{
    ServerReady = 220,
    NeedPassword = 331,
    LoginSuccess = 230,
}

/// <summary>
/// 向外提供 FTP 服务
/// 封装 FtpServer 的启动、停止和状态管理
/// </summary>
public class FileTransferProtocolService(DirectoryMgmt directoryMgmt, ILogger<FileTransferProtocolService> logger) : IDisposable
{
    private FtpServer? _server;

    /// <summary>
    /// 监听地址
    /// </summary>
    public string HostName { get; set; } = "0.0.0.0";

    /// <summary>
    /// 监听端口
    /// </summary>
    public int Port { get; set; } = 5002;

    /// <summary>
    /// FTP 根目录
    /// </summary>
    public string RootDirectory { get; set; } = Path.Combine(directoryMgmt.StorageHome, "FtpRoot");

    /// <summary>
    /// 允许登录的用户列表
    /// </summary>
    public List<FtpUser> Users { get; } = new()
    {
        FtpUser.Create("admin", "admin123"),
    };

    /// <summary>
    /// 服务器是否正在运行
    /// </summary>
    public bool IsRunning => _server?.IsStarted ?? false;

    /// <summary>
    /// 启动 FTP 服务器
    /// </summary>
    public FileTransferProtocolStatusCode Start()
    {
        if (_server != null)
        {
            logger.LogWarning("FTP 服务器已在运行中");
            return FileTransferProtocolStatusCode.ServerReady;
        }

        try
        {
            if (Users.Count == 0)
            {
                throw new InvalidOperationException("未配置任何 FTP 用户，请先添加用户到 Users 列表");
            }

            _server = new AuthenticatedFtpServer(HostName, Port, RootDirectory, logger, Users);
            if (!_server.Start())
            {
                logger.LogError($"FTP 服务器启动失败: {HostName}:{Port}，端口可能被占用");
                _server.Dispose();
                _server = null;
                throw new InvalidOperationException($"无法绑定到 {HostName}:{Port}，端口可能已被占用");
            }

            logger.LogInformation($"FTP 服务器已启动: ftp://{HostName}:{Port}，根目录: {RootDirectory}");
            return FileTransferProtocolStatusCode.ServerReady;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"FTP 服务器启动异常: {ex.Message}");
            _server?.Dispose();
            _server = null;
            throw;
        }
    }

    /// <summary>
    /// 停止 FTP 服务器
    /// </summary>
    public void Stop()
    {
        if (_server != null)
        {
            _server.Stop();
            _server.Dispose();
            _server = null;
            logger.LogInformation("FTP 服务器已停止");
        }
    }

    /// <summary>
    /// 重启 FTP 服务器
    /// </summary>
    public void Restart()
    {
        Stop();
        Start();
    }

    public void Dispose()
    {
        Stop();
        GC.SuppressFinalize(this);
    }

    public void RefreshUserList()
    {
        Users.Clear();

        var filePath = Path.Combine(directoryMgmt.TemporaryHome, "FtpUserList.json");
        if (!File.Exists(filePath))
        {
            logger.LogWarning("FTP 用户配置文件不存在: {Path}", filePath);
            return;
        }

        var text = File.ReadAllText(filePath);
        var config = JsonSerializer.Deserialize<FtpUserConfiguration>(text);
        if (config == null)
        {
            logger.LogError("FTP 用户配置文件反序列化失败: {Path}", filePath);
            return;
        }

        logger.LogInformation("已加载 FTP 用户配置: {Name}", config.Name);
        foreach (var user in config.Users)
        {
            Users.Add(FtpUser.Create(user.Name, user.Password, user.Home));
        }
        logger.LogInformation("共 {Count} 个用户", Users.Count);
    }
}
