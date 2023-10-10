namespace MySolarCells.Views;

public static class ViewsExtensions
{
    public static MauiAppBuilder ConfigureViews(this MauiAppBuilder builder)
    {
        
        //Onoarding
        builder.Services.AddTransient<ElectricitySupplierView>();
        builder.Services.AddTransient<InverterView>();
        builder.Services.AddTransient<EnergyCalculationParameterView>();
        builder.Services.AddTransient<FirstSyncView>();
        builder.Services.AddTransient<InvestmentAndLoanView>();
        //ROI
        builder.Services.AddTransient<RoiView>();
        //Energy
        builder.Services.AddTransient<EnergyView>();
        //More
        builder.Services.AddTransient<MoreView>();
        return builder;
    }
}

