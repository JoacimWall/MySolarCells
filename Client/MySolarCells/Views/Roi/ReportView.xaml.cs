namespace MySolarCells.Views.Roi;

public partial class ReportView : BaseContentPage
{
    //private ISetOrientationService setOrientationService;
    public ReportView(ReportViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        // setOrientationService = ServiceHelper.GetService<ISetOrientationService>();

    }
    /*protected override void OnAppearing()
	{
		//TODO: MAUI
		setOrientationService.OnlyLandscape();

		base.OnAppearing();

      
	}*/

    /*protected async override void OnDisappearing()
	{
		
		setOrientationService.OnlyPortrait();

		await Task.Delay(250);
		base.OnDisappearing();

       
	}*/
}
