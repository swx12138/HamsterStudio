using HamsterStudio.Bilibili.Services.Restful;
using HamsterStudio.RedBook.Services.XhsRestful;
using HamsterStudio.SinaWeibo.Services.Restful;
using HamsterStudio.Web.DataModels;

namespace HamsterStudioMaui.Services;

abstract class ProcessChain(ProcessChain? next)
{
    public virtual async Task<ServerRespModel?> Process(string link)
    {
        if (next != null)
        {
            return await next.Process(link);
        }
        return null;
    }
}

class XiaohongshuProcess(IRedBookClient xhsCli, ProcessChain? next) : ProcessChain(next)
{
    public override async Task<ServerRespModel?> Process(string link)
    {
        if (link.Contains("小红书"))
        {
            var urls = link.Split();
            var url = urls.FirstOrDefault(x => x.StartsWith("http") || x.StartsWith("xhslink"))?.Split("，").First();
            if (url == null || url == string.Empty)
            {
                //Log += "解析Url失败！";
                return null;
            }
            else
            {
                //Log += $"Loading url {url}";
            }

            if (!url.StartsWith("http"))
            {
                url = "http://" + url;
            }

            var noteData = await xhsCli.PostXhsShareLink(new() { Download = true, Url = url });
            return await xhsCli.DownloadXhsNote(noteData);
        }
        else
        {
            return await base.Process(link);
        }
    }
}

class BilibiliProcess(IBilibiliClient blient, ProcessChain? next) : ProcessChain(next)
{
    public override async Task<ServerRespModel?> Process(string link)
    {
        if (link.StartsWith("BV"))
        {
            var resp = await blient.PostDownloadInfo(new(link, ""));
            return resp;
        }
        else
        {
            return await base.Process(link);
        }
    }
}

class WeiboProcess(IWeiboClient client, ProcessChain? next) : ProcessChain(next)
{
    public override async Task<ServerRespModel?> Process(string link)
    {
        if (link.Contains("weibo.com"))
        {
            return await client.PostDownloadData(new(link));
        }
        return await base.Process(link);
    }
}

abstract class OfflineProcessChain(ProcessChain? next) : ProcessChain(next)
{
}