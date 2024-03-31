using MySolarCellsSQLite.Sqlite;
using MySolarCellsSQLite.Sqlite.Models;

namespace MySolarCells.ViewModels.OnBoarding;

public class ElectricitySupplierViewModel : BaseViewModel
{
    IGridSupplierInterface? gridSupplierService;
    private readonly MscDbContext mscDbContext;
    public ElectricitySupplierViewModel(MscDbContext mscDbContext,IDialogService dialogService,
        IAnalyticsService analyticsService, IInternetConnectionService internetConnectionService, ILogService logService,ISettingsService settingsService,IHomeService homeService): base(dialogService, analyticsService, internetConnectionService,
        logService,settingsService,homeService)
    {
        GridSupplierModels.Add(new PickerItem { ItemTitle = ElectricitySupplierEnum.Tibber.ToString(), ItemValue = (int)ElectricitySupplierEnum.Tibber });
        GridSupplierModels.Add(new PickerItem { ItemTitle = ElectricitySupplierEnum.Unknown.ToString(), ItemValue = (int)ElectricitySupplierEnum.Unknown });
        this.mscDbContext = mscDbContext;
        var home = this.mscDbContext.ElectricitySupplier.FirstOrDefault();
        if (home == null)
        {
            selectedGridSupplierModel = GridSupplierModels.First();
        }   
        else
        {
            foreach (var item in GridSupplierModels)
            {
                if (item.ItemValue == home.ElectricitySupplierType)
                {
                    selectedGridSupplierModel = item;
                    break;
                }
            }
            SelectedHome = home;
            ApiKey = selectedHome.ApiKey.Decrypt(AppConstants.Secretkey);
            //TestConnection();
        }

        for (int i = DateTime.Now.Year; i > DateTime.Now.AddYears(-20).Year; i--)
            InstallYears.Add(new PickerItem { ItemTitle = i.ToString(), ItemValue = i });


       



    }
    public ICommand GoToNavigateUrlCommand => new Command(async () => await GoToNavigateUrl());
    public ICommand TestConnectionCommand => new Command(async () => await TestConnection());
    public ICommand SaveCommand => new Command(async () => await SaveHome());
    
    private async Task TestConnection()
    {
        if (gridSupplierService == null)
            return;
        var result = await gridSupplierService.TestConnection(userName, password, apiUrl, apiKey);

        if (!result.WasSuccessful)
        {
            await DialogService.ShowAlertAsync(result.ErrorMessage, AppResources.My_Solar_Cells, AppResources.Ok);
            
        }
        else
        {
            var resultHomes = await gridSupplierService.GetPickerOne();
            if (!resultHomes.WasSuccessful || resultHomes.Model == null)
            {
                await DialogService.ShowAlertAsync(resultHomes.ErrorMessage, AppResources.My_Solar_Cells, AppResources.Ok);
                return;
            }

            foreach (var item in resultHomes.Model)
            {
                Homes.Add(item);

            }
            if (Homes.Count > 0)
            {
                SelectedHome = Homes.First();
                InstallDate = InstallYears.First();
                ShowHomePicker = true;
            }
            else
            {
                ShowHomePicker = false;
            }

        }

    }

    private async Task SaveHome()
    {
        //check that home exist
        
        var homeExist = await mscDbContext.Home.FirstOrDefaultAsync();
        if (homeExist == null)
        {
            homeExist = HomeService.CurrentHome();
            await mscDbContext.Home.AddAsync(homeExist);
            await mscDbContext.SaveChangesAsync();
        }

        //check if Home exist in db
        var electricitySupplierExist = await mscDbContext.ElectricitySupplier.FirstOrDefaultAsync(x => x.SubSystemEntityId == selectedHome.SubSystemEntityId);
        if (electricitySupplierExist == null)
        {
            electricitySupplierExist =  new(){ Name = "", SubSystemEntityId = "", HomeId = homeExist.HomeId};
            await mscDbContext.ElectricitySupplier.AddAsync(electricitySupplierExist);
        }

        electricitySupplierExist.ImportOnlySpotPrice = importOnlySpotPrice;
        electricitySupplierExist.ApiKey = selectedHome.ApiKey.Encrypt(AppConstants.Secretkey);
        electricitySupplierExist.Name = selectedHome.Name;
        electricitySupplierExist.SubSystemEntityId = selectedHome.SubSystemEntityId;
        electricitySupplierExist.ElectricitySupplierType = selectedHome.ElectricitySupplierType;
        electricitySupplierExist.FromDate = new DateTime(installDate.ItemValue, 1, 1);

        //TODO:Do we need more info from tibber homes

        await mscDbContext.SaveChangesAsync();


        SettingsService.SelectedHomeId = electricitySupplierExist.ElectricitySupplierId;
        HomeService.ResetCurrenHome();
        if (SettingsService.OnboardingStatus == OnboardingStatusEnum.OnboardingDone)
        {
            await GoBack();
        }
        else
        {
            SettingsService.OnboardingStatus = OnboardingStatusEnum.ElectricitySupplierSelected;
            await GoToAsync(nameof(InverterView));
        }

        //}
        //else
        //{
        //    await DialogService.ShowAlertAsync("This house is already tracked", AppResources.My_Solar_Cells, AppResources.Ok);
        //}
    }

    

