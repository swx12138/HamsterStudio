using HamsterStudio.FileManager.Models;
using Microsoft.Data.Sqlite;
using System.IO;

namespace HamsterStudio.FileManager.Services
{
    /// <summary>
    /// SQLite 标记数据库服务
    /// </summary>
    public class TagDatabaseService : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly string _dbPath;

        public string DatabasePath => _dbPath;

        public TagDatabaseService()
        {
            _dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "HamsterStudio",
                "FileManager",
                "filetags.db");

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
                CREATE TABLE IF NOT EXISTS FileTags (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    FilePath TEXT NOT NULL UNIQUE,
                    Tag INTEGER NOT NULL DEFAULT 0,
                    UpdatedAt TEXT NOT NULL
                );
                CREATE INDEX IF NOT EXISTS idx_filetags_filepath ON FileTags(FilePath);
                CREATE INDEX IF NOT EXISTS idx_filetags_tag ON FileTags(Tag);
            ";
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// 设置文件的标记
        /// </summary>
        public void SetTag(string filePath, FileTagType tag)
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO FileTags (FilePath, Tag, UpdatedAt)
                VALUES (@path, @tag, @time)
                ON CONFLICT(FilePath) DO UPDATE SET
                    Tag = @tag,
                    UpdatedAt = @time;
            ";
            cmd.Parameters.AddWithValue("@path", filePath);
            cmd.Parameters.AddWithValue("@tag", (int)tag);
            cmd.Parameters.AddWithValue("@time", DateTime.Now.ToString("O"));
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// 获取单个文件的标记
        /// </summary>
        public FileTagType GetTag(string filePath)
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT Tag FROM FileTags WHERE FilePath = @path LIMIT 1";
            cmd.Parameters.AddWithValue("@path", filePath);

            var result = cmd.ExecuteScalar();
            if (result != null && result != DBNull.Value)
                return (FileTagType)Convert.ToInt32(result);

            return FileTagType.None;
        }

        /// <summary>
        /// 批量获取文件的标记
        /// </summary>
        public Dictionary<string, FileTagType> GetTags(IEnumerable<string> filePaths)
        {
            var result = new Dictionary<string, FileTagType>();
            var paths = filePaths.ToList();
            if (paths.Count == 0) return result;

            // 先初始化所有为 None
            foreach (var p in paths)
                result[p] = FileTagType.None;

            using var cmd = _connection.CreateCommand();
            var paramNames = new List<string>();
            for (int i = 0; i < paths.Count; i++)
            {
                var paramName = $"@p{i}";
                paramNames.Add(paramName);
                cmd.Parameters.AddWithValue(paramName, paths[i]);
            }

            cmd.CommandText = $"SELECT FilePath, Tag FROM FileTags WHERE FilePath IN ({string.Join(",", paramNames)})";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var path = reader.GetString(0);
                var tag = (FileTagType)reader.GetInt32(1);
                result[path] = tag;
            }

            return result;
        }

        /// <summary>
        /// 删除文件标记
        /// </summary>
        public void RemoveTag(string filePath)
        {
            SetTag(filePath, FileTagType.None);
        }

        public void Dispose()
        {
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}
