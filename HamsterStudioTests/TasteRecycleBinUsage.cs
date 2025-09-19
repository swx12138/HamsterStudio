using Microsoft.VisualBasic.FileIO;

namespace HamsterStudioTests
{
    [TestClass]
     public class TasteRecycleBinUsage
    {

        [TestMethod]
        public void MyTestMethod()
        {


            // 将文件移动到回收站
            FileSystem.DeleteFile(@"E:\Pictures\bizhi\uid_1646123180938093_uid_8204745_cat.png",
                                  UIOption.OnlyErrorDialogs,
                                  RecycleOption.SendToRecycleBin);
        }
    }
}
