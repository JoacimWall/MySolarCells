

namespace MySolarCells.ViewModels.OnBoarding
{
	public class ElectricitySupplierConnectViewModel : BaseViewModel
    {
        ITibberService tibberService;
        public ElectricitySupplierConnectViewModel(ITibberService tibberService)
		{
            this.tibberService = tibberService;
   
        }
        public ICommand GoToTibberApiTokenPageCommand => new Command(async () => await GoToTibberApiTokenPage());
        public ICommand GetHomesCommand => new Command(async () => await GetTibberHomes());
        public ICommand SaveHomeCommand => new Command(async () => await SaveHome());

        private async Task SaveHome()
        {
            using var dbContext = new MscDbContext();
            //check if Home exist in db
            var homeExist = await dbContext.Home.FirstOrDefaultAsync(x => x.SubSystemEntityId == selecteddHome.Id.ToString() && x.ElectricitySupplier == (int)ElectricitySupplier.Tibber);
            if (homeExist == null)
            {
                homeExist = new Services.Sqlite.Models.Home
                {
                    ApiKey = tibberAccessToken,
                    Name = selecteddHome.AppNickname,
                    SubSystemEntityId = selecteddHome.Id.ToString(),
                    ElectricitySupplier = (int)ElectricitySupplier.Tibber,
                    FromDate = new DateTime(installDate.Year, installDate.Month, installDate.Day)
                };
                //TODO:Do we neeed more info from tibber homes
                await dbContext.Home.AddAsync(homeExist);
                await dbContext.SaveChangesAsync();

                SettingsService.OnboardingStatus = OnboardingStatusEnum.ElectricitySupplierSelected;
                SettingsService.SelectedHomeId = homeExist.HomeId;
                MySolarCellsGlobals.SelectedHome = homeExist;
                await GoToAsync(nameof(InverterConnectView));
            }
            else
            {
                await DialogService.ShowAlertAsync("This house is already tracked", AppResources.My_Solar_Cells, AppResources.Ok);
            }
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
            set { SetProperty(ref selecteddHome, value);}
        }
        private DateTime installDate = new DateTime(DateTime.Now.Year,1,1);
        public DateTime InstallDate
        {
            get => installDate;
            set{ SetProperty(ref installDate, value);}
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
        
    }
}

