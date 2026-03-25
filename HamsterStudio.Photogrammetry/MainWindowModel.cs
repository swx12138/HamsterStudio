using CommunityToolkit.Mvvm.ComponentModel;
using HamsterStudio.Photogrammetry.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace HamsterStudio.Photogrammetry;

partial class MainWindowModel : ObservableObject
{
    public PhotogrammetryMainViewModel MainViewModel { get; set; }

    public MainWindowModel()
    {
        MainViewModel = App.ServiceProvider?.GetRequiredService<PhotogrammetryMainViewModel>()!;


    }

}
