using System.IO;
using System.Text.Json.Serialization;

namespace HamsterStudio.Toolkits.Services;

public readonly struct ImageMetaInfo
{
    [JsonPropertyName("width")]
    public long Width { get; init; }

    [JsonPropertyName("height")]
    public long Height { get; init; }

    [JsonPropertyName("type")]
    public string Type { get; init; }
}

public interface IImageMetaInfoReader
{
    bool Accept(in byte[] header);
    ImageMetaInfo Read(in FileStream ifs);
}

public class ImageMetaInfoReadService
{
    public List<IImageMetaInfoReader> ImageMetaInfoReaders = [];

    public ImageMetaInfo Read(in string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException(Path.GetFullPath(path));
        }

        using FileStream ifs = File.OpenRead(path);
        var headerRaw = new byte[12];
        ifs.Read(headerRaw, 0, headerRaw.Length);
        foreach (var reader in ImageMetaInfoReaders)
        {
            if (reader.Accept(headerRaw))
            {
                return reader.Read(ifs);
            }
        }

        throw new NotSupportedException("Not support file format");
    }

}
