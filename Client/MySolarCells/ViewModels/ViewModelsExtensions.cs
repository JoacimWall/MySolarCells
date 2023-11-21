namespace MySolarCells.ViewModels;

public static class ViewModelsExtensions
{
    public static MauiAppBuilder ConfigureViewModels(this MauiAppBuilder builder)
    {
        
        //Onboarding
        builder.Services.AddTransient<ElectricitySupplierViewModel>();
        builder.Services.AddTransient<InverterViewModel>();
        builder.Services.AddTransient<FirstSyncViewModel>();
        builder.Services.AddTransient<EnergyCalculationParameterViewModel>();
        builder.Services.AddTransient<InvestmentAndLoanViewModel>();
        builder.Services.AddTransient<PowerTariffParameterViewModel>();

        //ROI
        builder.Services.AddTransient<RoiViewModel>();
        builder.Services.AddTransient<ReportViewModel>();
        //Energy
        builder.Services.AddTransient<EnergyViewModel>();
        //More
        builder.Services.AddTransient<MoreViewModel>();
        builder.Services.AddTransient<ParametersOverviewViewModel>();
        return builder;
    }
}

