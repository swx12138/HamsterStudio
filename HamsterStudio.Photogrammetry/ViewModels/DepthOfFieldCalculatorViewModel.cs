using CommunityToolkit.Mvvm.ComponentModel;
using HamsterStudio.Barefeet.MVVM;
using HamsterStudio.Photogrammetry.Services;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;

namespace HamsterStudio.Photogrammetry.ViewModels;

public partial class DepthOfFieldCalculatorViewModel : ViewModel
{
    const string CategroyName = "拍摄信息";

    [ObservableProperty]
    [property: Category(CategroyName)]
    [property: DisplayName("焦距（mm）")]
    private double _focalLength = 35.0;

    [ObservableProperty]
    [property: Category(CategroyName)]
    [property: DisplayName("光圈")]
    private double _aperture = 1.4;

    [ObservableProperty]
    [property: Category(CategroyName)]
    [property: DisplayName("对焦距离(m)")]
    private double _focusDistance = 2;

    [ObservableProperty]
    [property: Browsable(false)]
    private DoFResult result;

    [Browsable(false)]
    public ICommand? DestroyThisViewModelCommand { get; set; }

    public DepthOfFieldCalculatorViewModel(ILogger<DepthOfFieldCalculatorViewModel>? logger) : base(logger)
    {
        result = DepthOfFieldCalculator.Calculate(FocalLength, Aperture, FocusDistance);
    }

    public DepthOfFieldCalculatorViewModel() : this(null)
    {
    }

    ~DepthOfFieldCalculatorViewModel()
    {
        Trace.TraceInformation("Deconstructing DepthOfFieldCalculatorViewModel");
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(FocalLength) or nameof(Aperture) or nameof(FocusDistance))
        {
            Result = DepthOfFieldCalculator.Calculate(FocalLength, Aperture, FocusDistance);
        }
        base.OnPropertyChanged(e);
    }

}
