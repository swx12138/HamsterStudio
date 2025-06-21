using HamsterStudio.Barefeet.Logging;
using HamsterStudio.SinaWeibo.Models;
using HamsterStudio.SinaWeibo.Services;
using HamsterStudio.SinaWeibo.Services.Restful;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Refit;
using System.Text.Json;
using System.Threading.Tasks;

namespace HamsterStudioTests;

[TestClass]
public class TestSinaWeibo
{

    private IWeiboApi weiboApi = RestService.For<IWeiboApi>(new HttpClient(new LoggingHandler(new HttpClientHandler(), msg =>
    {
        // Log request details here if needed
    }))
    {
        BaseAddress = new Uri("https://weibo.com")
    });

    IWeiboMediaApi mediaApi = RestService.For<IWeiboMediaApi>(new HttpClient()
    {
        BaseAddress = new Uri("https://wx3.sinaimg.cn")
    });

    [TestMethod]
    public async Task TestDeserializeDataModel()
    {
        using var infile = File.OpenRead(@"D:\Code\HamsterStudio\Samples\weibo_show_Px74vs2RJ.json");
        var model = await JsonSerializer.DeserializeAsync<ShowDataModel>(infile);
        Assert.IsNotNull(model);
    }

    [TestMethod]
    public async Task TestGetShowInfo()
    {
        var model = await weiboApi.GetShowInfo("Px74vs2RJ");

        Assert.IsNotNull(model);

    }

    [TestMethod]
    public async Task TestMeddiaApi()
    {
        const string mediaName = "9011e417ly1i2mu4spcxgj21jk2i5e6t.jpg";
        const string filename = @"D:\Code\HamsterStudio\Samples\Imgs\" + mediaName;
        using var stream = await mediaApi.GetFile(mediaName);
        using var outfile = File.Create(filename);
        await stream.CopyToAsync(outfile);
        Assert.AreNotEqual(0, new FileInfo(filename).Length);
    }

    [TestMethod]
    public async Task TestDownload()
    {
        DownloadService service = new(weiboApi, mediaApi);
        //var resp = await service.Download("PxxRmeBta");

        string[] list = [
            "https://weibo.com/2417091607/P96nx9HI1",
        ];

        foreach (var item in list)
        {
            var resp = await service.Download(item.Split('/').Last());
            Assert.IsNotNull(resp);
            Console.WriteLine($"{item} Done.");
        }

        //Assert.IsTrue(resp.Data.StaticFiles.Length > 0);
    }


}
