using HamsterStudio.Barefeet.FileSystem;
using HamsterStudio.Web.Services;
using HamsterStudio.Web.Strategies.Download;
using HamsterStudio.Web.Strategies.Request;
using HamsterStudio.Web.Strategies.StreamCopy;
using System.Diagnostics;

namespace HamsterStudioTests;

[TestClass]
public class TasteMultipleThreadDownload
{
    public async Task DownloadToFileAsync(string url, string filePath)
    {
        using (var httpClient = new HttpClient())
        {
            using (var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();

                using (var contentStream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await contentStream.CopyToAsync(fileStream);
                }
            }
        }
    }

    [TestMethod]
    public async Task TestMemoryLoad()
    {
        var dest = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), nameof(HamsterStudioTests));
        if (!Directory.Exists(dest))
        {
            Directory.CreateDirectory(dest);
        }


        dest = Path.Combine(dest, "large_file.bin");
        if (File.Exists(dest))
        {
            File.Delete(dest);
        }

       await DownloadToFileAsync("http://localhost:5002/download", dest);
    }


    [TestMethod]
    public async Task TestMultipleThreadDownload()
    {
        HttpClientProvider provider = new();
        var downloader = new CommonDownloader(provider);

        var dest = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), nameof(HamsterStudioTests));
        if (!Directory.Exists(dest))
        {
            Directory.CreateDirectory(dest);
        }

        dest = Path.Combine(dest, "large_file.bin");
        if (File.Exists(dest))
        {
            File.Delete(dest);
        }

        var watch = Stopwatch.StartNew();
        var rslt = await downloader.DownloadFileAsync(
            new Uri("http://localhost:5002/download"),
            dest,
            new AuthenticRequestStrategy(provider.HttpClient),
            new FileStreamHttpContentCopyStrategy(),
            new FixedChunkSizeDownloadStrategy(FileSizeDescriptor.FileSize_4M,
            Environment.ProcessorCount));
        Console.WriteLine($"[{rslt}] {dest} in {watch.ElapsedMilliseconds} ms");
    }

}
