using HamsterStudioBridge.OpenCvWrapper;
using OpenCvSharp;
using static HamsterStudio.Toolkits.Services.ImageStitcher;
using Size2d = HamsterStudioBridge.OpenCvWrapper.Size2d;

namespace HamsterStudioTests.ImageTools
{
    [TestClass]
    public class ImageStitchTest
    {
        static readonly Size Size_Empty = new Size(0, 0);

        [TestMethod]
        public void TestImageStitch()
        {
            IReadOnlyCollection<string> imagePaths = [
                @"C:\Users\collei\Downloads\生成图片.png",
                @"C:\Users\collei\Downloads\生成图片.png",
                @"C:\Users\collei\Downloads\生成图片_.png",
                @"C:\Users\collei\Downloads\生成图片.png",
                @"C:\Users\collei\Downloads\生成图片.png",
                @"C:\Users\collei\Downloads\生成图片.png",
                @"C:\Users\collei\Downloads\生成图片_.png",
                @"C:\Users\collei\Downloads\生成图片.png",
                @"C:\Users\collei\Downloads\生成图片.png",
            ];

            StitchSelectionMode stitchSelectionMode = StitchSelectionMode.Both;
            Size2d cellSize = new(720, 720);

            var images = imagePaths.Select(imagePath => Cv2.ImRead(imagePath)).Where(image =>
            {
                return stitchSelectionMode switch
                {
                    StitchSelectionMode.Portrait => image.Width < image.Height,
                    StitchSelectionMode.Landscape => image.Height <= image.Width,
                    _ => true
                };
            });
            var preprocessed = images.Select(image =>
            {
                double xScale = 1.0 * cellSize.Width / image.Width;
                double yScale = 1.0 * cellSize.Height / image.Height;
                double scale = Math.Min(xScale, yScale);
                Mat result = new();
                Cv2.Resize(image, result, Size_Empty, scale, scale);
                return result;
            }).ToArray();

            if (preprocessed.Length <= 0)
            {
                return;
            }

            Mat mat = new Mat();
            ImageProcessor.GetStitchImages(preprocessed.Select(m => m.CvPtr).ToArray(), mat.CvPtr, cellSize, -1);

            Cv2.ImShow("mat", mat);
            Cv2.WaitKey();

            Cv2.ImWrite(@"C:\Users\collei\Downloads\生成图片__.jpg", mat);
        }
    }
}
