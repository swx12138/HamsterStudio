using HamsterStudio.Toolkits.Request;
using System.IO;

namespace HamsterStudio.Toolkits.Magius
{
    internal class HoneyHunter
    {
        static void Main0(string[] args)
        {
            Console.WriteLine($"Hello, World!\t{2:d2}");

            FakeBrowser browser = new();

            Directory.CreateDirectory("quotes/lynette/");
            Directory.SetCurrentDirectory("quotes/lynette/");
            //var tasks = new List<string> { "357_01_cn", "357_02_cn", "357_03_cn", "300001_cn", "300002_cn", "300003_cn", "1000000_01_cn", "1000000_02_cn", "1000000_03_cn", "1000000_04_cn", "1000000_05_cn", "1000000_06_cn", "1000000_07_cn", "1000001_01_cn", "1000001_02_cn", "1000001_03_cn", "1000002_01_cn", "1000002_02_cn", "1000002_03_cn", "1000100_01_cn", "1000100_02_cn", "1000100_03_cn", "1000100_04_cn", "1000100_05_cn", "1000100_06_cn", "1000101_01_cn", "1000101_02_cn", "1000101_03_cn", "1000101_04_cn", "1000101_05_cn", "1000101_06_cn", "1000200_01_cn", "1000200_02_cn", "1000200_03_cn", "1000400_01_cn", "1000400_02_cn", "1000400_03_cn", "1010000_01_cn", "1010000_02_cn", "1010000_03_cn", "1010100_01_cn", "1010100_02_cn", "1010100_03_cn", "1010100_04_cn", "1010100_05_cn", "1010200_01_cn", "1010200_02_cn", "1010200_03_cn", "1010201_01_cn", "1010201_02_cn", "1010201_03_cn", "1010300_01_cn", "1010300_02_cn", "1010300_03_cn", "1010400_01_cn", "1010400_02_cn", "1010400_03_cn", "1020000_01_cn", "1020000_02_cn", "1020000_03_cn" }
            //.Select(async x =>
            //{
            //    string filename = $"{x}.ogg";
            //    string url = "https://genshin.honeyhunterworld.com/audio/quotes/lynette/" + filename;
            //    try
            //    {
            //        byte[] data = await browser.Get(url);
            //        File.WriteAllBytes(filename, data);
            //        return filename;
            //    }
            //    catch (Exception ex)
            //    {
            //        return filename + ex.Message;
            //    }
            //});

            var tasks = new List<string> { "1006", "1101", "1102", "1103", "1201", "1202", "1203", "1204", "1205", "1206", "1001", "1002", "1003", "1004", "2001", "2005", "2003", "2002", "3001", "3002", "3003", "4001", "4002", "4003", "4004", "4005", "4006", "4007", "4008", "4009", "4010", "4011", "5001", "5002", "5003", "5004", "5005", "6001", "6012", "6003", "6004", "6007", "6008", "6009", "7001", "8001", "8002", "8003", "8004", "100001", "100002", "100003", "200001", "200002", "200003", "300001", "300002", "300003", "410001", "410002", "410003", "400001", "400002", "500001", "500002", "500003", "900001", "910001", "600001", "600002", "600003" }
            .Select(async x =>
            {
                string filename = $"{x}_cn.ogg";
                string url = "https://genshin.honeyhunterworld.com/audio/quotes/lynette/" + filename;
                try
                {
                    var stream = await browser.GetStreamAsync(url);
                    byte[] data = new BinaryReader(stream).ReadBytes((int)stream.Length);
                    File.WriteAllBytes(filename, data);
                    Thread.Sleep(1000);
                    return filename;
                }
                catch (Exception ex)
                {
                    return filename + ex.Message;
                }
            });

            foreach (var r in Task.WhenAll(tasks).Result)
            {
                Console.WriteLine(r);
            }
            return;
        }

        private static void GetVoice(FakeBrowser browser, int voice_index, int voice_count)
        {
            var result = Task.WhenAll(Enumerable.Range(1, voice_count)
                .Select(async x =>
                {
                    string url = "https://" + $"genshin.honeyhunterworld.com/audio/quotes/lynette/{voice_index:d9}_{x:d2}_cn.ogg";
                    var stream = await browser.GetStreamAsync(url);
                    byte[] data = new BinaryReader(stream).ReadBytes((int)stream.Length);
                    string filename = $"quotes/lynette/{voice_index}_0{x}_cn.ogg";
                    File.WriteAllBytes(filename, data);
                    return filename;
                })).Result;

            foreach (var r in result)
            {
                Console.WriteLine(r);
            }
        }

    }
}
