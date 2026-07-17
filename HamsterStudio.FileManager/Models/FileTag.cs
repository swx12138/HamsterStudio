namespace HamsterStudio.FileManager.Models
{
    /// <summary>
    /// 文件标记类型（位标志：喜欢/收藏独立，喜欢/不喜欢互斥）
    /// </summary>
    [Flags]
    public enum FileTagType
    {
        None = 0,
        Like = 1,
        Favorite = 2,
        Dislike = 4
    }

    /// <summary>
    /// 文件标记数据库记录
    /// </summary>
    public class FileTagRecord
    {
        public long Id { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public FileTagType Tag { get; set; } = FileTagType.None;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
