namespace MySolarCells.Views;

public partial class StartupShell : Shell
{
    public StartupShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(ElectricitySupplierView), typeof(ElectricitySupplierView));
        Routing.RegisterRoute(nameof(InverterView), typeof(InverterView));
        Routing.RegisterRoute(nameof(EnergyCalculationParameterView), typeof(EnergyCalculationParameterView));
        Routing.RegisterRoute(nameof(InvestmentAndLoanView), typeof(InvestmentAndLoanView));
        Routing.RegisterRoute(nameof(FirstSyncView), typeof(FirstSyncView));
    }
}
