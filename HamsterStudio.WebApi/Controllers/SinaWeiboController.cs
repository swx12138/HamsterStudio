using HamsterStudio.SinaWeibo.Models;
using HamsterStudio.SinaWeibo.Services;
using HamsterStudio.Web.DataModels;
using Microsoft.AspNetCore.Mvc;

namespace HamsterStudio.WebApi.Controllers;

[ApiController]
[Route("/weibo")]
public class SinaWeiboController(DownloadService downloadService) : ControllerBase
{
    [HttpPost("download")]
    public async Task<ActionResult<ServerRespModel?>> Download([FromBody] DownloadPostModel post)
    {
        var showId = post.Url.Split('?').First().Split('/').Last();
        var resp = await downloadService.Download(showId);
        return Ok(resp);
    }

}
