﻿using HamsterStudio.RedBook.Models;
using HamsterStudio.Web.DataModels;
using Refit;

namespace HamsterStudio.RedBook.Services.XhsRestful;

public interface IRedBookClient
{
    [Post("/xhs/share-link/info")]
    Task<NoteDataModel> PostXhsShareLink(PostBodyModel postBody);

    [Post("/xhs/share-link/download")]
    Task<ServerRespModel> DownloadXhsNote(NoteDataModel noteData);
}
