using HamsterStudio.Barefeet.MVVM;
using HamsterStudio.ImageTool.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HamsterStudio.ImageTool;

partial class MainWindowModel : ViewModel
{
    public ImageToolMainViewModel MainViewModel { get; }
    public ImageStitcherViewModel StitcherViewModel { get; }

    public MainWindowModel() : base(null)
    {
        base.logger = App.ServiceProvider?.GetRequiredService<ILogger<MainWindowModel>>()!;
        MainViewModel = App.ServiceProvider?.GetRequiredService<ImageToolMainViewModel>()!;
        StitcherViewModel = App.ServiceProvider?.GetRequiredService<ImageStitcherViewModel>()!;
    }

}
