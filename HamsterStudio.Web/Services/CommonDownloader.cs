using HamsterStudio.Barefeet.Logging;

namespace HamsterStudio.Web.Services;

public class CommonDownloader
{
    private readonly HttpClient client;

    public CommonDownloader()
    {
        client = new(new LoggingHandler(new HttpClientHandler()));
    }

    public async Task<bool> DownloadFile(string url, string destinationPath)
    {
        ArgumentException.ThrowIfNullOrEmpty(url, nameof(url));
        ArgumentException.ThrowIfNullOrEmpty(destinationPath, nameof(destinationPath));
        try
        {
            if (File.Exists(destinationPath))
            {
                Logger.Shared.Warning($"File already exists at {destinationPath}. Skipped.");
                return true;
            }

            using var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            await using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await response.Content.CopyToAsync(fileStream);

            Logger.Shared.Information($"{Path.GetFileName(destinationPath)} 成功下载到 {destinationPath}.");
            return true;
        }
        catch (Exception ex)
        {
            Logger.Shared.Warning($"Error downloading file: {ex.Message}");
            Logger.Shared.Debug(ex);
            return false;
        }
    }
}
