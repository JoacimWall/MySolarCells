using MySolarCells.Services.Sqlite;
using Plugin.LocalNotification;
using Plugin.LocalNotification.EventArgs;

namespace MySolarCells;

public partial class App : Application
{
    private readonly ISettingsService settingsService;
    private readonly MscDbContext mscDbContext;
    private readonly IDataSyncService dataSyncService;
    public App() //ISettingsService settingsService, IDataSyncService dataSyncService,MscDbContext mscDbContext
    {
        InitializeComponent();
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MjczNTI3NEAzMjMzMmUzMDJlMzBkR2xabUJjTnZ2Q3hQMnMrVVhobURpbDBMNzErbTMzYm15dmowZERRbUtFPQ==");
        //this.settingsService = settingsService;
        //this.dataSyncService = dataSyncService;
        //this.mscDbContext = mscDbContext;
        MySolarCellsGlobals.App = this;

        // Local Notification tap event listener
        //LocalNotificationCenter.Current.NotificationActionTapped += OnNotificationActionTapped;
        //LocalNotificationCenter.Current.NotificationReceived += Current_NotificationReceived;
        //LocalNotificationCenter.Current.No += Current_NotificationReceived;


        // MainPage = new StartupShell();
        MainPage = new ContentPage { BackgroundColor = Colors.MistyRose };

    }

    //private async void Current_NotificationReceived(NotificationEventArgs e)
    //{
    //    if (this.settingsService.OnboardingStatus != OnboardingStatusEnum.OnboardingDone)
    //        return;
    //    mscDbContext.Log.Add(new Services.Sqlite.Models.Log
    //    {
    //        LogTitle = "backgrund job started",
    //        CreateDate = DateTime.Now,
    //        LogDetails = "",
    //        LogTyp = (int)LogTyp.Info
    //    });
    //    await mscDbContext.SaveChangesAsync();
    //    var result = await this.dataSyncService.Sync().ConfigureAwait(false);
       
    //        mscDbContext.Log.Add(new Services.Sqlite.Models.Log
    //        {
    //            LogTitle = "backgrund sync job done.",
    //            CreateDate = DateTime.Now,
    //            LogDetails = "",
    //            LogTyp = result.WasSuccessful ? (int)LogTyp.Info : (int)LogTyp.Error
    //        });
    //    await mscDbContext.SaveChangesAsync();
    //    //create new
    //    //var notification = new NotificationRequest
    //    //{
    //    //    Silent = true,
    //    //    NotificationId = 100,
    //    //    Title = "Test",
    //    //    Description = "Test Description",
    //    //    ReturningData = "Dummy data", // Returning data when tapped on notification.
    //    //    Schedule =
    //    //            {
    //    //                NotifyTime = DateTime.Now.AddSeconds(30) // Used for Scheduling local notification, if not specified notification will show immediately.
    //    //            }
    //    //};
    //    //await LocalNotificationCenter.Current.Show(notification);

    //}

    //private void OnNotificationActionTapped(NotificationActionEventArgs e)
    //{
       
    //}

    private async Task<bool> initApp(bool fromResume)
    {
        //this.settingsService.OnboardingStatus = OnboardingStatusEnum.OnboardingDone;
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
            case OnboardingStatusEnum.EnergyCalculationparametersSelected:
                await Shell.Current.GoToAsync($"//{nameof(InvestmentAndLoanView)}");
                break;
            case OnboardingStatusEnum.InvestmentAndLoanDone:
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
        return;
        //then we don't want the Initnavigation to do the same thing. the app crash on IOS works on Android  
        //if (((Shell)(App.Current.MainPage)).CurrentPage is StartupView)
        await initApp(false);
        
        MySolarCellsGlobals.SelectedHome = this.mscDbContext.Home.FirstOrDefault(x => x.HomeId == this.settingsService.SelectedHomeId);
        MySolarCellsGlobals.ApplicationState = ApplicationState.Active;
       // await SyncData();
    }
    protected override async void OnResume()
    {
        //using var dbContext = new MscDbContext();
        MySolarCellsGlobals.SelectedHome = this.mscDbContext.Home.FirstOrDefault(x => x.HomeId == this.settingsService.SelectedHomeId);


        MySolarCellsGlobals.ApplicationState = ApplicationState.Active;
        // SyncData();

    }
    protected override async void OnSleep()
    {
       
        WeakReferenceMessenger.Default.Send(new AppSleepMessage(true));

      

        MySolarCellsGlobals.ApplicationState = ApplicationState.InActive;
    }
    //private async Task SyncData()
    //{
    //    var currentHour = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Hour);
    //    if (this.settingsService.LastDataSync < currentHour)
    //    {
    //        var result = await this.dataSyncService.Sync();

