using Microsoft.Win32;

namespace HamsterStudio.FileManager.Services
{
    /// <summary>
    /// 注册表存储服务，用于持久化应用设置
    /// </summary>
    public class RegistrySettingsService
    {
        private const string SubKey = @"Software\HamsterStudio\FileManager";

        public string? LastDirectory
        {
            get
            {
                using var key = Registry.CurrentUser.OpenSubKey(SubKey);
                return key?.GetValue("LastDirectory") as string;
            }
            set
            {
                using var key = Registry.CurrentUser.CreateSubKey(SubKey);
                key.SetValue("LastDirectory", value ?? string.Empty);
            }
        }
    }
}
