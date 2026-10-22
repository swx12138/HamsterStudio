
using HamsterStudio.Barefeet.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HamsterStudio.Barefeet;

public static class BarefeetProfile
{
    public static void ConfigureService(this IServiceCollection services)
    {
        services.AddSingleton(sp => new DirectoryMgmt(@"E:\HamsterStudioHome", sp.GetService<ILogger>()));
        services.AddSingleton<DataStorageMgmt>();
    }
}
