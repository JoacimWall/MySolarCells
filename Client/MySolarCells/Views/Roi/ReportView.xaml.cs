namespace MySolarCells.Views.Roi;

public partial class ReportView : BaseContentPage
{
	public ReportView(ReportViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
	}
}
