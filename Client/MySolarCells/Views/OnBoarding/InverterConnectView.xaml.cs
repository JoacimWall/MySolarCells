namespace MySolarCells.Views.OnBoarding;

public partial class InverterConnectView : BaseContentPage
{
	public InverterConnectView(InverterConnectViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }
}
