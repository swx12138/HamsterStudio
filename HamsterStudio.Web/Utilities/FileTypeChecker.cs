using HamsterStudio.Web.Interfaces;

namespace HamsterStudio.Web.Utilities;

public class FileTypeChecker : IBinaryDataTypeChecker
{
    private static readonly List<FileSignature> _signatures =
    [
        // 按魔数长度降序排列以确保优先检测更长的签名
        new FileSignature([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A], "PNG"),
            new FileSignature([0x47, 0x49, 0x46, 0x38, 0x37, 0x61], "GIF"),
            new FileSignature([0x47, 0x49, 0x46, 0x38, 0x39, 0x61], "GIF"),
            new FileSignature([0x25, 0x50, 0x44, 0x46], "PDF"),       // PDF
            new FileSignature([0x50, 0x4B, 0x03, 0x04], "ZIP"),      // ZIP
            new FileSignature([0x52, 0x61, 0x72, 0x21], "RAR"),     // RAR
            new FileSignature([0xFF, 0xD8, 0xFF], "JPEG"),           // JPEG (部分变种)
            new FileSignature([0xFF, 0xD8], "JPEG"),                 // JPEG标准
            new FileSignature([0x42, 0x4D], "BMP"),                  // BMP
            new FileSignature([0x49, 0x44, 0x33], "MP3"),            // MP3 with ID3
            new FileSignature([0x4D, 0x5A], "EXE")                   // Windows可执行文件
    ];

    public string CheckBinaryType(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0)
            return "Unknown";

        foreach (var signature in _signatures)
        {
            if (bytes.Length >= signature.Signature.Length)
            {
                bool isMatch = true;
                for (int i = 0; i < signature.Signature.Length; i++)
                {
                    if (bytes[i] != signature.Signature[i])
                    {
                        isMatch = false;
                        break;
                    }
                }
                if (isMatch)
                {
                    return signature.FileType;
                }
            }
        }
        return "bin";
    }

    private class FileSignature
    {
        public byte[] Signature { get; }
        public string FileType { get; }

        public FileSignature(byte[] signature, string fileType)
        {
            Signature = signature;
            FileType = fileType;
        }
    }
}