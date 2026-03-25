using HamsterStudio.ImageTool.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace HamsterStudio.ImageTool;

public static class ImageToolProfile
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.AddTransient<ImageToolMainViewModel>();
        services.AddTransient<ExposureAdjustmentViewModel>();
    }

}
