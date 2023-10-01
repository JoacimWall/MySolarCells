namespace MySolarCells.Views.OnBoarding;

public partial class InvestmentAndLoanView : BaseContentPage
{
	public InvestmentAndLoanView(InvestmentAndLoanViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }
}
