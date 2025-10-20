using HamsterStudio.Web.DataModels;
using HamsterStudio.Web.Services;
using HamsterStudio.Web.Strategies.Request;
using HamsterStudio.Web.Strategies.StreamCopy;
using System.Diagnostics;

namespace HamsterStudioTests;

[TestClass]
public class TasteMultipleThreadDownload
{
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
        if(File.Exists(dest))
        {
            File.Delete(dest);
        }

        var watch = Stopwatch.StartNew();
        var rslt = await downloader.EasyDownloadFileAsync(new Uri("http://localhost:5002/download"), dest, 2 * 1024 * 1024, true);
        Console.WriteLine($"[{rslt}] {dest} in {watch.ElapsedMilliseconds} ms");
    }

}
