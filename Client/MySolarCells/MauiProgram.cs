namespace MySolarCells;

using CommunityToolkit.Maui;
using MySolarCells.Jobs;
using Plugin.LocalNotification;
using Shiny;
using Shiny.Jobs;
using SkiaSharp.Views.Maui.Controls.Hosting;
using Syncfusion.Maui.Core.Hosting;
public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
        builder.Services.AddDbContext<MscDbContext>(
            options => options.UseSqlite($"Filename={GetDataBasePath()}", x => x.MigrationsAssembly(nameof(MySolarCellsSQLite))));
        

        builder
            .UseMauiApp<App>()
            .ConfigureSyncfusionCore()
            .UseSkiaSharp()
            .UseMauiCommunityToolkit()
            .UseShiny()
            .ConfigureServices()
            .ConfigureViewModels()
            .ConfigureViews()
            .ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("mysolarcells.ttf", "AppIconsFont");
            });

        if (DeviceInfo.Platform != DevicePlatform.MacCatalyst)
            builder.UseLocalNotification();


        //var job = new JobInfo("DalySync",typeof(JobDalySync),BatteryNotLow: true,DeviceCharging: false,RunOnForeground: false,RequiredInternetAccess: true ? InternetAccess.Any : InternetAccess.None);
        //builder.Services.AddJob(job);
       
       


#if DEBUG
        builder.Logging.AddDebug();

        MySolarCellsGlobals.ConsoleWriteLineDebug("CacheDirectory   " + FileSystem.CacheDirectory);
        MySolarCellsGlobals.ConsoleWriteLineDebug("AppDataDirectory   " + FileSystem.AppDataDirectory);
#endif
        var app = builder.Build();
        //we must initialize our service helper before using it
        ServiceHelper.Initialize(app.Services);

        //using var dbContext = new MscDbContext();
        //MySolarCellsGlobals.SelectedHome = dbContext.Home.FirstOrDefault(x => x.HomeId == this.settingsService.SelectedHomeId);

        return app;
	}
    public static string GetDataBasePath()
    {
        string databasePath ="";
        string databaseName = "Db_v_1.db3";
        if (DeviceInfo.Platform == DevicePlatform.Android)
        {
            databasePath = Path.Combine(FileSystem.AppDataDirectory, databaseName);
        }
        else if (DeviceInfo.Platform == DevicePlatform.iOS)
        {
            SQLitePCL.Batteries_V2.Init();
            //databasePath = Path.Combine(FileSystem.AppDataDirectory, databaseName);
            databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "..", "Library");
        }
        else if (DeviceInfo.Platform == DevicePlatform.MacCatalyst)
        {
            //Detta för att köra på mac som ios app
            //SQLitePCL.Batteries_V2.Init();
            //databasePath = Path.Combine(FileSystem.AppDataDirectory, databaseName);
            //databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "..", "Library");

            //detta som mac catalyst local
            databasePath = Path.Combine(FileSystem.AppDataDirectory, "MySolarCells");
            if (!Directory.Exists(databasePath))
            {
                Directory.SetCurrentDirectory(FileSystem.AppDataDirectory);
                Directory.CreateDirectory("MySolarCells");
            }
        }
        
       

        var dbPath = Path.Combine(databasePath, "Db_v_1.db3");

#if DEBUG
       MySolarCellsGlobals.ConsoleWriteLineDebug("DataBasePath   " + dbPath);
#endif
        return dbPath;
    }
}

