using System.IO;

namespace HamsterStudio.Toolkits.Services.ImageMetaInfoReader;

public class PngImageMetaInfoReader : IImageMetaInfoReader
{
    public bool Accept(in byte[] header)
    {
        var fullMagic = BitConverter.ToUInt64(header.Take(8).Reverse().ToArray());
        return fullMagic == 0x89504e470d0a1a0a;
    }

    public ImageMetaInfo Read(in FileStream ifs)
    {
        ifs.Seek(8, SeekOrigin.Begin);

        var blkSizeRaw = new byte[4];
        ifs.Read(blkSizeRaw, 0, blkSizeRaw.Length);


        var blkSize = ImageUtils.FromBigEndian(blkSizeRaw, 0);
        var blkData = new byte[blkSize];
        ifs.Read(blkData, 0, blkData.Length);

        var blkTypeCode = ImageUtils.FromBigEndian(blkData, 0);
        if (blkTypeCode == 0x49484452) // IHDR
        {
            var width = ImageUtils.FromBigEndian(blkData, 4);
            var height = ImageUtils.FromBigEndian(blkData, 8);
            var depth = blkData[12];
            return new() { Height = height, Width = width, Type = "Png" };
        }
        else
        {
            // 第一个Block一定是IHDR
            throw new FormatException("First block of png is not IHDR!!");
        }
    }
}

