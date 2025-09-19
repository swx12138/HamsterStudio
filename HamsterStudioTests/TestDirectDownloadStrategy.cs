using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Web.DataModels;
using HamsterStudio.Web.Strategies.Download;
using HamsterStudio.Web.Strategies.Request;
using HamsterStudio.Web.Strategies.StreamCopy;

namespace HamsterStudioTests
{

    [TestClass]
    public class TestDirectDownloadStrategy
    {
        [TestMethod]
        public async Task TestDownload()
        {
            var pgs = new HamsterProgress();
            pgs.ProgressChanged += Pg_ProgressChanged;
            var phccs = new ProgressHttpContentCopyStrategy(pgs);

            var httpClient = new HttpClient(new LoggingHandler(new HttpClientHandler()));
            var rs = new AuthenticRequestStrategy(httpClient);
            var dr = new DownloadRequest(new Uri("https://i0.hdslb.com/bfs/archive/7d0e9c8fb1a3e217149cbf7d61f4dd37d31b897c.jpg"), rs, phccs);

            var dds = new DirectDownloadStrategy();
            var rslt = await dds.DownloadAsync(dr);
        }

        private void Pg_ProgressChanged(object? sender, double e)
        {
            Console.WriteLine($"Progress is {e}%.");
        }
    }
}
