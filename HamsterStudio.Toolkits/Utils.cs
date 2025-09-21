using System.IO;
using System.IO.Compression;
using System.Windows.Media;

namespace HamsterStudio.Toolkits
{
    internal static class Utils
    {
        public static byte[] DecompressGZip(byte[] gzip)
        {
            // 创建一个内存流来保存输入的gzip数据
            using MemoryStream inputStream = new(gzip);
            // 创建一个内存流来保存解压后的输出数据
            using MemoryStream outputStream = new();
            // 使用GZipStream进行解压操作
            using (GZipStream decompressionStream = new(inputStream, CompressionMode.Decompress))
            {
                // 将解压后的内容复制到outputStream
                decompressionStream.CopyTo(outputStream);
            }

            // 获取解压后的字节数组
            byte[] decompressedData = outputStream.ToArray();

            return decompressedData;
        }

        public static ImageSource ExtractAndCreateImageSource(Stream stream, string imageFileNameInZip)
        {
            throw new NotImplementedException();
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
            {
                foreach (var entry in archive.Entries)
                {
                    if (entry.FullName == imageFileNameInZip)
                    {
                        using var entryStream = entry.Open();
                        using var memoryStream = new MemoryStream();
                        entryStream.CopyTo(memoryStream);
                        memoryStream.Position = 0;

                        //return ImageUtils.CreateImageSource(memoryStream);
                    }
                }
            }

            throw new FileNotFoundException("The specified image file was not found in the ZIP archive.");
        }
    }
}
