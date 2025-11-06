namespace MySolarCells.Views.More;

public partial class ParametersOverviewView : BaseContentPage
{
    public ParametersOverviewView(ParametersOverviewViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
