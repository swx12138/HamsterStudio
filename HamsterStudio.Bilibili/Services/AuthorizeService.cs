using Microsoft.Extensions.Logging;

namespace HamsterStudio.Bilibili.Services
{
    public class AuthorizeService
    {
        public string Cookie { get; private set; } = string.Empty;

        private FileMgmt FileMgmt { get; set; }
        private readonly ILogger _logger;

        public AuthorizeService(FileMgmt fileMgmt, ILogger<AuthorizeService> logger)
        {
            FileMgmt = fileMgmt;
            _logger = logger;
            Cookie = LoadCookies();
        }

        public void ReloadCookies()
        {
            Cookie = LoadCookies();
        }

        private string LoadCookies()
        {
            try
            {
                string cookiesFilename = Path.Combine(FileMgmt.StorageHome, "cookies.txt");
                var cookies = File.ReadAllText(cookiesFilename);
                _logger.LogInformation("Reloaded Bilibili cookies.");
                return cookies;
            }
            catch (Exception ex)
            {
                if (ex is DirectoryNotFoundException or FileNotFoundException)
                {
                    _logger.LogWarning("Load cookies failed.");
                    return string.Empty;
                }
                throw;
            }
        }

    }
}
