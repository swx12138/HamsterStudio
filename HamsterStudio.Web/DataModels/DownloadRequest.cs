using HamsterStudio.Web.Strategies.Request;
using HamsterStudio.Web.Strategies.StreamCopy;

namespace HamsterStudio.Web.DataModels;

public record DownloadRequest( // 不可变数据模型（符合ISP）
    Uri Url,
    IRequestStrategy RequestStrategy,
    IHttpContentCopyStrategy ContentCopyStrategy,
    int MaxConnections = 1
);
