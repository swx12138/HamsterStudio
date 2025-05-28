using HamsterStudio.RedBook.Models;
using Refit;

namespace HamsterStudio.RedBook.Services.XhsRestful;

public interface IRedBookClient
{
    [Post("/xhs/share-link/info")]
    Task<NoteDataModel> PostXhsShareLink(PostBodyModel postBody);

    [Post("/xhs/share-link/download")]
    Task<ServerResp> DownloadXhsNote(NoteDataModel noteData);

    [Get("/static/{**fileRelaPath}")]
    Task<Stream> GetStaticFile(string fileRelaPath);
}
