using Syncfusion.XlsIO;

namespace MySolarCells.ViewModels.More;

public class SelectLanguageCountryViewModel : BaseViewModel
{
    private readonly ISettingsService settingsService;
    public SelectLanguageCountryViewModel(ISettingsService settingsService)
    {
        this.settingsService = settingsService;
       

    }

    public ICommand SelectEngCommand => new Command(async () => await SelectEng());
    public ICommand SelectSweCommand => new Command(async () => await SelectSwe());
    private async Task SelectEng()
    {
        if (this.settingsService.UserCountry != CountryEnum.En_US)
        {
            this.settingsService.UserCountry = CountryEnum.En_US;
            App.Current.MainPage = new AppShell();
        }
        else
        {
            await GoBack();
        }

    }
    private async Task SelectSwe()
    {
        if (this.settingsService.UserCountry != CountryEnum.Sv_SE)
        {
            this.settingsService.UserCountry = CountryEnum.Sv_SE;
            App.Current.MainPage = new AppShell();
        }
        else
        {
            await GoBack();
        }
    }

   
}

