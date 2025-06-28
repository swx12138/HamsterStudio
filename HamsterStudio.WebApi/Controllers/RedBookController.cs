using HamsterStudio.Barefeet.Logging;
using HamsterStudio.RedBook.Interfaces;
using HamsterStudio.RedBook.Models;
using HamsterStudio.RedBook.Services;
using HamsterStudio.Web.DataModels;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace HamsterStudio.WebApi.Controllers;

[ApiController]
[Route("/xhs")]
public class RedBookController(IRedBookParser parser, NoteDownloadService downloadService) : ControllerBase
{
    [HttpGet("images/list")]
    public ActionResult<string[]> GetDownloadedImageList()
    {
        return Array.Empty<string>();
    }

    [HttpPost("share-link/info")]
    public ActionResult<NoteDataModel> PostShareLink([FromBody] PostBodyModel postBody)
    {
        Logger.Shared.Information($"RedBookController: PostShareLink: {postBody.Url}");

        var data = parser.GetNoteData(postBody.Url);
        if (data == null)
        {
            return StatusCode((int)HttpStatusCode.BadGateway, "Failed to parse the note data from the provided URL.");
        }

        return Ok(data);
    }

    [HttpPost("share-link/download")]
    public async Task<ActionResult<ServerRespModel?>> DownloadNote(NoteDataModel noteData)
    {
        return Ok(await downloadService.DownloadNoteAsync(noteData));
    }

}
