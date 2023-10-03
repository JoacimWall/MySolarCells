namespace MySolarCells;

public partial class App : Application
{
    private readonly ISettingsService settingsService;
    public App(ISettingsService settingsService)
    {
        InitializeComponent();
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MjczNTI3NEAzMjMzMmUzMDJlMzBkR2xabUJjTnZ2Q3hQMnMrVVhobURpbDBMNzErbTMzYm15dmowZERRbUtFPQ==");
        this.settingsService = settingsService;
        MainPage = new StartupShell();


    }
    private async Task<bool> initApp(bool fromResume)
    {
        //this.settingsService.OnboardingStatus = OnboardingStatusEnum.EnergyCalculationparametersSelected;
        switch (settingsService.OnboardingStatus)
        {
            case OnboardingStatusEnum.Unknown:
                await Shell.Current.GoToAsync($"//{nameof(ElectricitySupplierConnectView)}");
                break;
            case OnboardingStatusEnum.ElectricitySupplierSelected:
                await Shell.Current.GoToAsync($"//{nameof(InverterConnectView)}");
                break;
            case OnboardingStatusEnum.InverterSelected:
                await Shell.Current.GoToAsync($"//{nameof(EnergyCalculationParameterView)}");
                break;
            case OnboardingStatusEnum.EnergyCalculationparametersSelected:
            case OnboardingStatusEnum.FirstImportElectricitySupplierIsDone:
                await Shell.Current.GoToAsync($"//{nameof(FirstSyncView)}");
                break;
            default:
                if (!fromResume)
                    App.Current.MainPage = new AppShell();//
                break;
        }
       
        return true;
    }
    protected async override void OnStart()
    {
        //then we don't want the Initnavigation to do the same thing. the app crash on IOS works on Android  
        //if (((Shell)(App.Current.MainPage)).CurrentPage is StartupView)
        await initApp(false);
        using var dbContext = new MscDbContext();
        MySolarCellsGlobals.SelectedHome = dbContext.Home.FirstOrDefault(x => x.HomeId == this.settingsService.SelectedHomeId);
        MySolarCellsGlobals.ApplicationState = ApplicationState.Active;
    }
    protected override async void OnResume()
    {
        using var dbContext = new MscDbContext();
        MySolarCellsGlobals.SelectedHome = dbContext.Home.FirstOrDefault(x => x.HomeId == this.settingsService.SelectedHomeId);


        MySolarCellsGlobals.ApplicationState = ApplicationState.Active;

    }
    protected override async void OnSleep()
    {
       
        WeakReferenceMessenger.Default.Send(new AppSleepMessage(true));

      

        MySolarCellsGlobals.ApplicationState = ApplicationState.InActive;
    }
}
