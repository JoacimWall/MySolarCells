namespace MySolarCells.ViewModels.More;

public class SelectLanguageCountryViewModel : BaseViewModel
{
   
    public SelectLanguageCountryViewModel(IDialogService dialogService,
        IAnalyticsService analyticsService, IInternetConnectionHelper internetConnectionHelper, ILogService logService,ISettingsService settingsService): base(dialogService, analyticsService, internetConnectionHelper,
        logService,settingsService)
    {
       

    }

    public ICommand SelectEngCommand => new Command(async () => await SelectEng());
    public ICommand SelectSweCommand => new Command(async () => await SelectSwe());
    private async Task SelectEng()
    {
        if (SettingsService.UserCountry != CountryEnum.EnUs)
        {
            SettingsService.UserCountry = CountryEnum.EnUs;
            if (Application.Current != null) Application.Current.MainPage = new AppShell();
        }
        else
        {
            await GoBack();
        }

    }
    private async Task SelectSwe()
    {
        if (SettingsService.UserCountry != CountryEnum.SvSe)
        {
            SettingsService.UserCountry = CountryEnum.SvSe;
            if (Application.Current != null) Application.Current.MainPage = new AppShell();
        }
        else
        {
            await GoBack();
        }
    }

   
}

