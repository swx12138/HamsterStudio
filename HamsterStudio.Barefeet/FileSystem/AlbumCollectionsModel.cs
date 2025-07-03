using HamsterStudio.Barefeet.FileSystem.Interfaces;
using HamsterStudio.Barefeet.Logging;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace HamsterStudio.Barefeet.FileSystem;

public class AlbumCollectionModel
{
    [JsonPropertyName("owner")]
    public string OwnerName { get; set; } = string.Empty;

    [JsonPropertyName("count")]
    public int FileCount { get; set; } = 0;

    [JsonPropertyName("albums")]
    public HashSet<string> Albums { get; set; } = [];

    static bool wasHot;

    [JsonIgnore]
    public bool IsHot
    {
        get
        {
            bool isHot = Test();
            if (!wasHot && isHot)
            {
                wasHot = isHot;
                Logger.Shared.Information($"{OwnerName} has been hot!!");
            }
            return isHot;
        }
    }

    public AlbumCollectionModel()
    {
        wasHot = Test();
    }

    public virtual bool Test() { return (Albums.Count >= 2 && FileCount >= 9) || Albums.Count >= 5; }

    public void DoEnfeoff()
    {
        throw new NotImplementedException();
    }

}

public class AlbumCollectionsModel
{
    [JsonPropertyName("collections")]
    public Dictionary<string, AlbumCollectionModel> Collections { get; set; } = [];

    public bool AddAlbum(AlbumCollectionModel album, bool isInit = false)
    {
        if (Collections.TryGetValue(album.OwnerName, out var collection))
        {
            collection.FileCount += album.FileCount;
            foreach (var alb in album.Albums)
            {
                collection.Albums.Add(alb);
            }
        }
        else
        {
            Collections[album.OwnerName] = album;
        }

        return isInit ? Collections[album.OwnerName].Test() : Collections[album.OwnerName].IsHot;
    }

    public static AlbumCollectionsModel Load(string filePath, IFilenameInfoParser parser)
    {
        if (!File.Exists(filePath))
        {
            var model = new AlbumCollectionsModel();
            if (parser != null)
            {
                var home = Directory.GetParent(Path.GetDirectoryName(filePath)).FullName;
                foreach (var files in Directory.GetFiles(home))
                {
                    string filename = Path.GetFileName(files);
                    var info = parser.Parse(filename);
                    if (info != null && !string.IsNullOrEmpty(info.Title) && !string.IsNullOrEmpty(info.Owner))
                    {
                        var album = new AlbumCollectionModel
                        {
                            OwnerName = info.Owner,
                            FileCount = 1,
                            Albums = [info.Title]
                        };
                        model.AddAlbum(album, true);
                    }
                    else
                    {
                        Logger.Shared.Warning($"Failed to parse filename: {filename}");
                    }
                }
            }
            return model;
        }
        var json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<AlbumCollectionsModel>(json) ?? new AlbumCollectionsModel();
    }

    public void Save(string filePath)
    {
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions()
        {
            // 1. 设置编码器允许所有 Unicode 字符
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),

            // 2. 设置缩进格式（可选，使输出更易读）
            WriteIndented = true,
        });
        File.WriteAllText(filePath, json);
    }

}
