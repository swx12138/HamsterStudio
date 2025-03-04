
namespace HamsterStudio.BraveShine.Models;

public class QualityDescription
{
    public long qn { get; set; }
    public string? desc { get; set; }
}

public class Durl
{
    public string url { get; set; }
    public long length { get; set; } // 更改类型为long，因为长度通常不会是负数且可能超过int范围
    public long order { get; set; }
    public long stream_type { get; set; }
    public long p2p_type { get; set; }
}

public class LivePlayUrlData
{
    public long current_quality { get; set; }
    public List<string>? accept_quality { get; set; }
    public long current_qn { get; set; }
    public List<QualityDescription>? quality_description { get; set; }
    public List<Durl> durl { get; set; }
}