    //    }

    //}
    //private void SetupAppColors()
    //{
    //    Dictionary<TmColors, Color> colorConfig = new Dictionary<TmColors, Color>
    //    {
    //        //{ TmColors.Primary800Color, Color.FromArgb("#451C69") },
    //        //{ TmColors.Primary500Color, Color.FromArgb("#562483") },
    //        //{ TmColors.Primary400Color, Color.FromArgb("#9D81BB") },
    //        //{ TmColors.Primary200Color, Color.FromArgb("#F1EEF4") },
    //        //{ TmColors.Primary100Color, Color.FromArgb("#F8F6FA") },
    //        //{ TmColors.PrimaryAccentColor, Color.FromArgb("#F5E500") },

    //        //{ TmColors.SignalGreenColor, Color.FromArgb("#2BDB8C") },
    //        //{ TmColors.SignalYellowColor, Color.FromArgb("#F5E500") },
    //        //{ TmColors.SignalRedColor, Color.FromArgb("#FF675C") },
    //        //{ TmColors.SignalBlueColor, Color.FromArgb("#005EAB") },
    //        //{ TmColors.SignalOrangeColor, Color.FromArgb("#F4A528") },

    //        //Dessa är redan korrekt default
    //        //colorConfig.Add(TmtColors.BlackColor, Color.FromArgb("#000000"));
    //        //colorConfig.Add(TmtColors.WhiteColor, Color.FromArgb("#ffffff"));
    //        //colorConfig.Add(TmtColors.SuccessColor, Color.FromArgb("#3D9642"));
    //        //colorConfig.Add(TmtColors.TransparentColor, Color.FromRgba(0, 0, 0, 0));

    //        //colorConfig.Add(TmtColors.Gray900Color, Color.FromArgb("#1A1A1A"));
    //        //colorConfig.Add(TmtColors.Gray700Color, Color.FromArgb("#878787"));
    //        //colorConfig.Add(TmtColors.Gray400Color, Color.FromArgb("#BBBBBB"));
    //        //colorConfig.Add(TmtColors.Gray300Color, Color.FromArgb("#DEDEDE"));
    //        //colorConfig.Add(TmtColors.Gray200Color, Color.FromArgb("#EAEAEA"));
    //        //colorConfig.Add(TmtColors.Gray100Color, Color.FromArgb("#FAFAFA"));

    //        //Ska dessa ligga med ska de då heta detta
    //        //public static readonly Color Cyan500Color = Color.FromArgb("#19D5E5");
    //        //public static readonly Color Red500Color = Color.FromArgb("#E11D16");
    //        //public static readonly Color Gold500Color = Color.FromArgb("#BA9748");

    //        ////----------------  Controls colors check TietoEvryMauiStyles.xaml  ----------------------//
    //        //{ TmColors.TemplateTaskButtonTextColor, Color.FromArgb("#000000") },
    //        //{ TmColors.StatusbarBackgroundColor, Color.FromArgb("#562483") },
    //        //{ TmColors.StatusbarTextColor, Color.FromArgb("#ffffff") },
    //        //{ TmColors.PageBackgroundColor, Color.FromArgb("#ffffff") },
    //        //{ TmColors.DefaultTextColor, Color.FromArgb("#000000") },
    //        //{ TmColors.TmButtonValidBackgroundColor, Color.FromArgb("#562483") },
    //        //{ TmColors.TmButtonInValidBackgroundColor, Color.FromArgb("#F1EEF4") }
    //    };

    //    AppColors.Init(colorConfig);

    //}
}
