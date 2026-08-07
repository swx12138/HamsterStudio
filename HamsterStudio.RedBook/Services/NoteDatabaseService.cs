using HamsterStudio.RedBook.Models.Sub;
using Microsoft.Data.Sqlite;
using System.Text.Json;

namespace HamsterStudio.RedBook.Services;

/// <summary>
/// SQLite 笔记数据库服务，以笔记ID为主键存储笔记元数据
/// </summary>
public class NoteDatabaseService : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly string _dbPath;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public string DatabasePath => _dbPath;

    public NoteDatabaseService()
    {
        _dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "HamsterStudio",
            "RedBook",
            "notes.db");

        var dir = Path.GetDirectoryName(_dbPath)!;
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        _connection = new SqliteConnection($"Data Source={_dbPath}");
        _connection.Open();

        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Notes (
                NoteId TEXT PRIMARY KEY NOT NULL,
                Type TEXT NOT NULL DEFAULT '',
                UserInfo TEXT NOT NULL DEFAULT '{}',
                TagList TEXT NOT NULL DEFAULT '[]',
                ImageTokens TEXT NOT NULL DEFAULT '[]',
                CreatedAt TEXT NOT NULL
            );
        ";
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// 保存笔记数据，以 NoteId 为主键，存在则更新
    /// </summary>
    public void SaveNote(
        string noteId,
        string type,
        UserInfoModel userInfo,
        List<TagModel> tagList,
        List<string> imageTokens)
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Notes (NoteId, Type, UserInfo, TagList, ImageTokens, CreatedAt)
            VALUES (@noteId, @type, @userInfo, @tagList, @imageTokens, @createdAt)
            ON CONFLICT(NoteId) DO UPDATE SET
                Type = @type,
                UserInfo = @userInfo,
                TagList = @tagList,
                ImageTokens = @imageTokens,
                CreatedAt = @createdAt;
        ";
        cmd.Parameters.AddWithValue("@noteId", noteId);
        cmd.Parameters.AddWithValue("@type", type);
        cmd.Parameters.AddWithValue("@userInfo", JsonSerializer.Serialize(userInfo, JsonOptions));
        cmd.Parameters.AddWithValue("@tagList", JsonSerializer.Serialize(tagList, JsonOptions));
        cmd.Parameters.AddWithValue("@imageTokens", JsonSerializer.Serialize(imageTokens, JsonOptions));
        cmd.Parameters.AddWithValue("@createdAt", DateTime.Now.ToString("O"));
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// 根据笔记ID获取笔记数据
    /// </summary>
    public NoteRecord? GetNote(string noteId)
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "SELECT NoteId, Type, UserInfo, TagList, ImageTokens, CreatedAt FROM Notes WHERE NoteId = @noteId LIMIT 1";
        cmd.Parameters.AddWithValue("@noteId", noteId);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new NoteRecord
            {
                NoteId = reader.GetString(0),
                Type = reader.GetString(1),
                UserInfo = JsonSerializer.Deserialize<UserInfoModel>(reader.GetString(2)) ?? new(),
                TagList = JsonSerializer.Deserialize<List<TagModel>>(reader.GetString(3)) ?? [],
                ImageTokens = JsonSerializer.Deserialize<List<string>>(reader.GetString(4)) ?? [],
                CreatedAt = reader.GetString(5),
            };
        }

        return null;
    }

    /// <summary>
    /// 删除笔记记录
    /// </summary>
    public void DeleteNote(string noteId)
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "DELETE FROM Notes WHERE NoteId = @noteId";
        cmd.Parameters.AddWithValue("@noteId", noteId);
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// 检查笔记是否已存在
    /// </summary>
    public bool NoteExists(string noteId)
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "SELECT COUNT(1) FROM Notes WHERE NoteId = @noteId";
        cmd.Parameters.AddWithValue("@noteId", noteId);

        var result = cmd.ExecuteScalar();
        return result != null && Convert.ToInt64(result) > 0;
    }

    public void Dispose()
    {
        _connection?.Close();
        _connection?.Dispose();
    }
}

/// <summary>
/// 笔记数据库记录
/// </summary>
public class NoteRecord
{
    public string NoteId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public UserInfoModel UserInfo { get; set; } = new();
    public List<TagModel> TagList { get; set; } = [];
    public List<string> ImageTokens { get; set; } = [];
    public string CreatedAt { get; set; } = string.Empty;
}
