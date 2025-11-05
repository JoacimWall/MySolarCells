using MySolarCellsSQLite.Sqlite;
using MySolarCellsSQLite.Sqlite.Models;

namespace MySolarCells;

public partial class App : Application
{
    private readonly ISettingsService settingsService;
    private readonly MscDbContext mscDbContext;
    private readonly IBackgroundSyncService backgroundSyncService;
    private readonly IThemeService themeService;

    public App(ISettingsService settingsService, MscDbContext mscDbContext, IBackgroundSyncService backgroundSyncService, IThemeService themeService)
    {
        InitializeComponent();
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JFaF5cXGRCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWH5edXVRRGlcWEJ1W0dWYEg=");
        this.settingsService = settingsService;
        this.mscDbContext = mscDbContext;
        this.backgroundSyncService = backgroundSyncService;
        this.themeService = themeService;
        MySolarCellsGlobals.App = this;
        this.settingsService.SetCurrentCultureOnAllThreads(this.settingsService.UserCountry);

        // Initialize theme from user preference
        themeService.ApplyTheme(settingsService.UserTheme);

        MainPage = new StartupShell();

    }

   

    private async Task<bool> InitApp(bool fromResume)
    {
        if (settingsService.OnboardingStatus >= OnboardingStatusEnum.FirstImportElectricitySupplierIsDone)
        {
            if (!mscDbContext.Energy.Any())
            {
                settingsService.OnboardingStatus = OnboardingStatusEnum.InvestmentAndLoanDone;
            }
        }

        switch (settingsService.OnboardingStatus)
        {
            case OnboardingStatusEnum.Unknown:
                await Shell.Current.GoToAsync($"//{nameof(ElectricitySupplierView)}");
                break;
            case OnboardingStatusEnum.ElectricitySupplierSelected:
                await Shell.Current.GoToAsync($"//{nameof(InverterView)}");
                break;
            case OnboardingStatusEnum.InverterSelected:
                await Shell.Current.GoToAsync($"//{nameof(EnergyCalculationParameterView)}");
                break;
            case OnboardingStatusEnum.EnergyCalculationParameterSelected:
                await Shell.Current.GoToAsync($"//{nameof(InvestmentAndLoanView)}");
                break;
            case OnboardingStatusEnum.InvestmentAndLoanDone:
            case OnboardingStatusEnum.FirstImportElectricitySupplierIsDone:
                await Shell.Current.GoToAsync($"//{nameof(FirstSyncView)}");
                break;
            default:
                if (!fromResume)
                    if (Current != null)
                        Current.MainPage = new AppShell(); //
                break;
        }

      
           
        return true;
    }
    protected override async void OnStart()
    {

        //then we don't want the Init navigation to do the same thing. the app crash on IOS works on Android
        //if (((Shell)(App.Current.MainPage)).CurrentPage is StartupView)
        await InitApp(false);

        // MySolarCellsGlobals.SelectedHome = mscDbContext.ElectricitySupplier.FirstOrDefault(x => x.ElectricitySupplierId == settingsService.SelectedHomeId) ?? new ElectricitySupplier(){ Name = "", SubSystemEntityId = ""};
        MySolarCellsGlobals.ApplicationState = ApplicationState.Active;

        // Start background sync service
        backgroundSyncService.StartBackgroundSync();
    }
    protected override void OnResume()
    {
        //using var dbContext = new MscDbContext();
        //MySolarCellsGlobals.SelectedHome = mscDbContext.ElectricitySupplier.FirstOrDefault(x => x.ElectricitySupplierId == settingsService.SelectedHomeId) ?? new ElectricitySupplier(){ Name = "", SubSystemEntityId = ""};


        MySolarCellsGlobals.ApplicationState = ApplicationState.Active;

        // Resume background sync service
        backgroundSyncService.StartBackgroundSync();
    }
    protected override void OnSleep()
    {

        WeakReferenceMessenger.Default.Send(new AppSleepMessage(true));

        // Stop background sync service
        backgroundSyncService.StopBackgroundSync();

        MySolarCellsGlobals.ApplicationState = ApplicationState.InActive;
    }
   
}
