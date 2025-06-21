using HamsterStudio.Barefeet.Logging;
using HamsterStudio.SinaWeibo.Models;
using HamsterStudio.SinaWeibo.Services.Restful;
using Refit;
using System.Text.Json;

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


}
