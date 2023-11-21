namespace MySolarCells.Views;

public partial class AppShell : Shell
{
	public AppShell()
	{
        InitializeComponent();
        RegisterRoutes();
    }
    private void RegisterRoutes()
    {
        //Onboarding
        Routing.RegisterRoute(nameof(InvestmentAndLoanView), typeof(InvestmentAndLoanView));
        Routing.RegisterRoute(nameof(EnergyCalculationParameterView), typeof(EnergyCalculationParameterView));
        Routing.RegisterRoute(nameof(ElectricitySupplierView), typeof(ElectricitySupplierView));
        Routing.RegisterRoute(nameof(InverterView), typeof(InverterView));
        Routing.RegisterRoute(nameof(ParametersOverviewView), typeof(ParametersOverviewView));
        Routing.RegisterRoute(nameof(PowerTariffParameterView), typeof(PowerTariffParameterView));

    }
}

