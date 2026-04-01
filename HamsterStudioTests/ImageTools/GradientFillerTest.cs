using HamsterStudio.Toolkits.Services;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudioTests.ImageTools;

[TestClass]
public class GradientFillerTest
{
    [TestMethod]
    public void TestMethod()
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

}
