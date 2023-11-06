namespace MySolarCells.Views.Energy;

public partial class EnergyView : BaseContentPage
{
	public EnergyView(EnergyViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
		//this.StatusbarBackgroundColor = AppColors.Primary800Color;
    }

    void RefreshView_Refreshing(System.Object sender, System.EventArgs e)
    {
    }
}
