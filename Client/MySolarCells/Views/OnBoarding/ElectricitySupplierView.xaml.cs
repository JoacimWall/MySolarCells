namespace MySolarCells.Views.OnBoarding;

public partial class ElectricitySupplierView : BaseContentPage
{
	public ElectricitySupplierView(ElectricitySupplierViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }
}
