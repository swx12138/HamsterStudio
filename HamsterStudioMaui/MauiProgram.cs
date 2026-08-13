using HamsterStudio.Barefeet.Services;
using HamsterStudio.Web.Services;
using HamsterStudioMaui.Services;
using HamsterStudioMaui.ViewModels;
using Microsoft.Extensions.Logging;

namespace HamsterStudioMaui;

public static class MauiProgram
{
	private static MauiApp? _app;
	internal static MauiApp? App => _app;

	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		builder.Services.AddSingleton<OfflineProcessService>();
		builder.Services.AddTransient<MainPageModel>();

		builder.Services.AddSingleton(cp =>
		{
			var logger = cp.GetRequiredService<ILogger<FileTransferProtocolService>>();
			return new DirectoryMgmt("/sdcard/HamsterStudio/", logger);
        });
		builder.Services.AddSingleton<FileTransferProtocolService>();

		_app = builder.Build();
		return _app;
	}
}
