namespace MySolarCells.Views.OnBoarding;

public partial class PowerTariffParameterView : BaseContentPage
{
    public PowerTariffParameterView(PowerTariffParameterViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }


}
