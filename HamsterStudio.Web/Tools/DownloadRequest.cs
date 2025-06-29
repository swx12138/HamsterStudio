using HamsterStudio.Web.Strategies.Request;

namespace HamsterStudio.Web.Tools;

public record DownloadRequest( // 不可变数据模型（符合ISP）
    Uri Url,
    IRequestStrategy RequestStrategy,
    int MaxConnections = 1
);
