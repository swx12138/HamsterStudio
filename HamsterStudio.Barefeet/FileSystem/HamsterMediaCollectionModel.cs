using HamsterStudio.Barefeet.Logging;

namespace HamsterStudio.Barefeet.FileSystem;

public class MediaRecord
{
    public HashSet<string> MediaIds { get; set; } = [];
    public int MediaCount { get; set; } = 0;
}

public interface IHamsterMediaCollection
{
    void Prepare();
    bool Update(string authorId, string mediaId, int mediaCount);
    public void Enumerate(Action<string, MediaRecord> action);
}

public class HamsterMediaCollectionModel(string storageHome) : IHamsterMediaCollection
{
    public Dictionary<string, MediaRecord> MediaCollectionInfo { get; set; } = [];

    public bool Update(string authorId, string mediaId, int mediaCount)
    {
        if (!MediaCollectionInfo.TryGetValue(authorId, out MediaRecord? record))
        {
            record = new MediaRecord();
            MediaCollectionInfo[authorId] = record;
        }

        record.MediaIds.Add(mediaId);
        record.MediaCount += mediaCount;

        return ShouldGroup(record);
    }

    public void Enumerate(Action<string, MediaRecord> action)
    {
        foreach (var kvp in MediaCollectionInfo)
        {
            action(kvp.Key, kvp.Value);
        }
    }

    public static bool ShouldGroup(MediaRecord record)
    {
        return record.MediaCount >= 3;
    }

    public void Prepare()
    {
        foreach (var dir in Directory.GetDirectories(storageHome))
        {
            var dirInfo = new DirectoryInfo(dir);
            var authorId = dirInfo.Name;
            var mediaIds = new HashSet<string>();
            int mediaCount = 0;
            foreach (var file in dirInfo.GetFiles())
            {
                var parts = file.Name.Split('_');
                if (parts.Length > 3)
                {
                    var mediaId = parts[0];
                    mediaIds.Add(mediaId);
                    mediaCount++;
                }
                else
                {
                    Logger.Shared.Warning($"File '{file.FullName}' does not conform to expected naming pattern.");
                    continue;
                }
            }
            if (mediaIds.Count > 0)
            {
                MediaCollectionInfo[authorId] = new MediaRecord
                {
                    MediaIds = mediaIds,
                    MediaCount = mediaCount
                };

            }
            else
            {
                Logger.Shared.Warning($"No valid media files found in directory '{dirInfo.FullName}'.");
            }
        }

    }

}
