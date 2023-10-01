namespace MySolarCells.Views;

public partial class StartupShell : Shell
{
	public StartupShell()
	{
		InitializeComponent();
        Routing.RegisterRoute(nameof(ElectricitySupplierConnectView), typeof(ElectricitySupplierConnectView));
        Routing.RegisterRoute(nameof(InverterConnectView), typeof(InverterConnectView));
        Routing.RegisterRoute(nameof(EnergyCalculationParameterView), typeof(EnergyCalculationParameterView));
        Routing.RegisterRoute(nameof(InvestmentAndLoanView), typeof(InvestmentAndLoanView));
        Routing.RegisterRoute(nameof(FirstSyncView), typeof(FirstSyncView));
    }
}
