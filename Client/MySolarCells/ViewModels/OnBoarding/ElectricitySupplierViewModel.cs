namespace MySolarCells.ViewModels.OnBoarding;

public class ElectricitySupplierViewModel : BaseViewModel
{
    ITibberService tibberService;
    private MscDbContext dbContext = new MscDbContext();
    public ElectricitySupplierViewModel(ITibberService tibberService)
    {
        this.tibberService = tibberService;
        if (MySolarCellsGlobals.SelectedHome != null && MySolarCellsGlobals.SelectedHome.HomeId != 0)
        {
            TibberAccessToken = StringHelper.Decrypt(MySolarCellsGlobals.SelectedHome.ApiKey, AppConstants.Secretkey);
        }

    }
    public ICommand GoToTibberApiTokenPageCommand => new Command(async () => await GoToTibberApiTokenPage());
    public ICommand GetHomesCommand => new Command(async () => await GetTibberHomes());
    public ICommand SaveCommand => new Command(async () => await SaveHome());

    private async Task SaveHome()
    {

        //check if Home exist in db
        var homeExist = await dbContext.Home.FirstOrDefaultAsync(x => x.SubSystemEntityId == selecteddHome.Id.ToString() && x.ElectricitySupplier == (int)ElectricitySupplier.Tibber);
        if (homeExist == null)
        {
            homeExist = new Services.Sqlite.Models.Home();
            await dbContext.Home.AddAsync(homeExist);
        }

        homeExist.ImportOnlySpotPrice = importOnlySpotPrice;
        homeExist.ApiKey = StringHelper.Encrypt(tibberAccessToken, AppConstants.Secretkey);
        homeExist.Name = selecteddHome.AppNickname;
        homeExist.SubSystemEntityId = selecteddHome.Id.ToString();
        homeExist.ElectricitySupplier = (int)ElectricitySupplier.Tibber;
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

    private async Task GetTibberHomes()
    {
        this.tibberService.Init(this.tibberAccessToken);
        var result = await this.tibberService.GetBasicData();

        if (result.WasSuccessful)
        {
            foreach (var item in result.Model)
            {
                //Homes.Add(new PickerItem { ItemTitle = item.Name, ItemValue = item.HomeId, IsDefaultValue = isfirst });
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
        else
        {
            await DialogService.ShowAlertAsync(result.ErrorMessage, AppResources.My_Solar_Cells, AppResources.Ok);

        }

    }

    private async Task GoToTibberApiTokenPage()
    {
        await Launcher.OpenAsync(new Uri("https://developer.tibber.com/settings/access-token"));
    }


    private ObservableCollection<Tibber.Sdk.Home> homes = new ObservableCollection<Tibber.Sdk.Home>();
    public ObservableCollection<Tibber.Sdk.Home> Homes
    {
        get => homes;
        set
        {
            SetProperty(ref homes, value);

        }
    }
    private Tibber.Sdk.Home selecteddHome;
    public Tibber.Sdk.Home SelecteddHome
    {
        get => selecteddHome;
        set { SetProperty(ref selecteddHome, value); }
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

