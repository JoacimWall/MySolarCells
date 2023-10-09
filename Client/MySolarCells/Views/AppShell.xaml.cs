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

    }
}

