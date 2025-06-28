using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Readers;

namespace HamsterStudio.ArchiveMaster.Services;

public class PasswordTesterDs(params string[] passwords)
{
    public List<string> Passwords { get; private set; } = [.. passwords];

    public async Task<string> TestPasswords(
            string zipPath,
            IList<string> passwords,
            bool useMultiThreading,
            byte[] zipData = null)
    {
        var cts = new CancellationTokenSource();
        var progress = new Progress<int>(percent =>
        {
            Console.CursorLeft = 0;
            Console.Write($"进度: {percent}% | 已测试: {percent * passwords.Count / 100}/{passwords.Count}");
        });

        string foundPassword = null;

        if (useMultiThreading)
        {
            // 使用并行处理
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount,
                CancellationToken = cts.Token
            };

            try
            {
                await Task.Run(() =>
                {
                    var locker = new object();
                    var counter = 0;

                    Parallel.ForEach(passwords, options, (password, state) =>
                    {
                        if (cts.IsCancellationRequested)
                            state.Stop();

                        if (TestPassword(zipPath, password, zipData))
                        {
                            lock (locker)
                            {
                                if (foundPassword == null)
                                {
                                    foundPassword = password;
                                    cts.Cancel();
                                    state.Stop();
                                }
                            }
                        }

                        // 更新进度
                        Interlocked.Increment(ref counter);
                        if (counter % 100 == 0)
                        {
                            ((IProgress<int>)progress).Report((counter * 100) / passwords.Count);
                        }
                    });

                    // 最终进度报告
                    ((IProgress<int>)progress).Report(100);
                });
            }
            catch (OperationCanceledException)
            {
                // 密码找到，取消操作
            }
        }
        else
        {
            // 顺序处理
            for (int i = 0; i < passwords.Count; i++)
            {
                if (cts.IsCancellationRequested) break;

                var password = passwords[i];
                Console.CursorLeft = 0;
                Console.Write($"测试密码: {password.PadRight(30)} | {i + 1}/{passwords.Count}");

                if (TestPassword(zipPath, password, zipData))
                {
                    foundPassword = password;
                    break;
                }

                // 更新进度
                if (i % 10 == 0)
                {
                    ((IProgress<int>)progress).Report((i * 100) / passwords.Count);
                }
            }

            ((IProgress<int>)progress).Report(100);
        }

        return foundPassword;
    }

    static bool TestPassword(string zipPath, string password, byte[] zipData)
    {
        try
        {
            using (var archive = zipData != null ?
                ArchiveFactory.Open(new MemoryStream(zipData), new ReaderOptions { Password = password }) :
                ArchiveFactory.Open(zipPath, new ReaderOptions { Password = password }))
            {
                // 尝试读取第一个条目来验证密码
                var entry = archive.Entries.FirstOrDefault(e => !e.IsDirectory);
                if (entry != null)
                {
                    // 尝试读取一小部分数据来验证密码
                    using (var stream = entry.OpenEntryStream())
                    {
                        // 仅读取前16字节验证（减少资源使用）
                        byte[] buffer = new byte[16];
                        int bytesRead = stream.Read(buffer, 0, buffer.Length);
                        return bytesRead > 0;
                    }
                }
            }
        }
        catch (CryptographicException)
        {
            // 密码错误 - 正常情况
            return false;
        }
        catch (InvalidFormatException)
        {
            // 文件格式错误
            return false;
        }
        catch (Exception ex)
        {
            // 其他错误
            Console.WriteLine($"\n测试密码 '{password}' 时发生错误: {ex.Message}");
            return false;
        }

        return false;
    }

    static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        int order = 0;
        double len = bytes;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }

}

public class PasswordTester(params string[] passwords)
{
    public List<string> Passwords { get; private set; } = [.. passwords];

    public string? TestArchive(string archivePath)
    {
        int i = 0;
        return Passwords.AsParallel()
            .FirstOrDefault(password =>
            {
#if DEBUG
                Console.WriteLine($"正在测试密码 {i++}/[{Passwords.Count}]: {password}");
#endif
                return TestImpl(archivePath, password);
            });

        foreach (var pwd in Passwords)
        {
#if DEBUG
            Console.WriteLine($"正在测试密码 {i++}/[{Passwords.Count}]: {pwd}");
#endif
            if (TestImpl(archivePath, pwd))
            {
                return pwd;
            }
        }
        return null;
    }

    public static bool TestImpl(string archivePath, string password)
    {
        try
        {
            using var archive = ArchiveFactory.Open(archivePath, new ReaderOptions()
            {
                Password = password,
                LookForHeader = true // 对于加密文件可能需要
            });
            // 尝试读取第一个条目来验证密码
            var entry = archive.Entries.FirstOrDefault();
            if (entry != null)
            {
                using var stream = entry.OpenEntryStream();
                // 尝试读取一小部分数据来验证密码
                byte[] buffer = new byte[1];
                stream.Read(buffer, 0, 1);
                return true;
            }
        }
        catch (CryptographicException)
        {
            // 密码错误
            return false;
        }
        catch (Exception)
        {
            // 其他错误
            return false;
        }

        return false;
    }

}
