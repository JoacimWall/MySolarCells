using MySolarCells.ViewModels.Roi;

namespace MySolarCells.ViewModels;

public static class ViewModelsExtensions
{
    public static MauiAppBuilder ConfigureViewModels(this MauiAppBuilder builder)
    {
        
        //Onboarding
        builder.Services.AddTransient<ElectricitySupplierConnectViewModel>();
        builder.Services.AddTransient<InverterConnectViewModel>();
        builder.Services.AddTransient<FirstSyncViewModel>();
        builder.Services.AddTransient<EnergyCalculationParameterViewModel>();
        //ROI
        builder.Services.AddTransient<RoiViewModel>();
        //Energy
        builder.Services.AddTransient<EnergyViewModel>();
        return builder;
    }
}

