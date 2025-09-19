using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HamsterStudio.Barefeet.Services;

public class BinaryDataSerializer
{
    public static byte[] Serialize<T>(T data)
    {
        string jstr = JsonSerializer.Serialize(data);
        var bstr = Encoding.UTF8.GetBytes(jstr);

        using MemoryStream mem = new MemoryStream();
        using (GZipStream zipStream = new GZipStream(mem, CompressionMode.Compress))
        {
            zipStream.Write(bstr);
        }
        
        return mem.ToArray();
    }

    public static T? Deserialize<T>(byte[] data)
    {
        using var mem = new MemoryStream(data);
        using var os = new MemoryStream();
        using (var zipStream = new GZipStream(mem, CompressionMode.Decompress))
        {
            zipStream.CopyTo(os);
        }

        string jstr = Encoding.UTF8.GetString(os.ToArray());
        return JsonSerializer.Deserialize<T>(jstr);
    }

}
