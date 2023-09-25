using MySolarCells.Views.Roi;

namespace MySolarCells.Views;

public static class ViewsExtensions
{
    public static MauiAppBuilder ConfigureViews(this MauiAppBuilder builder)
    {
        
        //Onoarding
        builder.Services.AddTransient<ElectricitySupplierConnectView>();
        builder.Services.AddTransient<InverterConnectView>();
        builder.Services.AddTransient<EnergyCalculationParameterView>();
        builder.Services.AddTransient<FirstSyncView>();
        //ROI
        builder.Services.AddTransient<RoiView>();
        return builder;
    }
}

