namespace HamsterStudio.Web.Models;

public enum DownloadTaskStatus
{
    /// <summary>排队中</summary>
    Queued,

    /// <summary>下载中</summary>
    Downloading,

    /// <summary>已暂停</summary>
    Paused,

    /// <summary>已完成</summary>
    Completed,

    /// <summary>失败</summary>
    Failed,

    /// <summary>已取消</summary>
    Cancelled,
}

public enum DownloadPackageStatus
{
    /// <summary>排队等待</summary>
    Queued,

    /// <summary>下载中（至少一个子任务活跃）</summary>
    Downloading,

    /// <summary>全部暂停</summary>
    Paused,

    /// <summary>全部成功</summary>
    Completed,

    /// <summary>部分失败（至少一个完成、至少一个失败/取消）</summary>
    PartialFailed,

    /// <summary>全部失败</summary>
    Failed,

    /// <summary>已取消</summary>
    Cancelled,
}
