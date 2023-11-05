using Shiny.Jobs;
using Shiny.Support.Repositories;

namespace MySolarCells.Services;

public static class ServicesExtensions
{
    public static MauiAppBuilder ConfigureServices(this MauiAppBuilder builder)
    {
        //Singelton
        builder.Services.AddSingleton<IDialogService, DialogService>();
        builder.Services.AddSingleton<ISettingsService, SettingsService>();
        builder.Services.AddSingleton<IRoiService, RoiService>();
        builder.Services.AddSingleton<IEnergyChartService, EnergyChartService>();
        builder.Services.AddSingleton<IDataSyncService, DataSyncService>();
        builder.Services.AddSingleton<ISaveAndView, SaveService>();
        //builder.Services.AddSingleton<IJobManager, JobManager>();
        //builder.Services.AddSingleton<IRepository, Repository>();
        
        //Transient
        builder.Services.AddTransient<IRestClient, RestClient>();

        return builder;
    }
}

