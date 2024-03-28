namespace MySolarCells.Views.Energy;

public partial class EnergyView : BaseContentPage
{
	public EnergyView(EnergyViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }

}
