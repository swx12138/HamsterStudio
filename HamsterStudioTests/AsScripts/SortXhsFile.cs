using HamsterStudio.Barefeet.Services;
using HamsterStudio.Barefeet.SysCall;
using HamsterStudio.RedBook;
using HamsterStudio.RedBook.Services;
using HamsterStudio.Web.Services;
using HamsterStudioGUI;
using HamsterStudioGUI.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace HamsterStudioTests.AsScripts
{
    [TestClass]
    public class SortXhsFile
    {
        const string XhsFilePath = @"E:\HamsterStudioHome\xhs_p";
        const string XhsInvalidFilePath = @"E:\HamsterStudioHome\xhs_p\xhs_ng";
        const string XhsValidFilePath = @"E:\HamsterStudioHome\xhs_p\xhs_nb";
        const string XhsFailedFilePath = @"E:\HamsterStudioHome\xhs_p\xhs_failed";
        const string XhsFilenameFingerPrint = "1040g"; // 1040g3k031ld65q7vlo4g5o6n4d4089hnps8vi00

        private ServiceCollection Services = new();
        private ServiceProvider ServiceProvider;

        public SortXhsFile()
        {
            Services.AddLogging();
            Services.ConfigureService(App.Home);
            Services.AddSingleton<CommonDownloader>();
            Services.AddSingleton<HttpClientProvider>();
            Services.AddRedBookWebApiServices();

            ServiceProvider = Services.BuildServiceProvider();
        }

        public ICollection<string> ReadKeys()
        {
            var files = new DirectoryInfo(XhsFilePath)
                .EnumerateFiles()
                .ToArray();
            Console.WriteLine($"Found {files.Length} files in {XhsFilePath}");

            var keys = files.Where(f => f.Name.Contains(XhsFilenameFingerPrint))
                .Select(fileInfo => fileInfo.FullName)
                .ToList();
            Console.WriteLine($"Found {keys.Count} keys");

            return keys;
        }

        public ICollection<string> BuildExistsKeys()
        {
            return Directory.EnumerateFiles(@"E:\HamsterStudioHome\xiaohongshu", "*", new EnumerationOptions() { RecurseSubdirectories = true })
                .Where(filename => filename.Contains(XhsFilenameFingerPrint))
                .ToArray();
        }

        [TestMethod]
        public void CheckKeys()
        {
            var keys = ReadKeys();

            //var lengths = keys.Select(k => k.Length).ToList();
            //Console.WriteLine($"Key length stats : min={lengths.Min()}, max={lengths.Max()}, average={lengths.Average()}");

            var allKeys = BuildExistsKeys();
            using var fileStream = File.OpenWrite(@"E:\HamsterStudioHome\xiaohongshu_.txt");
            using var streamWriter = new StreamWriter(fileStream);

            var svc = ServiceProvider.GetRequiredService<NoteDownloadService>();
            keys.AsParallel().WithDegreeOfParallelism(8).ForAll(fullname =>
            {
                var key = XhsFilenameFingerPrint + fullname.Split(XhsFilenameFingerPrint).Last().Split('.').First();
                var efile = allKeys.FirstOrDefault(k => k.Contains(key));
                if (efile != null)
                {
                    var nb = Path.Combine(XhsValidFilePath, Path.GetFileName(efile));
                    if (!File.Exists(nb))
                    {
                        File.Copy(efile, nb);
                    }
                    try
                    {
                        ShellApi.SendToRecycleBin(fullname);
                    }
                    catch { }
                }
                //else // exist的情况有bug
                //{
                //    var resp = svc.DownloadWithBaseTokens([key]);
                //    if (resp.Result.Data.StaticFiles.Length <= 0)
                //    {
                //        Trace.TraceWarning("Download token failed");
                //        return;
                //    }

                //    var rfile = resp.Result.Data.StaticFiles.First();
                //    File.Copy(rfile, Path.Combine(XhsValidFilePath, Path.GetFileName(XhsValidFilePath)));
                //    try
                //    {
                //        ShellApi.SendToRecycleBin(fullname);
                //    }
                //    catch { }
                //}
            });

        }
    }
}
