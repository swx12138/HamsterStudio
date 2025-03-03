using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace HamsterStudio.Toolkits.Request
{
    public static class FakeBrowserDownloadExt
    {
        /// <summary>
        /// 将一个完整块按照partLen分为若干个小块
        /// </summary>
        /// <param name="max">完整块的大小</param>
        /// <param name="partLen">每个part的最大长度</param>
        /// <returns></returns>
        public static IEnumerable<PartInfo> GetParts(long max, int partLen)
        {
            for (int i = 0; i < max; i += partLen)
            {
                int length = (int)Math.Min(partLen, max - i); // 确保最后一个分块不超过总长度
                yield return new PartInfo { Id = i / partLen, Start = i, Length = length };
            }
        }

        /// <summary>
        /// 分块下载文件
        /// </summary>
        /// <param name="browser"></param>
        /// <param name="url"></param>
        /// <param name="partSize">默认2MB/块</param>
        /// <returns>文件总大小</returns>
        public static async Task<long> PartialDownload(this FakeBrowser browser, string url, string output, int partSize = 0x2000000, int? maxDegreeOfParallelism = null)
        {
            if (File.Exists(output)) { return -1; }

            long? size = await browser.GetFileSize(url);
            if (size == null) { return -1; }

            long totalSize = size.Value;
            if (await browser.DoServerSupportContentRange(url))
            {
                var parts = GetParts(totalSize, partSize);
                AutoResetEvent streamReady = new(false);
                Dictionary<PartInfo, Stream> downloaded = new();
                int nWrittenPart = 0;

                BackgroundWorker downloading = new();
                downloading.DoWork += (s, e) =>
                {
                    parts.AsParallel().WithDegreeOfParallelism(Math.Clamp(maxDegreeOfParallelism ?? Environment.ProcessorCount, 1, Environment.ProcessorCount / 2))
                    .ForAll(part =>
                    {
                        Trace.TraceInformation($"part {part.Id} start.");
                        Stream stream = browser.GetStreamAsync(url, part.Start, part.Length).Result;
                        lock (downloaded)
                        {
                            downloaded.Add(part, stream);
                            streamReady.Set();
                        }
                    });
                    while (downloaded.Count > 0) { streamReady.Set(); }
                    Trace.TraceInformation($"downloading done.");
                };
                downloading.RunWorkerCompleted += (s, e) =>
                {
                    if (e.Error != null) { Trace.TraceError(e.Error.Message); }
                    Trace.TraceInformation($"downloading down.");
                };
                downloading.RunWorkerAsync();

                using FileStream fileStream = nWrittenPart == 0 ? File.OpenWrite(output) : File.Open(output, FileMode.Append);
                while (nWrittenPart < parts.Count())
                {
                    streamReady.WaitOne();
                    lock (downloaded)
                    {
                        if (downloaded.Any(x => x.Key.Id == nWrittenPart))
                        {
                            var first = downloaded.First(x => x.Key.Id == nWrittenPart);
                            first.Value.CopyTo(fileStream);

                            nWrittenPart++;
                            downloaded.Remove(first.Key);

                            Trace.TraceInformation($"{output} part {nWrittenPart}");
                        }
                    }
                }
            }
            else
            {
                using Stream stream = await browser.GetStreamAsync(url);
                using FileStream fileStream = File.OpenWrite(output);
                await stream.CopyToAsync(fileStream);
            }

            return totalSize;
        }

        public delegate void ProgressReport(long progress);

        public static async void Downlaod(this FakeBrowser browser, string url, long fileSize, string filename, ProgressReport? progressReport)
        {
            Stream stream = await browser.GetStreamAsync(url);
            FileStream fileStream = File.Create(filename);
            Downlaod0(stream, fileStream, progressReport);
        }

        /// <summary>
        /// 还是先下载完才能分块写入
        /// </summary>
        /// <param name="webDataStream"></param>
        /// <param name="fileStream"></param>
        /// <param name="progressReport"></param>
        /// <param name="bufferSize"></param>
        public static async void Downlaod0(Stream webDataStream, FileStream fileStream, ProgressReport? progressReport, int bufferSize = 0x8000)
        {
            byte[] buffer = new byte[bufferSize];
            long totalBytesRead = 0;

            while (true)
            {
                int bytesRead = await webDataStream.ReadAsync(buffer, 0, bufferSize);

                if (bytesRead == 0) // 没有更多数据可读，表示读取完成
                    break;

                totalBytesRead += bytesRead;
                progressReport?.Invoke(totalBytesRead); // 报告进度

                // 如果需要按块收集数据（虽然题目要求直接写入FileStream，这里为了符合返回IEnumerable<byte[]>的需求做示例）
                byte[] chunk = new byte[bytesRead];
                Array.Copy(buffer, 0, chunk, 0, bytesRead);

                // 将读取的数据写入文件流
                await fileStream.WriteAsync(buffer, 0, bytesRead);

                Trace.TraceInformation($"pid:{Process.GetCurrentProcess().Id} tid:{Thread.CurrentThread.ManagedThreadId} read {bytesRead} bytes.");
            }
        }
    }

    public struct PartInfo
    {
        public int Id;
        public int Start;
        public int Length;
    }

}
