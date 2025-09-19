using System.Diagnostics;
using HamsterStudio.Toolkits;
using System.Drawing;

namespace HamsterStudioTests
{
    [TestClass]
    public class TasteImageMetaReader
    {

        [TestMethod]
        public void MyTestMethod()
        {
            string[] targets = [
                @"C:\Users\collei\Downloads\ffcc0191d049481fa39325249780999a67c29f65.jpg@672w_378h_1c_.webp",
                @"C:\Users\collei\AppData\Local\Packages\Microsoft.Windows.ContentDeliveryManager_cw5n1h2txyewy\LocalState\Assets\d4c7465612f8a897d9cb14070de8311168d33184f8a5f455ff92ad89d773edfb.png",
                @"C:\Users\collei\AppData\Local\Packages\Microsoft.Windows.ContentDeliveryManager_cw5n1h2txyewy\LocalState\Assets\d4c7465612f8a897d9cb14070de8311168d33184f8a5f455ff92ad89d773edfb.jpg",
                @"C:\Users\collei\AppData\Local\Packages\Microsoft.Windows.ContentDeliveryManager_cw5n1h2txyewy\LocalState\Assets\d4c7465612f8a897d9cb14070de8311168d33184f8a5f455ff92ad89d773edfb.png",
                @"C:\Users\collei\AppData\Local\Packages\Microsoft.Windows.ContentDeliveryManager_cw5n1h2txyewy\LocalState\Assets\d4c7465612f8a897d9cb14070de8311168d33184f8a5f455ff92ad89d773edfb.jpg",
                @"C:\Users\collei\AppData\Local\Packages\Microsoft.Windows.ContentDeliveryManager_cw5n1h2txyewy\LocalState\Assets\d4c7465612f8a897d9cb14070de8311168d33184f8a5f455ff92ad89d773edfb.png",
                @"C:\Users\collei\AppData\Local\Packages\Microsoft.Windows.ContentDeliveryManager_cw5n1h2txyewy\LocalState\Assets\d4c7465612f8a897d9cb14070de8311168d33184f8a5f455ff92ad89d773edfb.jpg",
                @"C:\Users\collei\AppData\Local\Packages\Microsoft.Windows.ContentDeliveryManager_cw5n1h2txyewy\LocalState\Assets\d4c7465612f8a897d9cb14070de8311168d33184f8a5f455ff92ad89d773edfb.png",
                @"C:\Users\collei\AppData\Local\Packages\Microsoft.Windows.ContentDeliveryManager_cw5n1h2txyewy\LocalState\Assets\d4c7465612f8a897d9cb14070de8311168d33184f8a5f455ff92ad89d773edfb.jpg",
                @"C:\Users\collei\AppData\Local\Packages\Microsoft.Windows.ContentDeliveryManager_cw5n1h2txyewy\LocalState\Assets\16e4b1301c8a80c7c98380a959d5f40862490c8bd3389d8b100bd33b95be6975.jpg",
                @"C:\Users\collei\AppData\Local\Packages\Microsoft.Windows.ContentDeliveryManager_cw5n1h2txyewy\LocalState\Assets\203b534d936e6c3d62ccc28baf66e5d753f9f552f599186d906f845127a131f3.jpg",
                @"C:\Users\collei\AppData\Local\Packages\Microsoft.Windows.ContentDeliveryManager_cw5n1h2txyewy\LocalState\Assets\33504718d23407b7d1116f51cb56c7646897104d300201099927385f798232db.jpg",
                @"C:\Users\collei\AppData\Local\Packages\Microsoft.Windows.ContentDeliveryManager_cw5n1h2txyewy\LocalState\Assets\3ab5f312aa18f699c0afc6c25f036f89ad4f9387b4568ff94d6180f20e55cbff.jpg",
                @"C:\Users\collei\AppData\Local\Packages\Microsoft.Windows.ContentDeliveryManager_cw5n1h2txyewy\LocalState\Assets\637eaa9c7c793c4ce65b98c9b6929aec5abd5ff1ea100e97abe3fdb0146ed459.jpg",
                @"C:\Users\collei\AppData\Local\Packages\Microsoft.Windows.ContentDeliveryManager_cw5n1h2txyewy\LocalState\Assets\6e86fd2f0f84b12e81f1031bb444b2e2611cf73e4c8d3d91a17d71ba69cdf1ce.jpg",
                @"C:\Users\collei\AppData\Local\Packages\Microsoft.Windows.ContentDeliveryManager_cw5n1h2txyewy\LocalState\Assets\78c8788ed001dfc9746f6e8ecda927b1979c0b20dc3b84d6dbf8e45f9c94abcf.jpg",
                @"C:\Users\collei\AppData\Local\Packages\Microsoft.Windows.ContentDeliveryManager_cw5n1h2txyewy\LocalState\Assets\9e4e91af0aecc4f8b4f9b846dcc99d769d67e7873f032ccdcaa15e38c44c050a.jpg",
                @"C:\Users\collei\AppData\Local\Packages\Microsoft.Windows.ContentDeliveryManager_cw5n1h2txyewy\LocalState\Assets\9ec0805f58a1a97380b671b5710cca99a756b2b00477857c4c33e84f345278c3.jpg",
                @"C:\Users\collei\AppData\Local\Packages\Microsoft.Windows.ContentDeliveryManager_cw5n1h2txyewy\LocalState\Assets\ab49abb9e221ad305c23c1bd1ae78e555270cd7659c6b08693c28a6a653d0048.jpg",
                @"C:\Users\collei\AppData\Local\Packages\Microsoft.Windows.ContentDeliveryManager_cw5n1h2txyewy\LocalState\Assets\b0f752d54d948348f250525e0c6ccbb57504038eb204c7155f01ccb4573e3f1f.jpg",
            ];

            var svc = new ImageMetaInfoReadService();
            svc.ImageMetaInfoReaders.Add(new WebpImageMetaInfoReader());
            svc.ImageMetaInfoReaders.Add(new PngImageMetaInfoReader());
            svc.ImageMetaInfoReaders.Add(new JpegImageMetaInfoReader());
            var stopwatch = new Stopwatch();
            //for (int i = 0; i < targets.Length; i++)
            foreach (string target in targets)
            {
                stopwatch.Restart();
                var ans = svc.Read(target);
                Console.WriteLine($"ImageUtils.ReadMeta => {ans.Width} {ans.Height}, used {stopwatch.ElapsedMilliseconds} ms.");
            }

            // low 1888ms
            //stopwatch.Restart();
            //for (int i = 0; i < targets.Length; i++)
            //    foreach (string target in targets)
            //{
            //    var img = Image.FromFile(target);
            //    //Console.WriteLine((img.Width, img.Height));
            //}
            //var score1 = stopwatch.ElapsedMilliseconds;
            //Console.WriteLine($"Image.FromFile used {score1} ms.");

            // lowest 2152ms
            //stopwatch.Restart();
            //for (int i = 0; i < targets.Length; i++)
            //    foreach (string target in targets)
            //{
            //    using FileStream ifs = File.OpenRead(target);
            //    var img = Image.FromStream(ifs);
            //    //Console.WriteLine((img.Width, img.Height));
            //}
            //var score2 = stopwatch.ElapsedMilliseconds;
            //Console.WriteLine($"Image.FromStream used {score2} ms.");

            //Assert.IsTrue(score < score1 && score < score2);
        }

    }
}
