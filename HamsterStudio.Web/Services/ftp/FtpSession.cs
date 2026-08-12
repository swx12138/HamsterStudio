using Microsoft.Extensions.Logging;
using NetCoreServer;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace HamsterStudio.Web.Services.ftp;

/// <summary>
/// FTP 会话状态
/// </summary>
enum FtpSessionState
{
    Connected,
    NeedPassword,
    LoggedIn,
}

/// <summary>
/// FTP 数据传输类型
/// </summary>
enum FtpTransferType
{
    Ascii,
    Binary,
}

/// <summary>
/// FTP 数据连接模式
/// </summary>
enum FtpDataMode
{
    None,
    Active,
    Passive,
}

/// <summary>
/// 处理单个 FTP 客户端连接
/// </summary>
public class FtpSession : TcpSession
{
    private readonly string _rootDirectory;
    private string _currentDirectory;
    private FtpSessionState _state = FtpSessionState.Connected;
    private FtpTransferType _transferType = FtpTransferType.Binary;
    private FtpDataMode _dataMode = FtpDataMode.None;

    // 主动模式：客户端地址
    private IPAddress? _activeAddress;
    private int _activePort;

    // 被动模式：服务器监听
    private TcpListener? _passiveListener;

    // 重命名源路径
    private string? _renameFromPath;

    private readonly ILogger _logger;

    // 当前用户名
    private string? _userName;

    // 缓存的客户端地址（OnDisconnected 时 Socket 可能已释放）
    private string _remoteEndPoint = "(未知)";

    public FtpSession(TcpServer server, string rootDirectory, ILogger logger)
        : base(server)
    {
        _rootDirectory = Path.GetFullPath(rootDirectory);
        _currentDirectory = "/";
        _logger = logger;
    }

    protected override void OnConnected()
    {
        _remoteEndPoint = Socket.RemoteEndPoint?.ToString() ?? "(未知)";
        _logger.LogInformation("FTP: 客户端连接 {Endpoint}", _remoteEndPoint);
        SendResponse(220, "HamsterStudio FTP Server Ready");
    }

    protected override void OnDisconnected()
    {
        _logger.LogInformation("FTP: 客户端断开 {Endpoint}", _remoteEndPoint);
        CleanupDataConnection();
    }

    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        var raw = Encoding.ASCII.GetString(buffer, (int)offset, (int)size);
        _logger.LogInformation("FTP CMD: {Command}", raw.TrimEnd('\r', '\n'));

        var command = raw.TrimEnd('\r', '\n');
        var spaceIndex = command.IndexOf(' ');
        var cmd = spaceIndex > 0 ? command[..spaceIndex].ToUpperInvariant() : command.ToUpperInvariant();
        var arg = spaceIndex > 0 ? command[(spaceIndex + 1)..] : string.Empty;

