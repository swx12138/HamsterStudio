﻿using HamsterStudio.Barefeet.Extensions;
using HamsterStudio.Bilibili.Models;
using HamsterStudio.Bilibili.Services;
using Microsoft.AspNetCore.Mvc;

namespace HamsterStudio.WebApi.Controllers;

[ApiController]
[Route("/bilib")]
public class BilibiliController(BangumiDownloadService downloadService) : ControllerBase
{
    [HttpPost("download-video")]
    public async Task<ActionResult<string>> Post([FromBody] DownloadVideoRequest request)
    {
        if (!request.Cookie.IsNullOrEmpty())
        {
            downloadService.Cookies = "bmg_af_switch=1; bmg_src_def_domain=i0.hdslb.com; VIP_CONTENT_REMIND=1; " + request.Cookie;
        }
        var resp = await downloadService.DownloadVideoByBvid(request.Bvid);
        return Ok(resp);
    }

}
