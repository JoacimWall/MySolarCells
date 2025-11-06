namespace MySolarCells.Views;

public static class ViewsExtensions
{
    public static MauiAppBuilder ConfigureViews(this MauiAppBuilder builder)
    {

        //OnBoarding
        builder.Services.AddTransient<ElectricitySupplierView>();
        builder.Services.AddTransient<InverterView>();
        builder.Services.AddTransient<EnergyCalculationParameterView>();
        builder.Services.AddTransient<FirstSyncView>();
        builder.Services.AddTransient<InvestmentAndLoanView>();
        builder.Services.AddTransient<PowerTariffParameterView>();

        //ROI
        builder.Services.AddTransient<RoiView>();
        builder.Services.AddTransient<ReportView>();
        //Energy
        builder.Services.AddTransient<EnergyView>();
        //More
        builder.Services.AddTransient<MoreView>();
        builder.Services.AddTransient<SelectLanguageCountryView>();
        builder.Services.AddTransient<ParametersOverviewView>();
        builder.Services.AddTransient<CloudSyncSettingsView>();
        return builder;
    }
}

