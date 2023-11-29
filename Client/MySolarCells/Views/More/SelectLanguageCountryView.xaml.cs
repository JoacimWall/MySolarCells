namespace MySolarCells.Views.More;

public partial class SelectLanguageCountryView : BaseContentPage
{
	public SelectLanguageCountryView(SelectLanguageCountryViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }
}
