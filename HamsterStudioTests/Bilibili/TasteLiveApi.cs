using HamsterStudio.Barefeet.Constants;
using HamsterStudio.Bilibili.Services.Restful;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudioTests.Bilibili;

[TestClass]
public class TasteLiveApi
{
    IBilibiliLiveApiSevice api = RestService.For<IBilibiliLiveApiSevice>(new HttpClient() { BaseAddress = new Uri("https://api.live.bilibili.com/") });

    [TestMethod]
    public async Task TestGetRoomInfo()
    {
        var resp = await api.GetRoomInnfoOld(2889968);
        Assert.IsNotNull(resp);
        Assert.IsTrue(resp.Code == 0);
        Assert.IsNotNull(resp.Data);
        Console.WriteLine(resp.Data.RoomId);
    }

    [TestMethod]
    public async Task TestGetPlayUrl()
    {
        var resp = await api.GetPlayUrl(21168868);
        Assert.IsNotNull(resp);
        Assert.IsTrue(resp.Code == 0);
        Assert.IsNotNull(resp.Data);
        for (var i = 0; i < resp.Data.DashUrls.Length; i++) {
            Console.WriteLine($"{i}. {resp.Data.DashUrls[i].Url}");        
        }
    }

    public async Task DownloadToFileAsync(string url, string filePath, string cookies)
    {
        using (var httpClient = new HttpClient())
        {
            httpClient.DefaultRequestHeaders.Add("User-Agent", BrowserConsts.EdgeUserAgent);

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Cookie", cookies);
            request.Headers.Add("Refer", "https://live.bilibili.com/21168868");

            using (var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
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
    public async Task TestDownloadLiveStream ()
    {
        string cookies = "buvid3=4122EB70-2D9E-192A-88BD-F75734138BB195860infoc; b_nut=1761035195; b_lsid=ECAE859D_19A05E090B2; _uuid=681810BE6-4E69-93AB-10DB3-F45C3198410F1094553infoc; enable_web_push=DISABLE; home_feed_column=5; buvid4=6C1387A8-29ED-4A0B-54CB-B1A9B5E34F9096467-025102116-mTRwgu8PeavYtoMKCxIyMQ%3D%3D; bili_ticket=eyJhbGciOiJIUzI1NiIsImtpZCI6InMwMyIsInR5cCI6IkpXVCJ9.eyJleHAiOjE3NjEyOTQzOTYsImlhdCI6MTc2MTAzNTEzNiwicGx0IjotMX0.SXUeNiVuCxldIq5Eev7nZO8rSRpvP3ufLeby9IyMLws; bili_ticket_expires=1761294336; buvid_fp=d2b6e70a29c932727149b5f9c4a7fb1b; SESSDATA=8dba3d0d%2C1776587228%2C6b29b%2Aa2CjD_DIPvOiebgxzeAa2q3dz1SXzQ1xFppNn5gYld3H2db-8QFWWJzSmP5nVBeQr1G74SVlRNaXJoUFNwT19HWW10ckV2UG1nX3ZNY1RtQWI3enBEWnc0SV9sTnk3U3hicXZvOG00NVEtdk1DQzNYQW9HenhfVDMtWXR3NnBTX1ZvOVRMWFROTGt3IIEC; bili_jct=21a33ce44eea30aae0ac3b595835bf8f; DedeUserID=286884672; DedeUserID__ckMd5=b2f1150da1c3b569; sid=g6dshyka; browser_resolution=1920-957";
        string liveUrl = "https://cn-zjjh-ct-04-31.bilivideo.com/live-bvc/711525/live_2889968_11636955.flv?expires=1761039031&pt=web&deadline=1761039031&len=0&oi=3730802782&platform=web&qn=80&trid=1000b4849fbb37eb4593b2aed063b1e8232f&uipk=100&uipv=100&nbs=1&uparams=cdn,deadline,len,oi,platform,qn,trid,uipk,uipv,nbs&cdn=cn-gotcha01&upsig=50716fab7b894ad85476df15ebe3cb1d&sk=e9d127f4e0cf165b6c26bddc583efce0&p2p_type=0&sl=1&free_type=0&mid=0&sid=cn-zjjh-ct-04-31&chash=1&bmt=1&sche=ban&score=13&source=one&trace=891&site=7d8305d247d217c740a4241b654e8ed0&zoneid_l=151355393&sid_l=live_2889968_11636955&order=1";
        await DownloadToFileAsync(liveUrl, @"C:\Users\collei\Downloads\test.flv", cookies);
    }

}
