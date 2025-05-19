namespace HamsterStudio.Web.Tools;

public record DownloadRequest( // 不可变数据模型（符合ISP）
    Uri Url,
    Dictionary<string, string> Headers,
    int? MaxConnections = null,
    int? ChunkSize = null
);
