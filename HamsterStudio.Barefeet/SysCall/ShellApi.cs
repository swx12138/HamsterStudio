using HamsterStudio.Barefeet.Logging;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace HamsterStudio.Barefeet.SysCall;

public static class ShellApi
{
    [DllImport("Shlwapi.dll", CharSet = CharSet.Unicode)]
    public static extern int StrCmpLogicalW(string psz1, string psz2);

    public static void OpenFolder(string folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
        {
            throw new ArgumentException("文件夹路径不能为空。");
        }

        // 检查路径是否存在（如果指向的文件夹可能不存在，可以注释掉此检查）
        if (!Directory.Exists(folderPath))
        {
            throw new DirectoryNotFoundException($"找不到路径：{folderPath}");
        }

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = folderPath, // 可以在这里附加参数，如 "/e, \"{folderPath}\""
                UseShellExecute = true // 通常保持为true，以通过系统Shell启动
            };
            Process.Start(startInfo);
        }
        catch (Exception ex) // 捕获如Win32Exception等异常
        {
            // 根据你的应用程序类型处理异常，例如记录日志或抛出
            throw new InvalidOperationException($"无法打开资源管理器：{ex.Message}", ex);
        }
    }

    public static void SelectFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            throw new ArgumentNullException(nameof(filePath), "File path cannot be null or empty.");
        }

        // Ensure the file path is properly formatted
        filePath = Path.GetFullPath(filePath);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("The specified file does not exist.", filePath);
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = $"/select,\"{filePath}\"",
            UseShellExecute = true
        });
    }

    public static bool OpenBrowser(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"打开浏览器失败: {ex.Message}");
            return false;
        }
    }

    public static void System(string cmd)
    {
        // 创建一个进程对象
        Process process = new Process();

        // 设置要执行的命令和参数
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.FileName = "cmd.exe";
        startInfo.Arguments = $"/C {cmd}"; // 示例命令：列出当前目录下的文件和文件夹
                                           //startInfo.RedirectStandardOutput = true;
                                           //startInfo.RedirectStandardError = true;

        // 设置进程对象的启动信息
        process.StartInfo = startInfo;
        //process.OutputDataReceived += (s, e) => { };
        //process.ErrorDataReceived += (s, e) => { };

        // 启动进程并等待执行完成
        process.Start();
        process.WaitForExit();

        // 关闭进程
        process.Close();
    }

    public static void SendToRecycleBin(string path)
    {
        if (!File.Exists(path)) { return; }
        // 将文件移动到回收站
        Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(path,
                              Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs,
                              Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
    }

}
