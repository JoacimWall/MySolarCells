namespace MySolarCells;
using CommunityToolkit.Maui;
using Plugin.LocalNotification;
using SkiaSharp.Views.Maui.Controls.Hosting;
using Syncfusion.Maui.Core.Hosting;
public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
       
        builder
            .UseMauiApp<App>()
            .ConfigureSyncfusionCore()
            .UseSkiaSharp()
            .UseMauiCommunityToolkit()
            .ConfigureServices()
            .ConfigureViewModels()
            .ConfigureViews()
            .ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("MySolarCells.ttf", "AppIconsFont");
            });

        if (DeviceInfo.Platform != DevicePlatform.MacCatalyst)
            builder.UseLocalNotification();
#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();
        //we must initialize our service helper before using it
        ServiceHelper.Initialize(app.Services);

		var logService = ServiceHelper.GetService<ILogService>();
		logService.ConsoleWriteLineDebug("CacheDirectory   " + FileSystem.CacheDirectory);
		logService.ConsoleWriteLineDebug("AppDataDirectory   " + FileSystem.AppDataDirectory);

        return app;
	}
    
}

