using CommunityToolkit.Mvvm.ComponentModel;
using HamsterStudio.Barefeet.MVVM;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Windows.Input;

namespace HamsterStudio.Photogrammetry.ViewModels;

public partial class EquivalentExposureCalculatorViewModel : ViewModel
{
    const string CategroyInputName = "参考镜头信息";
    const string CategroyOutputName = "等效镜头信息";

    [ObservableProperty]
    [property: Category(CategroyInputName)]
    [property: DisplayName("焦距（mm）")]
    private double _focalLength = 85;

    [ObservableProperty]
    [property: Category(CategroyInputName)]
    [property: DisplayName("光圈")]
    private double _aperture = 1.4;

    [ObservableProperty]
    [property: Category(CategroyOutputName)]
    [property: DisplayName("目标焦距（mm）")]
    private double _targetFocalLength = 200;

    [ObservableProperty]
    [property: Category(CategroyOutputName)]
    [property: DisplayName("等效光圈")]
    private double _equivalentTperture;

    [Browsable(false)]
    public ICommand? DestroyThisViewModelCommand { get; set; }

    public static double Calculate(double focalLength, double aperture, double targetFocalLength)
    {
        if (focalLength <= 0 || aperture <= 0 || targetFocalLength <= 0)
        {
            throw new ArgumentException("焦距、光圈和目标焦距必须大于零。");
        }
        return Math.Sqrt(targetFocalLength * aperture * aperture / focalLength);
    }

    public EquivalentExposureCalculatorViewModel(ILogger<EquivalentExposureCalculatorViewModel>? logger) : base(logger)
    {
        _equivalentTperture = Calculate(FocalLength, Aperture, TargetFocalLength);
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if(e.PropertyName is nameof(FocalLength) or nameof(Aperture) or nameof(TargetFocalLength))
        {
            EquivalentTperture = Calculate(FocalLength, Aperture, TargetFocalLength);
        }
        base.OnPropertyChanged(e);
    }

}
