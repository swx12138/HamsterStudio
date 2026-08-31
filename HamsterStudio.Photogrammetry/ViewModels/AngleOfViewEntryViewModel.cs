using CommunityToolkit.Mvvm.ComponentModel;
using HamsterStudio.Barefeet.MVVM;
using HamsterStudio.Photogrammetry.CameraLensMath;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Windows.Media;

namespace HamsterStudio.Photogrammetry.ViewModels;

/// <summary>
/// 供视角对比画布读取的条目接口。
/// </summary>
public interface IAngleOfViewEntry
{
    double FocalLength { get; }
    double AngleOfView { get; }
    Brush Brush { get; }
}

/// <summary>
/// 单个焦距的视角计算结果。
/// </summary>
public partial class AngleOfViewEntryViewModel : ViewModel, IAngleOfViewEntry
{
    const string CategoryInputName = "镜头参数";
    const string CategoryOutputName = "计算结果";

    [ObservableProperty]
    [property: Category(CategoryInputName)]
    [property: DisplayName("焦距（mm）")]
    [NotifyPropertyChangedFor(nameof(DisplayText))]
    private double _focalLength = 50.0;

    [ObservableProperty]
    [property: Browsable(false)]
    private Orientation _orientation = Orientation.Horizontal;

    [ObservableProperty]
    [property: Browsable(false)]
    private Brush _brush = Brushes.SteelBlue;

    [ObservableProperty]
    [property: Category(CategoryOutputName)]
    [property: DisplayName("视角（°）")]
    [property: ReadOnly(true)]
    [NotifyPropertyChangedFor(nameof(DisplayText))]
    private double _angleOfView;

    /// <summary>
    /// 用于列表胶囊展示的文本。
    /// </summary>
    [Browsable(false)]
    public string DisplayText => $"{FocalLength:0.#} mm  {AngleOfView:0.0}°";

    public AngleOfViewEntryViewModel(ILogger<AngleOfViewEntryViewModel>? logger) : base(logger)
    {
        Recalculate();
    }

    public AngleOfViewEntryViewModel() : this(null)
    {
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(FocalLength) or nameof(Orientation))
        {
            Recalculate();
        }
        base.OnPropertyChanged(e);
    }

    private void Recalculate()
    {
        AngleOfView = Orientation switch
        {
            Orientation.Diagonal => LensCalculator.GetDiagonalAngleOfView(FocalLength),
            Orientation.Vertical => LensCalculator.GetVerticalAngleOfView(FocalLength),
            _ => LensCalculator.GetHorizontalAngleOfView(FocalLength),
        };
    }
}
