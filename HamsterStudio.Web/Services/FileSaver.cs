using HamsterStudio.Web.Request;
using System.IO;

namespace HamsterStudio.Web.Services
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
        public static async Task<string> SaveFileFromUrl(string url, string dir, string? filename = null, FakeBrowser? fakeBrowser = null)
        {
            filename ??= url.Split("?")[0].Split("@")[0].Split('/').Where(x => x != "").Last();

            dir = Path.Combine(Directory.GetCurrentDirectory(), dir);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string path = Path.Combine(dir, filename);
            if (!Path.Exists(path))
            {
                fakeBrowser ??= FakeBrowser.CommonClient;
                var contentStream = fakeBrowser.GetStream(url);

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
        public static async Task<SaveFileResult> SaveFileToDisk(Task<Stream> data, string filename)
        {
            using var fs = File.OpenWrite(filename);
            await data.Result.CopyToAsync(fs);
            return SaveFileResult.Succeed;
        }

    }

}
