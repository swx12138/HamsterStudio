using System.IO.Compression;
using System.Text;

namespace HamsterStudio.Encryption.Services;

public class Blather
{
    public static readonly byte[] GZipHeader = [
        0x1f, 0x8b, 0x08, 0x00,
        0x00, 0x00, 0x00, 0x00,
        0x00, 0x0a
    ];

    private static async Task<byte[]> CompressData(byte[] input)
    {
        using var buffer = new MemoryStream();
        using (var compressStream = new GZipStream(buffer, CompressionMode.Compress))
        {
            await compressStream.WriteAsync(input);
        }
        return buffer.ToArray();
    }

    private static async Task<byte[]> DecompressData(byte[] input)
    {
        using var stream = new MemoryStream(input);
        using var decompressStream = new GZipStream(stream, CompressionMode.Decompress);

        using var buffer = new MemoryStream();
        await decompressStream.CopyToAsync(buffer);

        return buffer.ToArray();
    }

    public static async Task<string> Encode(string plainText)
    {
        var data = Encoding.UTF8.GetBytes(plainText);
        var compressed_data = await CompressData(data);
        var b64 = Convert.ToBase64String(compressed_data.Skip(10).ToArray());
        return HaEncoding.ToHa(b64);
    }

    public static async Task<string> Decode(string str)
    {
        var b64 = HaEncoding.FromHa(str);
        var compressed_data = GZipHeader.Concat(Convert.FromBase64String(b64)).ToArray();
        var data = await DecompressData(compressed_data);
        return Encoding.UTF8.GetString(data);
    }

}
