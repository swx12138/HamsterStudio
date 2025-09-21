using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Barefeet.SysCall;
using HamsterStudio.Toolkits.Services;
using System.Text.Json.Serialization;
using System.Windows.Input;

namespace HamsterStudio.WallpaperEngine.Services;

public class ImageModelDim
{
    [JsonIgnore]
    public string FileName => System.IO.Path.GetFileName(Path);

    [JsonPropertyName("path")]
    public string Path { get; set; }

    [JsonPropertyName("meta")]
    public ImageMetaInfo MetaInfo { get; init; }

    [JsonPropertyName("mark")]
    public bool Mark { get; set; } = false;

    [JsonIgnore]
    public ICommand RevalInExplorerCommand { get; }

    [JsonIgnore]
    public ICommand RemoveImageCommand { get; init; }

    public ImageModelDim()
    {
        RevalInExplorerCommand = new RelayCommand(() => ShellApi.SelectFile(Path));
    }

}
