namespace MySolarCells.ViewModels.OnBoarding;

public class ElectricitySupplierViewModel : BaseViewModel
{
    IGridSupplierInterface gridSupplierService;
    private MscDbContext dbContext = new MscDbContext();
    public ElectricitySupplierViewModel()
    {
        GridSupplierModels.Add(new PickerItem { ItemTitle = ElectricitySupplier.Tibber.ToString(), ItemValue = (int)ElectricitySupplier.Tibber });
        GridSupplierModels.Add(new PickerItem { ItemTitle = ElectricitySupplier.Unknown.ToString(), ItemValue = (int)ElectricitySupplier.Unknown });
        using var dbContext = new MscDbContext();
        var home = dbContext.Home.FirstOrDefault();
        if (home == null)
        {
            selectedGridSupplierModel = GridSupplierModels.First();
        }
        else
        {
            foreach (var item in GridSupplierModels)
            {
                if (item.ItemValue == home.ElectricitySupplier)
                {
                    selectedGridSupplierModel = item;
                    break;
                }
            }

        }
        //if (MySolarCellsGlobals.SelectedHome != null && MySolarCellsGlobals.SelectedHome.HomeId != 0)
        //{
        //    TibberAccessToken = StringHelper.Decrypt(MySolarCellsGlobals.SelectedHome.ApiKey, AppConstants.Secretkey);
        //}

    }
    public ICommand GoToNavigateUrlCommand => new Command(async () => await GoToNavigateUrl());
    public ICommand TestConnectionCommand => new Command(async () => await TestConnection());
    public ICommand SaveCommand => new Command(async () => await SaveHome());


    private async Task TestConnection()
    {
        //this.gridSupplierService.Init(this.tibberAccessToken);
        var result = await this.gridSupplierService.TestConnection(this.userName, this.password, this.apiUrl, this.apiKey);

        if (!result.WasSuccessful)
        {
            await DialogService.ShowAlertAsync(result.ErrorMessage, AppResources.My_Solar_Cells, AppResources.Ok);
            
        }
        else
        {
            var resultHomes = await this.gridSupplierService.GetPickerOne();
            if (!resultHomes.WasSuccessful)
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
                SelecteddHome = Homes.First();
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

        //check if Home exist in db
        var homeExist = await dbContext.Home.FirstOrDefaultAsync(x => x.SubSystemEntityId == selecteddHome.SubSystemEntityId.ToString());
        if (homeExist == null)
        {
            homeExist = new Services.Sqlite.Models.Home();
            await dbContext.Home.AddAsync(homeExist);
        }

        homeExist.ImportOnlySpotPrice = importOnlySpotPrice;
        homeExist.ApiKey = StringHelper.Encrypt(selecteddHome.ApiKey, AppConstants.Secretkey);
        homeExist.Name = selecteddHome.Name;
        homeExist.SubSystemEntityId = selecteddHome.SubSystemEntityId.ToString();
        homeExist.ElectricitySupplier = selecteddHome.ElectricitySupplier;
        homeExist.FromDate = new DateTime(installDate.Year, installDate.Month, installDate.Day);

        //TODO:Do we neeed more info from tibber homes

        await dbContext.SaveChangesAsync();


        SettingsService.SelectedHomeId = homeExist.HomeId;
        MySolarCellsGlobals.SelectedHome = homeExist;
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
        await Launcher.OpenAsync(new Uri(this.navigationUrl));
    }

    private ObservableCollection<PickerItem> gridSupplierModels = new ObservableCollection<PickerItem>();
    public ObservableCollection<PickerItem> GridSupplierModels
    {
        get => gridSupplierModels;
        set { SetProperty(ref gridSupplierModels, value); }
    }
    private PickerItem selectedGridSupplierModel;
    public PickerItem SelectedGridSupplierModel
    {
        get => selectedGridSupplierModel;
        set
        {
            SetProperty(ref selectedGridSupplierModel, value);
            this.gridSupplierService = ServiceHelper.GetGridSupplierService(value.ItemValue);
            this.ShowUserName = this.gridSupplierService.ShowUserName;
            this.ShowPassword = this.gridSupplierService.ShowPassword;
            this.ShowApiUrl = this.gridSupplierService.ShowApiUrl;
            this.ShowApiKey = this.gridSupplierService.ShowApiKey;
            this.GuideText = this.gridSupplierService.GuideText;
            this.ApiUrl = this.gridSupplierService.DefaultApiUrl;
            this.showNavigateUrl = this.gridSupplierService.ShowNavigateUrl;
            this.navigationUrl = this.gridSupplierService.NavigationUrl;
        }
    }

    private ObservableCollection<Services.Sqlite.Models.Home> homes = new ObservableCollection<Services.Sqlite.Models.Home>();
    public ObservableCollection<Services.Sqlite.Models.Home> Homes
    {
        get => homes;
        set
        {
            SetProperty(ref homes, value);

        }
    }
    private Services.Sqlite.Models.Home selecteddHome;
    public Services.Sqlite.Models.Home SelecteddHome
    {
        get => selecteddHome;
        set { SetProperty(ref selecteddHome, value); }
    }

    private string guideText;
    public string GuideText
    {
        get => guideText;
        set { SetProperty(ref guideText, value); }
    }
    private string userName;
    public string UserName
    {
        get => userName;
        set { SetProperty(ref userName, value); }
    }
    private string password;
    public string Password
    {
        get => password;
        set { SetProperty(ref password, value); }
    }
    
    private string apiUrl;
    public string ApiUrl
    {
        get => apiUrl;
        set { SetProperty(ref apiUrl, value); }
    }
    private string navigationUrl;
    public string NavigationUrl
    {
        get => navigationUrl;
        set { SetProperty(ref navigationUrl, value); }
    }
    private string apiKey = "";
    public string ApiKey
    {
        get => apiKey;
        set { SetProperty(ref apiKey, value); }
    }
    private bool showUserName = false;
    public bool ShowUserName
    {
        get => showUserName;
        set { SetProperty(ref showUserName, value); }
    }
    private bool showPassword = false;
    public bool ShowPassword
    {
        get => showPassword;
        set { SetProperty(ref showPassword, value); }
    }
    private bool showApiUrl = false;
    public bool ShowApiUrl
    {
        get => showApiUrl;
        set { SetProperty(ref showApiUrl, value); }
    }
    private bool showApiKey = false;
    public bool ShowApiKey
    {
        get => showApiKey;
        set { SetProperty(ref showApiKey, value); }
    }
    private bool showNavigateUrl = false;
    public bool ShowNavigateUrl
    {
        get => showNavigateUrl;
        set { SetProperty(ref showNavigateUrl, value); }
    }
    
    private DateTime installDate = new DateTime(DateTime.Now.Year, 1, 1);
    public DateTime InstallDate
    {
        get => installDate;
        set { SetProperty(ref installDate, value); }
    }
    private string tibberAccessToken;
    public string TibberAccessToken
    {
        get => tibberAccessToken;
        set { SetProperty(ref tibberAccessToken, value); }
    }
    private bool showHomePicker;
    public bool ShowHomePicker
    {
        get => showHomePicker;
        set { SetProperty(ref showHomePicker, value); }
    }
    private bool importOnlySpotPrice;
    public bool ImportOnlySpotPrice
    {
        get => importOnlySpotPrice;
        set { SetProperty(ref importOnlySpotPrice, value); }
    }
}

