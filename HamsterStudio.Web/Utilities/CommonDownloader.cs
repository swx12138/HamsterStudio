using HamsterStudio.Barefeet.Extensions;
using HamsterStudio.Web.Interfaces;
using NetCoreServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudio.Web.Utilities;

public class CommonDownloader(FakeBrowser? browser = null) : IDownloader
{
    public async Task<DownloadResukt> DownloadAsync(string url, string? path = null, string? filename = null, IBinaryDataTypeChecker? typeChecker = null, HttpRequestMessage? httpRequest = null)
    {
        filename ??= url.Split("?")[0].Split("@")[0].Split('/').Where(x => x != "").Last();
        filename = FileNameUtil.SanitizeFileName(filename);
        if (filename == string.Empty)
        {
            return new DownloadResukt
            {
                Success = false,
                ErrorMessage = "filename is empty"
            };
        }

        path ??= Directory.GetCurrentDirectory();
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        string fullPath = Path.Combine(path, filename);
        if (File.Exists(fullPath))
        {
            return new DownloadResukt
            {
                Success = true,
                FullFilename = fullPath
            };
        }

        var contentStream = await (browser ?? FakeBrowser.CommonClient).GetStreamAsync(url, httpRequest);
        if (contentStream == null)
        {
            return new DownloadResukt
            {
                Success = false,
                ErrorMessage = "Content stream is null"
            };
        }

        //using (StreamReader streamReader = new(contentStream))
        //{
        //    streamReader.BaseStream.Seek(0, SeekOrigin.Begin);

        //    byte[] buffer = new byte[0x10];
        //    streamReader.BaseStream.Read(buffer, 0, buffer.Length);
        //    string ext = typeChecker?.CheckBinaryType(buffer) ?? string.Empty;

        //    fullPath = fullPath + "." + ext;
        //    streamReader.BaseStream.Seek(0, SeekOrigin.Begin);
        //}

        using FileStream fileStream = new(fullPath, FileMode.Create, FileAccess.Write);
        await contentStream.CopyToAsync(fileStream);

        return new DownloadResukt
        {
            Success = true,
            FullFilename = fullPath
        };
    }
}
