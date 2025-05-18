using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudio.Web.Interfaces;

public class DownloadResukt
{
    public string FullFilename { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

public interface IDownloader
{
    Task<DownloadResukt> DownloadAsync(string url, string? filename = null, string? path = null, IBinaryDataTypeChecker? typeChecker = null, HttpRequestMessage? httpRequest = null);

}
