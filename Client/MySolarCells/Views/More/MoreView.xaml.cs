namespace MySolarCells.Views.More;

public partial class MoreView : BaseContentPage
{
	public MoreView(MoreViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }
}
