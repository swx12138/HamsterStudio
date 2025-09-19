using System.IO;

namespace HamsterStudio.Toolkits.Services.ImageMetaInfoReader;

public class JpegImageMetaInfoReader : IImageMetaInfoReader
{
    public bool Accept(in byte[] header)
    {
        ushort magic = BitConverter.ToUInt16(header.Take(2).Reverse().ToArray());
        return magic == 0xffd8;
    }

    public ImageMetaInfo Read(in FileStream ifs)
    {
        ifs.Seek(2, SeekOrigin.Begin);
        while (true)
        {
            // 读取两个字节
            var rawTag = new byte[2];
            if (0 == ifs.Read(rawTag, 0, rawTag.Length))
            {
                throw new KeyNotFoundException("没有找到文件头信息，可能不是一个正确的JPEG文件。");
            }
            //rawTag = rawTag.Reverse().ToArray();

            if (rawTag[0] != 0xff)
            {
                throw new FormatException("Not a JPEG format.");
            }
            var rawLen = new byte[2];
            ifs.Read(rawLen, 0, rawLen.Length);
            int length = (BitConverter.ToUInt16(rawLen.Reverse().ToArray()) - 2); // 减2是因为表示长度的两个字节也在内
            if (rawTag[1] != 0xc0 && rawTag[1] != 0xc2) // C0是标准SOF，C2不知道哪来的
            {
                ifs.Seek(length, SeekOrigin.Current);
                if (rawTag[1] == 0xdb) // DQT
                {
                    var next = new byte[3];
                    ifs.Read(next, 0, next.Length);
                    if (next[0] == 0x24 && next[1] == 0x03 &&
                        (length + 2 - next[2] == 0)) // 不知道为啥要这么做
                    {
                        ifs.Seek(length, SeekOrigin.Current);
                    }
                    else
                    {
                        ifs.Seek(-next.Length, SeekOrigin.Current);
                    }
                }
                continue;
            }

            var sofRaw = new byte[length];
            ifs.Read(sofRaw, 0, sofRaw.Length);

            byte sample = sofRaw[0];
            ushort height = BitConverter.ToUInt16(sofRaw.Skip(1).Take(2).Reverse().ToArray());
            ushort width = BitConverter.ToUInt16(sofRaw.Skip(3).Take(2).Reverse().ToArray());
            byte channel = sofRaw[5];

            //Console.WriteLine($"It's a jpeg image, {width}*{height} in size, with {channel} channels.");
            //break;
            return new() { Height = height, Width = width, Type = "Jpeg" };
        }
        throw new Exception("Unreachable code.");
    }
}
