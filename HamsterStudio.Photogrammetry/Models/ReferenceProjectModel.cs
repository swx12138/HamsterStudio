using System.Text.Json.Serialization;

namespace HamsterStudio.Photogrammetry.Models;

public class ReferenceProjectModel
{
    public const string EmptyProjectName = "Empty Project";

    [JsonPropertyName("name")]
    public string Name { get; set; } = EmptyProjectName;

    [JsonPropertyName("desc")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("refs")]
    public List<ReferenceModel> References { get; set; } = [];
}

public enum ReferenceDemandDegree
{
    Normal, Must
}

public class ReferenceModel
{
    [JsonPropertyName("demand")]
    public ReferenceDemandDegree DemandDegree { get; set; } = ReferenceDemandDegree.Normal;

    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    [JsonPropertyName("shot")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool IsShot { get; set; }
}
