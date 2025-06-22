using HamsterStudio.SinaWeibo.Models;
using System.Text.Json;

namespace HamsterStudio.SinaWeibo.Services;

internal class UserNameMap
{
    public string Home { get; }

    public Dictionary<string, string> CacheMap { get; } = [];
    public Dictionary<string, string> BannedMap { get; } = [];

    private readonly JsonSerializerOptions DefaultSaveMapOpts = new() { WriteIndented = true };

    public UserNameMap(FileMgmt fileMgmt)
    {
        Home = Path.Combine(fileMgmt.Home, ".doi");

        BannedMap = LoadMap("banned.json");
        CacheMap = LoadMap("cache.json");
    }

    public Dictionary<string, string> LoadMap(string name)
    {
        string cacheFile = Path.Combine(Home, name);
        using var infile = File.OpenRead(cacheFile);
        return JsonSerializer.Deserialize<Dictionary<string, string>>(infile)!;
    }

    public void UpdateCache(UserModel user)
    {
        if (CacheMap.ContainsKey(user.Idstr)) { return; }
        CacheMap[user.Idstr] = user.ScreenName;
        SaveMap("cache.json", CacheMap);
    }

    public void SaveMap(string name, Dictionary<string, string> map)
    {
        string cacheFile = Path.Combine(Home, name);
        using var outfile = File.Create(cacheFile);
        JsonSerializer.Serialize(outfile, map, DefaultSaveMapOpts);
    }

}
