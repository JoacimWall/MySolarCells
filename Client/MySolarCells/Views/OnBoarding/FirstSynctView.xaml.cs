namespace MySolarCells.Views.OnBoarding;

public partial class FirstSyncView : BaseContentPage
{
    public FirstSyncView(FirstSyncViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
