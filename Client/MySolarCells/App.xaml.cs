using MySolarCellsSQLite.Sqlite;
using MySolarCellsSQLite.Sqlite.Models;

namespace MySolarCells;

public partial class App : Application
{
    private readonly ISettingsService settingsService;
    private readonly MscDbContext mscDbContext;
    public App(ISettingsService settingsService, MscDbContext mscDbContext) 
    {
        InitializeComponent();
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mzc4NTkxOEAzMjM5MmUzMDJlMzAzYjMyMzkzYm1kK3BWV2EySTJldjNXNjBSSktxRW90NGU0Y3NJZjJJK0d5SFFweC9ldEk9");
        this.settingsService = settingsService;
        this.mscDbContext = mscDbContext;
        MySolarCellsGlobals.App = this;
        this.settingsService.SetCurrentCultureOnAllThreads(this.settingsService.UserCountry); 
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
       // await SyncData();
    }
    protected override void OnResume()
    {
        //using var dbContext = new MscDbContext();
        //MySolarCellsGlobals.SelectedHome = mscDbContext.ElectricitySupplier.FirstOrDefault(x => x.ElectricitySupplierId == settingsService.SelectedHomeId) ?? new ElectricitySupplier(){ Name = "", SubSystemEntityId = ""};


        MySolarCellsGlobals.ApplicationState = ApplicationState.Active;
        // SyncData();

    }
    protected override void OnSleep()
    {
       
        WeakReferenceMessenger.Default.Send(new AppSleepMessage(true));

      

        MySolarCellsGlobals.ApplicationState = ApplicationState.InActive;
    }
   
}
