using HamsterStudio.Bilibili;
using HamsterStudio.RedBook.Services.Parsing;
using HamsterStudio.Web;
using HamsterStudio.Web.DataModels;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace HamsterStudioMaui.Services;

public partial class OfflineProcessService
{
    private readonly ILogger<OfflineProcessService> _logger;
    private readonly string _cacheDir;
    private readonly FakeBrowser _browser;

    public OfflineProcessService(ILogger<OfflineProcessService> logger)
    {
        _logger = logger;
        _cacheDir = Path.Combine(FileSystem.CacheDirectory, "offline_downloads");
        Directory.CreateDirectory(_cacheDir);
        _browser = FakeBrowser.CommonClient;
    }

    /// <summary>
    /// 离线处理分享链接，返回包含本地文件路径的 ServerRespModel
    /// </summary>
    public async Task<ServerRespModel?> ProcessAsync(string shareInfo)
    {
        if (IsRedBookLink(shareInfo))
        {
            return await ProcessRedBookAsync(shareInfo);
        }
        else if (IsBilibiliLink(shareInfo))
        {
            return await ProcessBilibiliAsync(shareInfo);
        }

        return new ServerRespModel { Message = "没有匹配的处理模块（仅支持小红书、B站）", Status = -1 };
    }

    #region 平台检测

    private static bool IsRedBookLink(string text) =>
        text.Contains("小红书") || text.Contains("xhslink");

    private static bool IsBilibiliLink(string text) =>
        text.Contains("BV") || text.Contains("b23.tv") || text.Contains("bilibili");

    #endregion

    #region 小红书离线处理

    private async Task<ServerRespModel?> ProcessRedBookAsync(string shareInfo)
    {
        var url = ExtractRedBookUrl(shareInfo);
        if (string.IsNullOrEmpty(url))
        {
            return new ServerRespModel { Message = "解析小红书链接失败", Status = -1 };
        }

        // 复用 RedBookNoteParser 解析笔记
        var loggerFactory = LoggerFactory.Create(b => b.AddDebug());
        var parserLogger = loggerFactory.CreateLogger<RedBookNoteParser>();
        var parser = new RedBookNoteParser(parserLogger, _browser);
        var noteData = parser.GetNoteData(url);

        if (noteData == null || !noteData.NoteDetailMap.TryGetValue(noteData.CurrentNoteId, out var currentNote))
        {
            return new ServerRespModel { Message = "解析小红书笔记失败", Status = -1 };
        }

        var noteDetail = currentNote.NoteDetail;

        // 下载图片到本地缓存
        var downloadedFiles = new List<string>();
        for (int i = 0; i < noteDetail.ImageList.Count; i++)
        {
            var image = noteDetail.ImageList[i];
            var token = ExtractRedBookToken(image.Url);
            var filePath = await DownloadRedBookImage(token, noteDetail.Title, noteDetail.UserInfo.Nickname, i + 1);
            if (filePath != null)
            {
                downloadedFiles.Add(filePath);
            }
        }

        return new ServerRespModel
        {
            Message = "ok",
            Status = 0,
            Data = new ServerRespData
            {
                Title = noteDetail.Title,
                Description = noteDetail.Description,
                AuthorNickName = noteDetail.UserInfo.Nickname,
                StaticFiles = downloadedFiles
            }
        };
    }

    private static string? ExtractRedBookUrl(string shareInfo)
    {
        var urls = shareInfo.Split(' ', '\n', '\r');
        var url = urls.FirstOrDefault(x =>
            x.StartsWith("http://") || x.StartsWith("https://") || x.StartsWith("xhslink"))
            ?.Split('，', ',')
            .First()
            ?.Trim();

        if (string.IsNullOrEmpty(url))
            return null;

        if (!url.StartsWith("http"))
            url = "http://" + url;

        return url;
    }

    private async Task<string?> DownloadRedBookImage(string token, string title, string nickname, int index)
    {
        // 按格式优先级尝试下载
        var formats = new[] { "png", "webp", "jpeg", "jpg" };

        foreach (var format in formats)
        {
            var imageUrl = $"https://ci.xiaohongshu.com/{token}?imageView2/format/{format}";
            var filename = SanitizeFileName($"{title}_{index}_{nickname}_{token}.{format}");
            var filePath = Path.Combine(_cacheDir, filename);

            if (await TryDownloadFileAsync(imageUrl, filePath))
            {
                return filePath;
            }
        }

        _logger.LogWarning("小红书图片下载失败: token={Token}", token);
        return null;
    }