        try
        {
            HandleCommand(cmd, arg);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "FTP 命令处理异常: {Message}", ex.Message);
            SendResponse(550, $"Requested action not taken: {ex.Message}");
        }
    }

    protected override void OnError(SocketError error)
    {
        _logger.LogWarning("FTP 会话错误: {Error}", error);
    }

    #region 命令分发

    private void HandleCommand(string cmd, string arg)
    {
        switch (cmd)
        {
            case "USER": HandleUser(arg); break;
            case "PASS": HandlePass(arg); break;
            case "QUIT": HandleQuit(); break;
            case "PWD":  HandlePwd(); break;
            case "CWD":  HandleCwd(arg); break;
            case "CDUP": HandleCdup(); break;
            case "TYPE": HandleType(arg); break;
            case "PASV": HandlePasv(); break;
            case "PORT": HandlePort(arg); break;
            case "LIST": HandleList(arg); break;
            case "NLST": HandleNlst(arg); break;
            case "RETR": HandleRetr(arg); break;
            case "STOR": HandleStor(arg); break;
            case "DELE": HandleDele(arg); break;
            case "RMD":  HandleRmd(arg); break;
            case "MKD":  HandleMkd(arg); break;
            case "RNFR": HandleRnfr(arg); break;
            case "RNTO": HandleRnto(arg); break;
            case "SIZE": HandleSize(arg); break;
            case "SYST": HandleSyst(); break;
            case "FEAT": HandleFeat(); break;
            case "NOOP": HandleNoop(); break;
            case "OPTS": HandleOpts(arg); break;
            case "STAT": HandleStat(arg); break;
            case "HELP": HandleHelp(); break;
            case "APPE": HandleAppe(arg); break;
            case "REST": HandleRest(arg); break;
            default:
                SendResponse(502, "Command not implemented.");
                break;
        }
    }

    #endregion

    #region 认证命令

    private void HandleUser(string userName)
    {
        _userName = userName;
        _state = FtpSessionState.NeedPassword;
        SendResponse(331, $"User {userName} okay, need password.");
    }

    private void HandlePass(string password)
    {
        // 简化认证：接受任意密码，或者允许匿名登录
        if (string.IsNullOrEmpty(_userName))
        {
            SendResponse(503, "Login with USER first.");
            return;
        }

        // TODO: 替换为实际的用户认证逻辑
        _state = FtpSessionState.LoggedIn;
        _logger.LogInformation("FTP: 用户 {UserName} 登录成功", _userName);
        SendResponse(230, "User logged in, proceed.");
    }

    private void HandleQuit()
    {
        SendResponse(221, "Goodbye.");
        Disconnect();
    }

    #endregion

    #region 导航命令

    private void HandlePwd()
    {
        RequireLogin();
        SendResponse(257, $"\"{_currentDirectory}\" is the current directory.");
    }

    private void HandleCwd(string path)
    {
        RequireLogin();

        var targetPath = ResolvePath(path);

        if (Directory.Exists(targetPath))
        {
            _currentDirectory = GetVirtualPath(targetPath);
            SendResponse(250, $"CWD command successful. \"{_currentDirectory}\" is the current directory.");
        }
        else
        {
            SendResponse(550, $"Failed to change directory: {path}");
        }
    }

    private void HandleCdup()
    {
        RequireLogin();
        HandleCwd("..");
    }

    #endregion

    #region 传输设置

    private void HandleType(string typeCode)
    {
        RequireLogin();

        _transferType = typeCode switch
        {
            "A" or "A N" => FtpTransferType.Ascii,
            "I" or "L8" => FtpTransferType.Binary,
            _ => FtpTransferType.Binary,
        };

        SendResponse(200, $"Type set to {_transferType}.");
    }

    private void HandlePasv()
    {
        RequireLogin();

        CleanupDataConnection();

        _passiveListener = new TcpListener(IPAddress.Any, 0);
        _passiveListener.Start();
        var localEndpoint = (IPEndPoint)_passiveListener.LocalEndpoint;

        // 获取服务器的外部 IP
        var localAddr = ((IPEndPoint)Socket.LocalEndPoint!).Address;
        if (IPAddress.IsLoopback(localAddr))
            localAddr = GetLocalIPAddress();

        var port = localEndpoint.Port;
        var addrBytes = localAddr.GetAddressBytes();

        _dataMode = FtpDataMode.Passive;

        SendResponse(227, $"Entering Passive Mode ({addrBytes[0]},{addrBytes[1]},{addrBytes[2]},{addrBytes[3]},{port / 256},{port % 256}).");
    }

    private void HandlePort(string arg)
    {
        RequireLogin();

        CleanupDataConnection();

        var parts = arg.Split(',');
        if (parts.Length != 6)
        {
            SendResponse(501, "Syntax error in parameters.");
            return;
        }

        _activeAddress = IPAddress.Parse($"{parts[0]}.{parts[1]}.{parts[2]}.{parts[3]}");
        _activePort = int.Parse(parts[4]) * 256 + int.Parse(parts[5]);
        _dataMode = FtpDataMode.Active;

        SendResponse(200, "PORT command successful.");
    }

    #endregion

    #region 文件操作

    private void HandleList(string path)
    {
        RequireLogin();

        var targetPath = ResolvePath(string.IsNullOrEmpty(path) ? _currentDirectory : path);

        if (Directory.Exists(targetPath))
        {
            var listing = GenerateDirectoryListing(targetPath);
            SendDataResponse(listing);
        }
        else if (File.Exists(targetPath))
        {
            var listing = GenerateFileListingLine(targetPath);
            SendDataResponse(listing + "\r\n");
        }
        else
        {
            // 支持通配符
            var dirName = Path.GetDirectoryName(targetPath);
            var pattern = Path.GetFileName(targetPath);

            if (string.IsNullOrEmpty(dirName))
                dirName = GetPhysicalPath(_currentDirectory);

            if (string.IsNullOrEmpty(pattern))
                pattern = "*";

            if (Directory.Exists(dirName))
            {
                var files = Directory.GetFileSystemEntries(dirName, pattern);
                var listing = string.Join("\r\n", files.Select(f =>
                    Directory.Exists(f)
                        ? GenerateDirectoryListingLine(f)
                        : GenerateFileListingLine(f)));
                listing += "\r\n";
                SendDataResponse(listing);
            }
            else
            {
                SendResponse(550, "Directory not found.");
            }
        }
    }

    private void HandleNlst(string path)
    {
        RequireLogin();

        var targetPath = ResolvePath(string.IsNullOrEmpty(path) ? _currentDirectory : path);

        if (Directory.Exists(targetPath))
        {
            var entries = Directory.GetFileSystemEntries(targetPath)
                .Select(Path.GetFileName)
                .Where(name => name != null);
            var listing = string.Join("\r\n", entries) + "\r\n";
            SendDataResponse(listing);
        }
        else
        {
            SendResponse(550, "Directory not found.");
        }
    }

    private void HandleRetr(string path)
    {
        RequireLogin();

        var targetPath = ResolvePath(path);

        if (!File.Exists(targetPath))
        {
            SendResponse(550, "File not found.");
            return;
        }

        SendResponse(150, $"Opening {_transferType} mode data connection for {Path.GetFileName(targetPath)} ({new FileInfo(targetPath).Length} bytes).");

        try
        {
            using var dataStream = OpenDataConnection();
            if (dataStream == null)
            {
                SendResponse(425, "Can't open data connection.");
                return;
            }

            var fileBytes = File.ReadAllBytes(targetPath);
            dataStream.Write(fileBytes, 0, fileBytes.Length);
            dataStream.Flush();

            SendResponse(226, "Transfer complete.");
        }
        catch (Exception ex)
        {
            SendResponse(550, $"Transfer failed: {ex.Message}");
        }
        finally
        {
            CleanupDataConnection();
        }
    }

    private void HandleStor(string path)
    {
        RequireLogin();

        var targetPath = ResolvePath(path);

        SendResponse(150, $"Opening {_transferType} mode data connection for {Path.GetFileName(targetPath)}.");

        try
        {
            using var dataStream = OpenDataConnection();
            if (dataStream == null)
            {
                SendResponse(425, "Can't open data connection.");
                return;
            }

            using var fileStream = File.Create(targetPath);
            var buffer = new byte[8192];
            int bytesRead;
            while ((bytesRead = dataStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                fileStream.Write(buffer, 0, bytesRead);
            }
            fileStream.Flush();

            SendResponse(226, "Transfer complete.");
        }
        catch (Exception ex)
        {
            SendResponse(550, $"Transfer failed: {ex.Message}");
        }
        finally
        {
            CleanupDataConnection();
        }
    }

    private void HandleAppe(string path)
    {
        RequireLogin();

        var targetPath = ResolvePath(path);

        SendResponse(150, $"Opening {_transferType} mode data connection for {Path.GetFileName(targetPath)}.");

        try
        {
            using var dataStream = OpenDataConnection();
            if (dataStream == null)
            {
                SendResponse(425, "Can't open data connection.");
                return;
            }

            using var fileStream = new FileStream(targetPath, FileMode.Append, FileAccess.Write);
            var buffer = new byte[8192];
            int bytesRead;
            while ((bytesRead = dataStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                fileStream.Write(buffer, 0, bytesRead);
            }
            fileStream.Flush();

            SendResponse(226, "Transfer complete.");
        }
        catch (Exception ex)
        {
            SendResponse(550, $"Transfer failed: {ex.Message}");
        }
        finally
        {
            CleanupDataConnection();
        }
    }

    private void HandleDele(string path)
    {
        RequireLogin();

        var targetPath = ResolvePath(path);

        if (File.Exists(targetPath))
        {
            File.Delete(targetPath);
            SendResponse(250, "File deleted successfully.");
        }
        else
        {
            SendResponse(550, "File not found.");
        }
    }

    private void HandleRmd(string path)
    {
        RequireLogin();

        var targetPath = ResolvePath(path);

        if (Directory.Exists(targetPath))
        {
            Directory.Delete(targetPath, true);
            SendResponse(250, "Directory removed successfully.");
        }
        else
        {
            SendResponse(550, "Directory not found.");
        }
    }

    private void HandleMkd(string path)
    {
        RequireLogin();

        var targetPath = ResolvePath(path);
        var dir = Directory.CreateDirectory(targetPath);
        var virtPath = GetVirtualPath(targetPath);
        SendResponse(257, $"\"{virtPath}\" directory created.");
    }

    private void HandleRnfr(string path)
    {
        RequireLogin();

        var targetPath = ResolvePath(path);

        if (File.Exists(targetPath) || Directory.Exists(targetPath))
        {
            _renameFromPath = targetPath;
            SendResponse(350, "File exists, ready for destination name.");
        }
        else
        {
            SendResponse(550, "File not found.");
        }
    }

    private void HandleRnto(string path)
    {
        RequireLogin();

        if (_renameFromPath == null)
        {
            SendResponse(503, "Bad sequence of commands. Use RNFR first.");
            return;
        }

        var targetPath = ResolvePath(path);

        try
        {
            if (File.Exists(_renameFromPath))
                File.Move(_renameFromPath, targetPath);
            else if (Directory.Exists(_renameFromPath))
                Directory.Move(_renameFromPath, targetPath);

            _renameFromPath = null;
            SendResponse(250, "Rename successful.");
        }
        catch (Exception ex)
        {
            _renameFromPath = null;
            SendResponse(550, $"Rename failed: {ex.Message}");
        }
    }

    private void HandleSize(string path)
    {
        RequireLogin();

        var targetPath = ResolvePath(path);

        if (File.Exists(targetPath))
        {
            var fileInfo = new FileInfo(targetPath);
            SendResponse(213, fileInfo.Length.ToString());
        }
        else
        {
            SendResponse(550, "File not found.");
        }
    }

    #endregion

    #region 系统/信息命令

    private void HandleSyst()
    {
        RequireLogin();
        SendResponse(215, "UNIX Type: L8");
    }

    private void HandleFeat()
    {
        RequireLogin();

        var features = new StringBuilder();
        features.AppendLine("211-Features:");
        features.AppendLine(" SIZE");
        features.AppendLine(" MDTM");
        features.AppendLine(" REST STREAM");
        features.AppendLine(" UTF8");
        features.AppendLine("211 End");

        SendRaw(features.ToString());
    }

    private void HandleNoop()
    {
        SendResponse(200, "NOOP command successful.");
    }

    private void HandleOpts(string arg)
    {
        if (arg.Equals("UTF8 ON", StringComparison.OrdinalIgnoreCase))
            SendResponse(200, "Always in UTF8 mode.");
        else
            SendResponse(200, "OK.");
    }

    private void HandleStat(string path)
    {
        RequireLogin();

        if (string.IsNullOrEmpty(path))
        {
            SendResponse(211, "Server status: Connected, logged in.");
        }
        else
        {
            HandleList(path);
        }
    }

    private void HandleHelp()
    {
        SendResponse(214, "The following commands are recognized: USER PASS QUIT PWD CWD CDUP TYPE PASV PORT LIST NLST RETR STOR DELE RMD MKD RNFR RNTO SIZE SYST FEAT NOOP OPTS STAT HELP REST APPE");
    }

    private void HandleRest(string restartMarker)
    {
        RequireLogin();
        // REST 标记用于断点续传，简化实现：接受但不实际支持
        SendResponse(350, $"Restarting at {restartMarker}. Send STOR or RETR to initiate transfer.");
    }

    #endregion

    #region 辅助方法

    private void RequireLogin()
    {
        if (_state != FtpSessionState.LoggedIn)
            throw new InvalidOperationException("Not logged in.");
    }

    /// <summary>
    /// 发送 FTP 响应
    /// </summary>
    private void SendResponse(int code, string message)
    {
        var response = $"{code} {message}\r\n";
        _logger.LogInformation("FTP RES: {Response}", response.TrimEnd('\r', '\n'));
        SendRaw(response);
    }

    /// <summary>
    /// 发送原始数据
    /// </summary>
    private void SendRaw(string data)
    {
        SendAsync(Encoding.ASCII.GetBytes(data));
    }

    /// <summary>
    /// 将虚拟路径解析为物理路径
    /// </summary>
    private string ResolvePath(string virtualPath)
    {
        if (virtualPath == "/")
            return _rootDirectory;

        // 绝对路径：从根目录解析
        if (virtualPath.StartsWith('/'))
        {
            var relativePath = virtualPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.GetFullPath(Path.Combine(_rootDirectory, relativePath));

            if (!fullPath.StartsWith(_rootDirectory, StringComparison.OrdinalIgnoreCase))
                return _rootDirectory;

            return fullPath;
        }

        // 盘符绝对路径（如 C:\...）
        if (Path.IsPathRooted(virtualPath))
            return Path.GetFullPath(virtualPath);

        // 相对路径：从当前目录解析
        var currentPhysical = GetPhysicalPath(_currentDirectory);
        var combined = Path.Combine(currentPhysical, virtualPath);
        var fullPath2 = Path.GetFullPath(combined);

        // 安全检查：不允许访问根目录之外的文件
        if (!fullPath2.StartsWith(_rootDirectory, StringComparison.OrdinalIgnoreCase))
            return _rootDirectory;

        return fullPath2;
    }

    /// <summary>
    /// 将虚拟路径转为物理路径
    /// </summary>
    private string GetPhysicalPath(string virtualPath)
    {
        if (virtualPath == "/")
            return _rootDirectory;

        virtualPath = virtualPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        return Path.GetFullPath(Path.Combine(_rootDirectory, virtualPath));
    }

    /// <summary>
    /// 将物理路径转为虚拟路径
    /// </summary>
    private string GetVirtualPath(string physicalPath)
    {
        if (string.Equals(physicalPath, _rootDirectory, StringComparison.OrdinalIgnoreCase))
            return "/";

        var relative = Path.GetRelativePath(_rootDirectory, physicalPath);
        return "/" + relative.Replace(Path.DirectorySeparatorChar, '/');
    }

    /// <summary>
    /// 生成类似 Unix `ls -l` 的目录列表
    /// </summary>
    private string GenerateDirectoryListing(string directoryPath)
    {
        var sb = new StringBuilder();
        var dirInfo = new DirectoryInfo(directoryPath);

        // 添加 . 和 ..
        sb.AppendLine(GenerateDirectoryListingLineRaw("drwxr-xr-x", 2, "owner", "group", 4096,
            DateTime.Now.ToString("MMM dd HH:mm"), "."));
        sb.AppendLine(GenerateDirectoryListingLineRaw("drwxr-xr-x", 2, "owner", "group", 4096,
            DateTime.Now.ToString("MMM dd HH:mm"), ".."));

        foreach (var dir in dirInfo.GetDirectories())
        {
            sb.AppendLine(GenerateDirectoryListingLine(dir.FullName));
        }

        foreach (var file in dirInfo.GetFiles())
        {
            sb.AppendLine(GenerateFileListingLine(file.FullName));
        }

        return sb.ToString();
    }

    private string GenerateDirectoryListingLine(string directoryPath)
    {
        var di = new DirectoryInfo(directoryPath);
        return GenerateDirectoryListingLineRaw("drwxr-xr-x", 2, "owner", "group", 4096,
            di.LastWriteTime.ToString("MMM dd HH:mm"), di.Name);
    }

    private string GenerateFileListingLine(string filePath)
    {
        var fi = new FileInfo(filePath);
        var perms = "-rw-r--r--";
        return GenerateDirectoryListingLineRaw(perms, 1, "owner", "group", fi.Length,
            fi.LastWriteTime.ToString("MMM dd HH:mm"), fi.Name);
    }

    private static string GenerateDirectoryListingLineRaw(string perms, int links, string owner,
        string group, long size, string date, string name)
    {
        // 格式: perms  links owner group size date name
        return $"{perms} {links,3} {owner,-8} {group,-8} {size,8} {date} {name}";
    }

    /// <summary>
    /// 通过数据连接发送响应
    /// </summary>
    private void SendDataResponse(string data)
    {
        SendResponse(150, "Opening data connection.");

        try
        {
            using var dataStream = OpenDataConnection();
            if (dataStream == null)
            {
                SendResponse(425, "Can't open data connection.");
                return;
            }

            var bytes = Encoding.ASCII.GetBytes(data);
            dataStream.Write(bytes, 0, bytes.Length);
            dataStream.Flush();

            SendResponse(226, "Transfer complete.");
        }
        catch (Exception ex)
        {
            SendResponse(425, $"Data transfer error: {ex.Message}");
        }
        finally
        {
            CleanupDataConnection();
        }
    }

    /// <summary>
    /// 打开数据连接（主动或被动模式）
    /// </summary>
    private Stream? OpenDataConnection()
    {
        return _dataMode switch
        {
            FtpDataMode.Active => OpenActiveDataConnection(),
            FtpDataMode.Passive => OpenPassiveDataConnection(),
            _ => null,
        };
    }

    private Stream? OpenActiveDataConnection()
    {
        if (_activeAddress == null)
            return null;

        try
        {
            var client = new System.Net.Sockets.TcpClient();
            client.Connect(new IPEndPoint(_activeAddress, _activePort));
            return client.GetStream();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "FTP 主动模式连接失败: {Message}", ex.Message);
            return null;
        }
    }

    private Stream? OpenPassiveDataConnection()
    {
        if (_passiveListener == null)
            return null;

        try
        {
            // 设置较短的超时
            _passiveListener.Server.ReceiveTimeout = 30000;

            var client = _passiveListener.AcceptTcpClient();
            CleanupPassiveListener();
            return client.GetStream();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "FTP 被动模式连接失败: {Message}", ex.Message);
            return null;
        }
    }

    /// <summary>
    /// 清理数据连接资源
    /// </summary>
    private void CleanupDataConnection()
    {
        _dataMode = FtpDataMode.None;
        CleanupPassiveListener();
    }

    private void CleanupPassiveListener()
    {
        if (_passiveListener != null)
        {
            _passiveListener.Stop();
            _passiveListener = null;
        }
    }

    /// <summary>
    /// 获取本机非回环 IP 地址
    /// </summary>
    private static IPAddress GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                return ip;
        }
        return IPAddress.Loopback;
    }

    #endregion
}
