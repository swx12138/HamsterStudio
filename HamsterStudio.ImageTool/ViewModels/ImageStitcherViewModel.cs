using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Barefeet.MVVM;
using HandyControl.Controls;
using LiteObservableCollections;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace HamsterStudio.ImageTool.ViewModels;

[DisplayName("ImageStitcher")]
public partial class ImageStitcherViewModel : KnownViewModel
{
    static ImageSelector Placeholder => new ImageSelector() { Filter = "(.png)|*.png|(.jpg)|*.jpg" };

    [ObservableProperty]
    private int gridColumns = 3;

    [ObservableProperty]
    private int gridRows = 3;

    [ObservableProperty]
    private ObservableCollection<ImageSelector> selectedFiles = new(Enumerable.Range(0, 3 * 3).Select(x => Placeholder));

    public ImageStitcherViewModel(ILogger<ImageStitcherViewModel> logger) : base(logger)
    {
        DisplayName = "ImageStitcher";
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(GridColumns) || e.PropertyName == nameof(GridRows))
        {
            RecapacitySelectedFiles();
        }
        base.OnPropertyChanged(e);
    }

    private void RecapacitySelectedFiles()
    {
        int newSize = GridRows * GridColumns;
        if (newSize == SelectedFiles.Count)
        {
            return;
        }
        else if (newSize > SelectedFiles.Count)
        {
            SelectedFiles.AddRange(Enumerable.Range(0, newSize - SelectedFiles.Count).Select(x => Placeholder));
        }
        else
        {
            SelectedFiles.RemoveRange(SelectedFiles.Skip(newSize));
        }
    }

    [RelayCommand]
    private void DoStitchImage()
    {





    }

}
