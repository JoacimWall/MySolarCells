namespace MySolarCells.Views.OnBoarding;

public partial class ElectricitySupplierConnectView : BaseContentPage
{
	public ElectricitySupplierConnectView(ElectricitySupplierConnectViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }
}
