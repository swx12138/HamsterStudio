using HamsterStudio.Barefeet.Models.FileMgmt;

namespace BarefeetTest
{
    [TestClass]
    public sealed class DirectoryModelTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            Assert.AreEqual("dd", DirectoryModel.GetDirectoryName("D:/dd"));
            Assert.AreEqual("dd", DirectoryModel.GetDirectoryName("D:\\dd"));
            Assert.AreEqual("dd", DirectoryModel.GetDirectoryName("D:/dd/"));
            Assert.AreEqual("dd", DirectoryModel.GetDirectoryName("D:\\dd\\"));
            Assert.AreEqual("dd", DirectoryModel.GetDirectoryName("D://dd"));
        }
    }
}
