namespace MySolarCells.ViewModels.OnBoarding;

public class InverterConnectViewModel : BaseViewModel
{

    IInverterServiceInterface InverterService;
    public InverterConnectViewModel()
    {

        InverterModels.Add(new PickerItem { ItemTitle = InverterTyp.Kostal.ToString(), ItemValue = (int)InverterTyp.Kostal });
        InverterModels.Add(new PickerItem { ItemTitle = InverterTyp.HomeAssistent.ToString(), ItemValue = (int)InverterTyp.HomeAssistent });
        InverterModels.Add(new PickerItem { ItemTitle = InverterTyp.Huawei.ToString(), ItemValue = (int)InverterTyp.Huawei });
        SelectedInverterModel = InverterModels.First();

    }

    public ICommand TestConnectionCommand => new Command(async () => await TestConnection());
    public ICommand SaveInverterCommand => new Command(async () => await SavAndMoveOn());
    private async Task TestConnection()
    {
        var resultLogin = await this.InverterService.TestConnection(this.userName, this.password,this.apiUrl,this.apiKey);
        if (!resultLogin.WasSuccessful)
        {
            await DialogService.ShowAlertAsync(resultLogin.ErrorMessage, AppResources.My_Solar_Cells, AppResources.Ok);
            return;
        }
        var resultSites = await this.InverterService.GetPickerOne();
        if (!resultSites.WasSuccessful)
        {
            await DialogService.ShowAlertAsync(resultSites.ErrorMessage, AppResources.My_Solar_Cells, AppResources.Ok);
            return;
        }
        SitesNodes = new ObservableCollection<PickerItem>(resultSites.Model);

        SelectedSiteNode = SitesNodes.First();
        ShowSiteNodePicker = true;
    }



    private async Task SavAndMoveOn()
    {

        //Get Inverter
        var resultInverter = await this.InverterService.GetInverter(selectedSiteNode);
        if (!resultInverter.WasSuccessful)
        {
            await DialogService.ShowAlertAsync(resultInverter.ErrorMessage, AppResources.My_Solar_Cells, AppResources.Ok);
            return;
        }

        using var dbContext = new MscDbContext();
        //check if Home exist in db
        var InverterExist = await dbContext.Inverter.FirstOrDefaultAsync(x => x.SubSystemEntityId == resultInverter.Model.ToString() && x.InverterTyp == SelectedInverterModel.ItemValue);
        if (InverterExist == null)
        {
            InverterExist = new Services.Sqlite.Models.Inverter
            {
                SubSystemEntityId = resultInverter.Model.InverterId,
                InverterTyp = (int)SelectedInverterModel.ItemValue,
                FromDate = new DateTime(installDate.Year, installDate.Month, installDate.Day),
                HomeId = MySolarCellsGlobals.SelectedHome.HomeId,
                Name = resultInverter.Model.Name,
                UserName = this.userName,
                Password = StringHelper.Encrypt(this.password, AppConstants.Secretkey)
            };
            //TODO:Do we neeed more info from Inverter
            await dbContext.Inverter.AddAsync(InverterExist);
            await dbContext.SaveChangesAsync();
        }
        else
        {
            await DialogService.ShowAlertAsync("This house is already tracked", AppResources.My_Solar_Cells, AppResources.Ok);
        }

        SettingsService.OnboardingStatus = OnboardingStatusEnum.InverterSelected;
        await GoToAsync(nameof(EnergyCalculationParameterView));
    }

    private ObservableCollection<PickerItem> sitesNodes = new ObservableCollection<PickerItem>();
    public ObservableCollection<PickerItem> SitesNodes
    {
        get => sitesNodes;
        set
        {
            SetProperty(ref sitesNodes, value);

        }
    }
    private PickerItem selectedSiteNode;
    public PickerItem SelectedSiteNode
    {
        get => selectedSiteNode;
        set { SetProperty(ref selectedSiteNode, value); }
    }
    private ObservableCollection<PickerItem> inverterModels = new ObservableCollection<PickerItem>();
    public ObservableCollection<PickerItem> InverterModels
    {
        get => inverterModels;
        set { SetProperty(ref inverterModels, value); }
    }
    private PickerItem selectedInverterModel;
    public PickerItem SelectedInverterModel
    {
        get => selectedInverterModel;
        set
        {
            SetProperty(ref selectedInverterModel, value);
            this.InverterService = ServiceHelper.GetInverterService(value.ItemValue);
            this.ShowUserName = this.InverterService.ShowUserName;
            this.ShowPassword = this.InverterService.ShowPassword;
            this.ShowApiUrl = this.InverterService.ShowApiUrl;
            this.ShowApiKey = this.InverterService.ShowApiKey;
            this.InverterGuideText = this.InverterService.InverterGuideText;
            this.ApiUrl = this.InverterService.DefaultApiUrl;

        }
    }
    
    private string inverterGuideText;
    public string InverterGuideText
    {
        get => inverterGuideText;
        set { SetProperty(ref inverterGuideText, value); }
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
    private DateTime installDate = DateTime.Now;
    public DateTime InstallDate
    {
        get => installDate;
        set { SetProperty(ref installDate, value); }
    }
    private string apiUrl;
    public string ApiUrl
    {
        get => apiUrl;
        set { SetProperty(ref apiUrl, value); }
    }
    private string apiKey= "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiIyNzZjN2E3ZmY5OWU0MGI2OGNhZDE1NmM2YTgzMDI4OCIsImlhdCI6MTY5NjM5NjI1NiwiZXhwIjoyMDExNzU2MjU2fQ.o_JFjiPNdPyJl0YtBmY5fKJO_A6ms3Gs40jDfZG9ofY";
    public string ApiKey
    {
        get => apiKey;
        set { SetProperty(ref apiKey, value); }
    }



    private bool showSiteNodePicker;
    public bool ShowSiteNodePicker
    {
        get => showSiteNodePicker;
        set { SetProperty(ref showSiteNodePicker, value); }
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
}

