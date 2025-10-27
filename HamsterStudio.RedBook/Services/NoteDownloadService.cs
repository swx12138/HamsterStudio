using HamsterStudio.Barefeet.FileSystem;
using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Barefeet.SysCall;
using HamsterStudio.RedBook.Models;
using HamsterStudio.RedBook.Models.Sub;
using HamsterStudio.Web.DataModels;
using HamsterStudio.Web.Services;

namespace HamsterStudio.RedBook.Services;

public class NoteDownloadService(FileMgmt fileMgmt, CommonDownloader downloader)
{
    private readonly Logger _logger = Logger.Shared;

    public event Action<NoteDetailModel> OnNoteDetailUpdated = delegate { };

    public event Action<string> OnImageTokenDetected = delegate { };

    public async Task<ServerRespModel> DownloadNoteAsync(NoteDataModel noteData)
    {
        var currentNote = noteData.NoteDetailMap[noteData.CurrentNoteId];
        var noteDetail = currentNote.NoteDetail;

        _logger.Information($"开始处理<{NoteDetailHelper.GetTypeName(noteDetail)}>作品：{noteData.CurrentNoteId}");
        return await DownloadNoteLowAsync(noteDetail,
            new NoteDataOptionsModel { WithComments = false, AuthorCommentsOnly = true },
            []);
    }

    public bool HotProtocol(NoteDetailModel noteDetail, CommentModel[] comments)
    {
        bool isHot = fileMgmt.AlbumCollections.AddAlbum(new AlbumCollectionModel
        {
            OwnerName = noteDetail.UserInfo.Nickname,
            FileCount = noteDetail.ImageList.Sum(x => x.LivePhoto ? 2 : 1) + (noteDetail.Type == "video" ? 1 : 0) + comments.Length,
            Albums = [noteDetail.Title]
        });

        if (isHot)
        {
            var indepent = fileMgmt.CreateSubFolder(noteDetail.UserInfo.Nickname);
            foreach (var file in indepent.Parent.GetFiles($"*_xhs_{noteDetail.UserInfo.Nickname}_*"))
            {
                string newName = Path.Combine(indepent.FullName, file.Name);
                File.Move(file.FullName, newName, true);
            }
        }

        return isHot;
    }

    public async Task<ServerRespModel> DownloadNoteLowAsync(NoteDetailModel noteDetail, NoteDataOptionsModel options, CommentModel[] comments)
    {
        OnNoteDetailUpdated?.Invoke(noteDetail);

        bool isHot = HotProtocol(noteDetail, comments);

        _logger.Information($"标题：{noteDetail.Title}【{noteDetail.ImageList.Count}】");

        var processor = new NoteDetailProcessor(noteDetail, fileMgmt, downloader, _logger, isHot);
        string title = NoteDetailHelper.SelectTitle(noteDetail);
        await Parallel.ForEachAsync(
            noteDetail.ImageList,
            new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount / 2 },
            async (x, ct) => await processor.ProcessImage(x, title, OnImageTokenDetected));

        if (noteDetail.Type == "video")
        {
            await processor.ProcessVideoDownload();
        }
        foreach (var comment in comments)
        {
            await processor.ProcessComment(comment);
        }

        _logger.Information("Done.");

        return BuildResponse(noteDetail, processor.ContainedFiles);
    }


    private ServerRespModel BuildResponse(NoteDetailModel noteDetail, List<string> files)
    {
        var comparer = new NaturalStringComparer();
        return new ServerRespModel
        {
            Message = "ok",
            Data = new ServerRespData
            {
                Title = noteDetail.Title,
                Description = noteDetail.Description,
                AuthorNickName = noteDetail.UserInfo.Nickname,
                StaticFiles = [.. files.Select(f => $"xiaohongshu/{f}").OrderBy(f => f, comparer)]
            }
        };
    }


    private class NaturalStringComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            return ShellApi.StrCmpLogicalW(x, y);
        }
    }

}