    private async Task GoToNavigateUrl()
    {
        await Launcher.OpenAsync(new Uri(navigationUrl));
    }

    private ObservableCollection<PickerItem> gridSupplierModels = new ObservableCollection<PickerItem>();
    public ObservableCollection<PickerItem> GridSupplierModels
    {
        get => gridSupplierModels;
        set => SetProperty(ref gridSupplierModels, value);
    }
    private PickerItem selectedGridSupplierModel= new();
    public PickerItem SelectedGridSupplierModel
    {
        get => selectedGridSupplierModel;
        set
        {
            SetProperty(ref selectedGridSupplierModel, value);
            gridSupplierService = ServiceHelper.GetGridSupplierService(value.ItemValue);
            ShowUserName = gridSupplierService.ShowUserName;
            ShowPassword = gridSupplierService.ShowPassword;
            ShowApiUrl = gridSupplierService.ShowApiUrl;
            ShowApiKey = gridSupplierService.ShowApiKey;
            GuideText = gridSupplierService.GuideText;
            ApiUrl = gridSupplierService.DefaultApiUrl;
            showNavigateUrl = gridSupplierService.ShowNavigateUrl;
            navigationUrl = gridSupplierService.NavigationUrl;
        }
    }

    private ObservableCollection<ElectricitySupplier> homes = new ObservableCollection<ElectricitySupplier>();
    public ObservableCollection<ElectricitySupplier> Homes
    {
        get => homes;
        set => SetProperty(ref homes, value);
    }
    private ElectricitySupplier selectedHome = new(){ Name = "", SubSystemEntityId = ""};
    public ElectricitySupplier SelectedHome
    {
        get => selectedHome;
        set => SetProperty(ref selectedHome, value);
    }
    
    private string guideText = "";
    public string GuideText
    {
        get => guideText;
        set => SetProperty(ref guideText, value);
    }
    private string userName="";
    public string UserName
    {
        get => userName;
        set => SetProperty(ref userName, value);
    }
    private string password="";
    public string Password
    {
        get => password;
        set => SetProperty(ref password, value);
    }
    
    private string apiUrl="";
    public string ApiUrl
    {
        get => apiUrl;
        set => SetProperty(ref apiUrl, value);
    }
    private string navigationUrl="";
    public string NavigationUrl
    {
        get => navigationUrl;
        set => SetProperty(ref navigationUrl, value);
    }
    private string apiKey = "";
    public string ApiKey
    {
        get => apiKey;
        set => SetProperty(ref apiKey, value);
    }
    private bool showUserName;
    public bool ShowUserName
    {
        get => showUserName;
        set => SetProperty(ref showUserName, value);
    }
    private bool showPassword;
    public bool ShowPassword
    {
        get => showPassword;
        set => SetProperty(ref showPassword, value);
    }
    private bool showApiUrl;
    public bool ShowApiUrl
    {
        get => showApiUrl;
        set => SetProperty(ref showApiUrl, value);
    }
    private bool showApiKey;
    public bool ShowApiKey
    {
        get => showApiKey;
        set => SetProperty(ref showApiKey, value);
    }
    private bool showNavigateUrl;
    public bool ShowNavigateUrl
    {
        get => showNavigateUrl;
        set => SetProperty(ref showNavigateUrl, value);
    }
    
    private ObservableCollection<PickerItem> installYears = new ObservableCollection<PickerItem>();
    public ObservableCollection<PickerItem> InstallYears
    {
        get => installYears;
        set => SetProperty(ref installYears, value);
    }
    private PickerItem installDate=new();
    public PickerItem InstallDate
    {
        get => installDate;
        set => SetProperty(ref installDate, value);
    }
    private string tibberAccessToken="";
    public string TibberAccessToken
    {
        get => tibberAccessToken;
        set => SetProperty(ref tibberAccessToken, value);
    }
    private bool showHomePicker;
    public bool ShowHomePicker
    {
        get => showHomePicker;
        set => SetProperty(ref showHomePicker, value);
    }
    private bool importOnlySpotPrice;
    public bool ImportOnlySpotPrice
    {
        get => importOnlySpotPrice;
        set => SetProperty(ref importOnlySpotPrice, value);
    }
}

