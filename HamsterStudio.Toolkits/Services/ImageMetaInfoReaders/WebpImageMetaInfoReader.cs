using System.IO;

namespace HamsterStudio.Toolkits.Services.ImageMetaInfoReader;

public class WebpImageMetaInfoReader : IImageMetaInfoReader
{
    public bool Accept(in byte[] header)
    {
        UInt32 riffMagic = BitConverter.ToUInt32(header.Take(4).Reverse().ToArray());
        if (riffMagic != 0x52494646)
        {
            return false;
        }

        UInt32 webpMagic = BitConverter.ToUInt32(header.Skip(8).Take(4).Reverse().ToArray());
        if (webpMagic != 0x57454250)
        {
            return false;
        }

        return true;
    }

    public static int LiittleEndian12bit(in byte[] data)
    {
        return (data[2] << 16 | data[1] << 8 | data[0]);
    }

    public ImageMetaInfo Read(in FileStream ifs)
    {
        while (true)
        {
            var tagRaw = new byte[8];
            if (0 == ifs.Read(tagRaw, 0, tagRaw.Length))
            {
                break;
            }

            var tag = BitConverter.ToUInt32(tagRaw.Take(4).Reverse().ToArray());
            var dataSize = BitConverter.ToUInt32(tagRaw.Skip(4).Take(4).ToArray());
            if (tag != 0x56503858) // VB8X
            {
                ifs.Seek(dataSize, SeekOrigin.Current);
                continue;
            }

            var data = new byte[dataSize];
            ifs.Read(data, 0, data.Length);

            var flags = data[0];
            var width = LiittleEndian12bit(data.Skip(4).Take(3).ToArray()) + 1;
            var height = LiittleEndian12bit(data.Skip(7).Take(3).ToArray()) + 1;
            return new() { Width = width, Height = height, Type = "Webp" };
        }
        throw new FormatException();
    }
}

