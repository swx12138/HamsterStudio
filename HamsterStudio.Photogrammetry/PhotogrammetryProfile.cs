using HamsterStudio.Photogrammetry.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HamsterStudio.Photogrammetry;

public static class PhotogrammetryProfile
{
    public static void RegisterServices(IServiceCollection services)
    {
        // 注册景深计算器服务
        services.AddSingleton<LensCalculateService>();
        // 注册视图模型
        services.AddTransient<ViewModels.PhotogrammetryMainViewModel>();
        services.AddTransient<ViewModels.DepthOfFieldCalculatorViewModel>();
        services.AddTransient<ViewModels.EquivalentExposureCalculatorViewModel>();
        services.AddTransient<ViewModels.ReferenceProjectViewModel>();
        services.AddTransient<ViewModels.AngleOfViewCalculatorViewModel>();
        services.AddTransient<ViewModels.AngleOfViewEntryViewModel>();
    }
}
