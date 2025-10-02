using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Barefeet.SysCall;
using HamsterStudio.Toolkits.DragDrop;
using HamsterStudio.Toolkits.Services;
using System.Text.Json.Serialization;
using System.Windows.Input;

namespace HamsterStudio.WallpaperEngine.Services;

public class ImageModelDim : IDragable
{
    public string DataFormat => nameof(ImageModelDim);

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

    public static ImageModelDim FromPath(string path, Action<ImageModelDim> callback, ImageMetaInfoReadService imageMetaInfoReadService, bool mark = false)
    {
        return new ImageModelDim()
        {
            Path = path,
            MetaInfo = imageMetaInfoReadService.Read(path),
            Mark = mark,
            RemoveImageCommand = new RelayCommand<ImageModelDim>(img =>
            {
                if (img == null) return;
                callback(img);
            })
        };
    }

}
