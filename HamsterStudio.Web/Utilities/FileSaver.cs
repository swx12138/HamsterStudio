﻿using HamsterStudio.Barefeet.Extensions;
using HamsterStudio.Barefeet.Logging;
using System.IO;

namespace HamsterStudio.Web.Utilities
{
    public static class FileSaver
    {
        /// <summary>
        /// 从url下载文件到本地
        /// </summary>
        /// <param name="url"></param>
        /// <param name="dir"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static async Task<string> SaveFileFromUrl(string url, string dir, string? filename = null, FakeBrowser? fakeBrowser = null, HttpRequestMessage? httpRequest = null)
        {
            filename ??= url.Split("?")[0].Split("@")[0].Split('/').Where(x => x != "").Last();
            filename = FileNameUtil.SanitizeFileName(filename);

            dir = Path.Combine(Directory.GetCurrentDirectory(), dir);
            if (!Directory.Exists(dir))
            {
                Logger.Shared.Debug($"{dir} not exists,create it.");
                Directory.CreateDirectory(dir);
            }

            string path = Path.Combine(dir, filename);
            if (!Path.Exists(path))
            {
                fakeBrowser ??= FakeBrowser.CommonClient;
                var contentStream = await fakeBrowser.GetStreamAsync(url, httpRequest);

                SaveFileResult result = await SaveFileToDisk(contentStream, path);
                if (result != SaveFileResult.Succeed)
                {
                    throw new Exception($"{result}");
                }
            }

            return path;
        }

        public enum SaveFileResult
        {
            Succeed,
            Existed,
            IrregularFilename,
        }

        /// <summary>
        /// 将二进制数据写入磁盘
        /// </summary>
        /// <param name="data"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static async Task<SaveFileResult> SaveFileToDisk(Stream data, string filename)
        {
            if (File.Exists(filename))
            {
                return SaveFileResult.Existed;
            }

            using var fs = File.OpenWrite(filename);
            await data.CopyToAsync(fs);
            return SaveFileResult.Succeed;
        }

    }

}
