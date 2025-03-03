
using System.IO;

namespace HamsterStudio.Toolkits.Magius
{
    static class BilibiliCacheRecover
    {
        static void Main0(string[] args)
        {
            Console.WriteLine("Hello, World!");

            string root = "D:\\Movies\\.video\\tv.danmaku.bili\\download";
            string[] files = [];/* Arsenal.Base.Utilities.GetFilesRecursive(root)
                .Where(filename => filename.EndsWith("video.m4s"));*/

            Console.WriteLine(files.Count());
            foreach (var video in files)
            {
                var audio = Path.Combine(Path.GetDirectoryName(video)!, "audio.m4s");
                if (File.Exists(audio))
                {
                    var oo = video.Split("\\")[6];
                    string outp = Path.Combine(root, $"{oo}.mp4");
                    if (!File.Exists(outp))
                    {
                        string cmd = $"ffmpeg -i \"{video}\" -i \"{audio}\" -c:v copy -c:a copy \"{outp}\"";
                        //Arsenal.Base.Utilities.System(cmd);
                        Console.WriteLine($"{outp} Succeed.");
                    }
                    else
                    {
                        Console.WriteLine($"{outp} Existed.");
                    }
                }
                else
                {
                    Console.WriteLine($"Lonely {video}.");
                }
            }

        }
    }
}
