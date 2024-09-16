namespace MySolarCells.Services;

public static class ServicesExtensions
{
    public static MauiAppBuilder ConfigureServices(this MauiAppBuilder builder)
    {
        //Singleton
        builder.Services.AddSingleton<IDialogService, DialogService>();
        builder.Services.AddSingleton(new MscDbContext());
        builder.Services.AddSingleton<ISettingsService, SettingsService>();
        builder.Services.AddSingleton<IHistoryDataService, HistoryDataService>();
        builder.Services.AddSingleton<IRoiService, RoiService>();
        builder.Services.AddSingleton<IEnergyChartService, EnergyChartService>();
        builder.Services.AddSingleton<IDataSyncService, DataSyncService>();
        builder.Services.AddSingleton<ISaveAndView, SaveService>();
        builder.Services.AddSingleton<IInternetConnectionService, InternetConnectionService>();
        builder.Services.AddSingleton<IDialogService, DialogService>();
        builder.Services.AddSingleton<IAnalyticsService, AnalyticsService>();
        builder.Services.AddSingleton<ILogService, LogService>();
        builder.Services.AddSingleton<ISettingsService, SettingsService>();
        builder.Services.AddSingleton<IHomeService, HomeService>();
        builder.Services.AddSingleton<ISetOrientationService, SetOrientationService>();
        //Transient
        builder.Services.AddTransient<IMyRestClientGeneric, MyRestClientGeneric>();
        builder.Services.AddTransient<IRestClient, RestClient>();

        return builder;
    }
}

