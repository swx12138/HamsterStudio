using HamsterStudio.ArchiveMaster.Services;
using HamsterStudio.Barefeet.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudioTests;

[TestClass]
public class TestArchiveMaster
{
    [TestMethod]
    public void MyTestMethod()
    {
        PasswordTester tester = new("668899");
        Assert.IsNotNull(tester.TestArchive(@"D:\BaiduNetdiskDownload\sml\新建文本文档.7z"), "测试压缩包失败，可能是测试方法有问题。");

        tester.Passwords.AddRange(
            File.ReadAllLines(@"D:\passworddictionary最新库.txt")
            .Concat(File.ReadLines(@"D:\pass.dic"))
            .Select(x => x.Trim())
            .Where(x => !x.IsNullOrEmpty())
            .Reverse()
            .Distinct());

        string? password = tester.TestArchive(@"D:\BaiduNetdiskDownload\sml\No.109.7z");
        Assert.IsNotNull(password);
        Assert.IsFalse(password.IsNullOrEmpty(), "密码不能为空！");
        Console.WriteLine($"密码: {password}");
    }
}
