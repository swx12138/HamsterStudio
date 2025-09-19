using HamsterStudio.ImageTool.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudioTests;

[TestClass]
public class TestImageTools
{
    [TestMethod]
    public void TestArwDecode()
    {
        var srv = new DecodeImage();
        var metas =  srv.LoadSonyRaw(@"C:\Users\collei\Documents\DSC03848\DSC03848.ARW");
        Console.WriteLine(metas);
    }

}
