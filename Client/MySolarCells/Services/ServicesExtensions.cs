namespace MySolarCells.Services;

public static class ServicesExtensions
{
    public static MauiAppBuilder ConfigureServices(this MauiAppBuilder builder)
    {
        //Singelton
        builder.Services.AddSingleton<IDialogService, DialogService>();
        builder.Services.AddSingleton<ISettingsService, SettingsService>();
        builder.Services.AddSingleton<ITibberService, TibberService>();
        builder.Services.AddSingleton<IRoiService, RoiService>();

        //Transient
        builder.Services.AddTransient<IRestClient, RestClient>();

        return builder;
    }
}

