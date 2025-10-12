using HamsterStudio.Douyin.DataModels;
using HamsterStudio.Douyin.Services;
using HamsterStudio.Web.DataModels;
using Microsoft.AspNetCore.Mvc;

namespace HamsterStudio.WebApi.Controllers;

[ApiController]
[Route("/douyin")]
public class DouyinController(DouyinResourcesDownloadService downloadService) : ControllerBase
{
    [HttpPost("share-link/download")]
    public async Task<ActionResult<ServerRespModel?>> DownloadNote(RequestDataModel request)
    {
        return Ok(await downloadService.DownloadResourcesBy(request));
    }

}
