using HamsterStudio.RedBook.DataModels;
using Refit;

namespace HamsterStudio.WebApi.Services;

public interface IHamsterClient
{
    [Post("/xhs/share-link/info")]
    Task<NoteDataModel> PostXhsShareLink(PostBodyModel postBody);

    [Post("/xhs/share-link/download")]
    Task<ServerResp> DownloadXhsNote(NoteDataModel noteData);

}
