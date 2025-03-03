using System.Diagnostics;
using System.IO;

namespace HamsterStudio.Toolkits.SysCall
{
    public static class ExplorerShell
    {
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

        public static long MilisecondTimestamp => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    }

}
