using HamsterStudio.BeautyLegs.RedBook;
using Refit;

namespace HamsterStudio.Web.Services;

public interface IRedBookClient
{
    [Post("/xhs/share-link/info")]
    Task<NoteDataModel> PostXhsShareLink(PostBodyModel postBody);

    [Post("/xhs/share-link/download")]
    Task<ServerResp> DownloadXhsNote(NoteDataModel noteData);

    [Get("/static/{**fileRelaPath}")]
    Task<Stream> GetStaticFile(string fileRelaPath);
}
