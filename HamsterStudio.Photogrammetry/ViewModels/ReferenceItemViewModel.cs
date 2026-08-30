using CommunityToolkit.Mvvm.ComponentModel;
using HamsterStudio.Barefeet.MVVM;
using HamsterStudio.Photogrammetry.Models;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HamsterStudio.Photogrammetry.ViewModels;

/// <summary>
/// 单个参考图条目，包装 <see cref="ReferenceModel"/>。
/// </summary>
public partial class ReferenceItemViewModel : ViewModel
{
    private ImageSource? _thumbnail;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FileName))]
    [NotifyPropertyChangedFor(nameof(ImageUri))]
    [NotifyPropertyChangedFor(nameof(Thumbnail))]
    private string _path = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DemandDegreeIndex))]
    private ReferenceDemandDegree _demandDegree = ReferenceDemandDegree.Normal;

    [ObservableProperty]
    private bool _isShot;

    /// <summary>
    /// 文件名（用于搜索与显示）。
    /// </summary>
    public string FileName => string.IsNullOrWhiteSpace(Path) ? "(空)" : System.IO.Path.GetFileName(Path);

    /// <summary>
    /// 供图片查看器使用的 Uri。
    /// </summary>
    public Uri? ImageUri => File.Exists(Path) ? new Uri(Path, UriKind.Absolute) : null;

    /// <summary>
    /// 缩略图。
    /// </summary>
    public ImageSource? Thumbnail => _thumbnail;

    /// <summary>
    /// 用于 ComboBox 绑定（0=普通，1=必须）。
    /// </summary>
    public int DemandDegreeIndex
    {
        get => DemandDegree == ReferenceDemandDegree.Must ? 1 : 0;
        set => DemandDegree = value == 1 ? ReferenceDemandDegree.Must : ReferenceDemandDegree.Normal;
    }

    public ReferenceItemViewModel() : this(null)
    {
    }

    public ReferenceItemViewModel(ILogger<ReferenceItemViewModel>? logger) : base(logger)
    {
    }

    partial void OnPathChanged(string value)
    {
        _thumbnail = LoadThumbnail(value);
    }

    private static ImageSource? LoadThumbnail(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return null;
        }

        try
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(path, UriKind.Absolute);
            bitmap.DecodePixelWidth = 256;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }
        catch
        {
            return null;
        }
    }

    public ReferenceModel ToModel() => new()
    {
        DemandDegree = DemandDegree,
        Path = Path,
        IsShot = IsShot,
    };

    public static ReferenceItemViewModel FromModel(ReferenceModel model) => new()
    {
        DemandDegree = model.DemandDegree,
        Path = model.Path,
        IsShot = model.IsShot,
    };
}
