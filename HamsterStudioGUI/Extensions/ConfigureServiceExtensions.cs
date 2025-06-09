using HamsterStudio.Barefeet.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HamsterStudioGUI.Extensions;

internal static class ConfigureServiceExtensions
{
    public static void ConfigureService(this IServiceCollection services, string home)
    {
        services.AddSingleton(new DirectoryMgmt(home));
    }
}