    /// <summary>
    /// 从小红书图片 URL 中提取 token（复刻 NoteDetailHelper.ExtractToken 逻辑）
    /// </summary>
    private static string ExtractRedBookToken(string url)
    {
        var token = url.Split("!").First();
        // 取第5个 '/' 之后的内容
        int count = 0;
        int idx = -1;
        for (int i = 0; i < token.Length; i++)
        {
            if (token[i] == '/')
            {
                count++;
                if (count == 5)
                {
                    idx = i;
                    break;
                }
            }
        }
        return idx >= 0 ? token[(idx + 1)..] : token;
    }

    #endregion

    #region Bilibili 离线处理

    private async Task<ServerRespModel?> ProcessBilibiliAsync(string shareInfo)
    {
        var bvid = ExtractBvid(shareInfo);
        if (string.IsNullOrEmpty(bvid))
        {
            return new ServerRespModel { Message = "无法从链接中提取 BV 号", Status = -1 };
        }

        // 复用 Bilibili.WebApiExtensions.CreateServ() 创建 API 客户端
        var apiService = WebApiExtensions.CreateServ();

        // 获取视频信息
        var videoInfoResp = await apiService.GetVideoInfoAsync(bvid);
        if (videoInfoResp.Code != 0 || videoInfoResp.Data == null)
        {
            return new ServerRespModel
            {
                Message = videoInfoResp.Message,
                Status = -(int)videoInfoResp.Code
            };
        }

        var videoInfo = videoInfoResp.Data;
        var downloadedFiles = new List<string>();

        // 下载封面
        if (!string.IsNullOrEmpty(videoInfo.Pic))
        {
            var coverPath = await DownloadFileAsync(
                videoInfo.Pic,
                SanitizeFileName($"{videoInfo.Title}_cover.jpg"));
            if (coverPath != null)
                downloadedFiles.Add(coverPath);
        }

        // 下载视频（取第一个分P，使用 durl 直链方式，无需 ffmpeg）
        if (videoInfo.Pages.Count > 0)
        {
            try
            {
                var page = videoInfo.Pages[0];
                var streamResp = await apiService.GetVideoStreamInfoAsync(
                    page.Cid, bvid, "", fnval: 1, qn: 80);

                if (streamResp.Code == 0 && streamResp.Data?.Durl is { Length: > 0 })
                {
                    var durl = streamResp.Data.Durl[0];
                    var videoUrl = durl.Url;

                    // 尝试备用链接
                    if (string.IsNullOrEmpty(videoUrl) && durl.BackupUrl is { Length: > 0 })
                    {
                        videoUrl = durl.BackupUrl[0];
                    }

                    if (!string.IsNullOrEmpty(videoUrl))
                    {
                        // B站视频直链需要 Referer
                        _browser.Referer = "https://www.bilibili.com/";
                        var videoPath = await DownloadFileAsync(
                            videoUrl,
                            SanitizeFileName($"{videoInfo.Title}.mp4"));
                        _browser.Referer = null;

                        if (videoPath != null)
                            downloadedFiles.Add(videoPath);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "B站视频流下载失败");
            }
        }

        if (downloadedFiles.Count == 0)
        {
            return new ServerRespModel
            {
                Message = "视频下载失败，可能需要登录Cookie",
                Status = -1
            };
        }

        return new ServerRespModel
        {
            Message = "ok",
            Status = 0,
            Data = new ServerRespData
            {
                Title = videoInfo.Title,
                Description = videoInfo.Desc,
                AuthorNickName = videoInfo.Owner.Name,
                StaticFiles = downloadedFiles
            }
        };
    }

    private static string? ExtractBvid(string text)
    {
        // 匹配 BV 号模式
        var match = BvRegex().Match(text);
        return match.Success ? match.Value : null;
    }

    [GeneratedRegex(@"BV[a-zA-Z0-9]{10}")]
    private static partial Regex BvRegex();

    #endregion

    #region 通用下载工具

    /// <summary>
    /// 尝试下载文件，成功返回 true
    /// </summary>
    private async Task<bool> TryDownloadFileAsync(string url, string filePath)
    {
        try
        {
            using var stream = await _browser.GetStreamAsync(url);
            using var fileStream = File.Create(filePath);
            await stream.CopyToAsync(fileStream);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 下载文件到缓存目录，返回本地路径，失败返回 null
    /// </summary>
    private async Task<string?> DownloadFileAsync(string url, string filename)
    {
        var filePath = Path.Combine(_cacheDir, SanitizeFileName(filename));
        if (await TryDownloadFileAsync(url, filePath))
        {
            return filePath;
        }
        _logger.LogWarning("文件下载失败: {Url}", url);
        return null;
    }

    #endregion

    #region 工具方法

    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        foreach (var c in invalid)
            name = name.Replace(c, '_');
        // 限制文件名长度
        if (name.Length > 200)
            name = name[..200];
        return name;
    }

    #endregion
}
