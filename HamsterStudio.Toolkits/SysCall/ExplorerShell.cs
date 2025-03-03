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


    }

}
