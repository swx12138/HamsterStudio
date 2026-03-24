using System.Diagnostics;

namespace HamsterStudioTests.AsScripts;

[TestClass]
public class FindFile
{
    [TestMethod]
    public void MyTestMethod()
    {
        var finder = new FileFinder();
        var func = new Func<string, bool>(x => Path.GetFileName(x).Contains("_XHS_"));
        finder.Search(@"E:/HamsterStudioHome", func);
        finder.Search(@"E:/Pictures", func);

        Assert.IsTrue(finder.Directories.Count != 0);
        foreach (var dir in finder.Directories)
        {
            Trace.WriteLine(dir);
        }

    }

    class FileFinder
    {
        public List<string> Directories { get; } = [];

        public void Search(string baseDir, Func<string, bool> func)
        {
            var dirs = Directory.EnumerateFiles(baseDir, "*", SearchOption.AllDirectories)
                 .Where(func)
                 .Select(x => Path.GetDirectoryName(x)!)
                 .Distinct();
            Directories.AddRange(dirs);
        }

    }

}
