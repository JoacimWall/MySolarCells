namespace MySolarCells.Views.OnBoarding;

public partial class InverterView : BaseContentPage
{
    public InverterView(InverterViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
