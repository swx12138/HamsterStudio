using HamsterStudio.Barefeet.Logging;
using System.Reflection;

namespace HamsterStudio.Barefeet.Services;

public class DataStorageMgmt
{
    private Dictionary<string, string> _data = [];
    private readonly DirectoryMgmt _directoryMgmt;
    private const string DatFilename = "underwear.dat";

    public event EventHandler BeforePersist;

    public DataStorageMgmt(DirectoryMgmt directoryMgmt)
    {
        _directoryMgmt = directoryMgmt;
        try
        {
            var bin = File.ReadAllBytes(Path.Combine(_directoryMgmt.TemporaryHome, DatFilename));
            _data = BinaryDataSerializer.Deserialize<Dictionary<string, string>>(bin) ?? [];
        }
        catch
        { }
    }

    public void Set<T>(string key, T value)
    {
        try
        {
            var bin = BinaryDataSerializer.Serialize(value);
            var b64str = Convert.ToBase64String(bin);
            _data[key] = b64str;
        }
        catch (Exception ex)
        {
            Logger.Shared.Error($"Save {key} data failed.");
            Logger.Shared.Critical(ex);
        }
    }

    public T? Get<T>(string key)
    {
        string b64str = _data.TryGetValue(key, out string? value) ? value : string.Empty;
        var bin = Convert.FromBase64String(b64str);
        if (bin.Length == 0)
        {
            return default;
        }

        var data = BinaryDataSerializer.Deserialize<T>(bin);
        return data ?? default;
    }

    public void Persist()
    {
        BeforePersist?.Invoke(this, null);

        var bin = BinaryDataSerializer.Serialize(_data);
        if (!Directory.Exists(_directoryMgmt.TemporaryHome))
        {
            Directory.CreateDirectory(_directoryMgmt.TemporaryHome);
        }
        File.WriteAllBytes(Path.Combine(_directoryMgmt.TemporaryHome, DatFilename), bin);
    }

}
