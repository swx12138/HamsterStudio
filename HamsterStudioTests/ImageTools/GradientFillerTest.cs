using HamsterStudio.Toolkits.Services;
using HamsterStudioBridge.OpenCvWrapper;
using OpenCvSharp;
using System.Diagnostics;

namespace HamsterStudioTests.ImageTools;

[TestClass]
public class GradientFillerTest
{
    [TestMethod]
    public void TestFillBilinearFast()
    {
        Mat mat = new Mat(840, 840, MatType.CV_8UC3);
        Stopwatch stopwatch = new();
        long total_used = 0;
        for (int i = 0; i < 1000; i++)
        {
            stopwatch.Restart();
            ImageStitcher.FillBilinearFast(mat, Scalar.Red, Scalar.Green, Scalar.Blue, Scalar.WhiteSmoke);
            stopwatch.Stop();

            Trace.TraceInformation($"used {stopwatch.ElapsedMilliseconds} ms");
            total_used += stopwatch.ElapsedMilliseconds;
        }
        Trace.TraceInformation($"average used {total_used / 1000.0} ms");
        Assert.IsTrue(total_used < 10 * 1000);
    }

    [TestMethod]
    public void TestManagedFillBilinear()
    {
        Mat mat = new Mat(840, 840, MatType.CV_8UC3);
        Stopwatch stopwatch = new();
        double total_used = 0;
        for (int i = 0; i < 1000; i++)
        {
            stopwatch.Restart();
            ImageProcessor.FillBilinearWrapper(mat.CvPtr, ColorScalar.Red, ColorScalar.Green, ColorScalar.Blue, ColorScalar.WhiteSmoke);
            stopwatch.Stop();

            Trace.TraceInformation($"used {stopwatch.Elapsed.TotalMicroseconds} ms");
            total_used += stopwatch.Elapsed.TotalMicroseconds;
        }
        Trace.TraceInformation($"average used {total_used / 1000.0 / 1000} ms");

        Cv2.ImShow("view", mat);
        Cv2.WaitKey();

        Assert.IsTrue(total_used < 10 * 1000 * 1000);
    }

}
