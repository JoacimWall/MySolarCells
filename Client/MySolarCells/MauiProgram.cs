namespace MySolarCells;

using CommunityToolkit.Maui;
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
                fonts.AddFont("mysolarcells.ttf", "AppIconsFont");
            });

#if DEBUG
		builder.Logging.AddDebug();

        MySolarCellsGlobals.ConsoleWriteLineDebug("CacheDirectory   " + FileSystem.CacheDirectory);
        MySolarCellsGlobals.ConsoleWriteLineDebug("AppDataDirectory   " + FileSystem.AppDataDirectory);
#endif
        var app = builder.Build();
        //we must initialize our service helper before using it
        ServiceHelper.Initialize(app.Services);
        return app;
	}
}

