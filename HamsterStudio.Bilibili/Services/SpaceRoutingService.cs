using System.Text.Json;
using HamsterStudio.Barefeet.Services;
using Microsoft.Extensions.Logging;

namespace HamsterStudio.Bilibili.Services;

/// <summary>
/// 用户空间路由服务 —— 根据 uid 决定文件归属的子目录。
/// 配置来源: Configurations/config.json
/// </summary>
public class SpaceRoutingService
{
    private readonly ILogger<SpaceRoutingService> _logger;
    private readonly Dictionary<string, string> _uidToSubdir = new(StringComparer.Ordinal);

    public SpaceRoutingService(ILogger<SpaceRoutingService> logger, DirectoryMgmt directoryMgmt)
    {
        _logger = logger;

        var configPath = Path.Combine(directoryMgmt.TemporaryHome, "bili_subdir_config.json");
        if (!File.Exists(configPath))
        {
            _logger.LogWarning("Space routing config not found: {Path}", configPath);
            return;
        }

        try
        {
            var json = File.ReadAllText(configPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            };
            var entries = JsonSerializer.Deserialize<List<RoutingEntry>>(json, options) ?? [];

            foreach (var entry in entries)
            {
                foreach (var uid in entry.SpaceIds)
                {
                    _uidToSubdir[uid] = entry.Subdir;
                }
            }

            _logger.LogInformation("SpaceRoutingService loaded {Count} uid mappings", _uidToSubdir.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load space routing config");
        }
    }

    /// <summary>根据 uid 获取对应的子目录名，无匹配则返回 null</summary>
    public string? GetSubdir(string? uid)
    {
        if (string.IsNullOrEmpty(uid))
            return null;
        return _uidToSubdir.TryGetValue(uid, out var subdir) ? subdir : null;
    }

    private record RoutingEntry(string Subdir, List<string> SpaceIds);
}
