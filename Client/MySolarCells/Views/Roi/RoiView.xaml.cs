namespace MySolarCells.Views.Roi;

public partial class RoiView : BaseContentPage
{
    public RoiView(RoiViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
