namespace MySolarCells.Views.OnBoarding;

public partial class EnergyCalculationParameterView : BaseContentPage
{
	public EnergyCalculationParameterView(EnergyCalculationParameterViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }

   
}
