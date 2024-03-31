namespace MySolarCells.ViewModels.OnBoarding;

public class InverterViewModel : BaseViewModel
{
    private InverterLoginResponse? inverterLoginResponse;
    private readonly MscDbContext mscDbContext;
    IInverterServiceInterface? inverterService;
    public InverterViewModel(MscDbContext mscDbContext,IDialogService dialogService,
        IAnalyticsService analyticsService, IInternetConnectionService internetConnectionService, ILogService logService,ISettingsService settingsService,IHomeService homeService): base(dialogService, analyticsService, internetConnectionService,
        logService,settingsService, homeService)
    {
        this.mscDbContext = mscDbContext;
        InverterModels.Add(new PickerItem { ItemTitle = InverterTypeEnum.HomeAssistent.ToString(), ItemValue = (int)InverterTypeEnum.HomeAssistent });
        InverterModels.Add(new PickerItem { ItemTitle = InverterTypeEnum.Huawei.ToString(), ItemValue = (int)InverterTypeEnum.Huawei });
        InverterModels.Add(new PickerItem { ItemTitle = InverterTypeEnum.SolarEdge.ToString(), ItemValue = (int)InverterTypeEnum.SolarEdge });
       
        var inverter = this.mscDbContext.Inverter.OrderByDescending(s => s.FromDate).FirstOrDefault();
        if (inverter == null)
        {
            SelectedInverterModel = InverterModels.First();
        }
        else
        {
            foreach (var item in InverterModels)
            {
                if (item.ItemValue == inverter.InverterTyp)
                {
                    SelectedInverterModel = item;
                    break;
                }
            }

            UserName = inverter.UserName ?? "Inverter";
            Password = string.IsNullOrEmpty(inverter.Password) ? "" : inverter.Password.Decrypt(AppConstants.Secretkey); 
            ApiKey = string.IsNullOrEmpty(inverter.ApiKey) ? "" : inverter.ApiKey.Decrypt(AppConstants.Secretkey); 
        }
    }

    public ICommand TestConnectionCommand => new Command(async () => await TestConnection());
    public ICommand SaveCommand => new Command(async () => await SaveInverter());
    private async Task TestConnection()
    {
        if (inverterService == null)
            return;
        
        var resultLogin = await inverterService.TestConnection(userName, password,apiUrl,apiKey);
        if (!resultLogin.WasSuccessful || resultLogin.Model == null)
        {
            await DialogService.ShowAlertAsync(resultLogin.ErrorMessage, AppResources.My_Solar_Cells, AppResources.Ok);
            return;
        }
        inverterLoginResponse = resultLogin.Model;
        var resultSites = await inverterService.GetPickerOne();
        if (!resultSites.WasSuccessful || resultSites.Model == null)
        {
            await DialogService.ShowAlertAsync(resultSites.ErrorMessage, AppResources.My_Solar_Cells, AppResources.Ok);
            return;
        }
        SitesNodes = new ObservableCollection<InverterSite>(resultSites.Model);
        
        SelectedSiteNode = SitesNodes.First();
        ShowSiteNodePicker = true;
    }



    private async Task SaveInverter()
    {
        if (inverterService == null || selectedSiteNode == null || SelectedInverterModel == null)
            return;
        //Get Inverter
        var resultInverter = await inverterService.GetInverter(selectedSiteNode);
        if (!resultInverter.WasSuccessful || resultInverter.Model == null)
        {
            await DialogService.ShowAlertAsync(resultInverter.ErrorMessage, AppResources.My_Solar_Cells, AppResources.Ok);
            return;
        }

        
        //check if Home exist in db
        var inverterExist = await mscDbContext.Inverter.FirstOrDefaultAsync(x => x.SubSystemEntityId == resultInverter.Model.InverterId && x.InverterTyp == SelectedInverterModel.ItemValue);
        if (inverterExist == null)
        {
            string token = "";
            if (inverterLoginResponse != null)
            {
                token = inverterLoginResponse.token;
            }
            inverterExist = new Inverter
            {
                SubSystemEntityId = resultInverter.Model.InverterId,
                InverterTyp = SelectedInverterModel.ItemValue,
                FromDate = new DateTime(installDate.Year, installDate.Month, installDate.Day),
                HomeId = HomeService.CurrentHome().HomeId,
                Name = resultInverter.Model.Name,
                UserName = userName,
                Password = string.IsNullOrEmpty(password) ? "" : password.Encrypt(AppConstants.Secretkey),
                ApiUrl = apiUrl,
                ApiKey = string.IsNullOrEmpty(token) ? "" : token.Encrypt(AppConstants.Secretkey)
            };
            //TODO:Do we need more info from Inverter
            await mscDbContext.Inverter.AddAsync(inverterExist);
            await mscDbContext.SaveChangesAsync();
        }
        else
        {
            await DialogService.ShowAlertAsync("This house is already tracked", AppResources.My_Solar_Cells, AppResources.Ok);
        }
        if (SettingsService.OnboardingStatus == OnboardingStatusEnum.OnboardingDone)
        {
            await GoBack();
        }
        else
        {
            SettingsService.OnboardingStatus = OnboardingStatusEnum.InverterSelected;
            await GoToAsync(nameof(EnergyCalculationParameterView));
        }
        
    }

    private ObservableCollection<InverterSite> sitesNodes = new ObservableCollection<InverterSite>();
    public ObservableCollection<InverterSite> SitesNodes
    {
        get => sitesNodes;
        set => SetProperty(ref sitesNodes, value);
    }
    private InverterSite? selectedSiteNode;
    public InverterSite? SelectedSiteNode
    {
        get => selectedSiteNode;
        set
        {
            SetProperty(ref selectedSiteNode, value);
            if (value != null) InstallDate = value.InstallationDate;
        }
    }
    private ObservableCollection<PickerItem> inverterModels = new ObservableCollection<PickerItem>();
    public ObservableCollection<PickerItem> InverterModels
    {
        get => inverterModels;
        set => SetProperty(ref inverterModels, value);
    }
    private PickerItem? selectedInverterModel;
    public PickerItem? SelectedInverterModel
    {
        get => selectedInverterModel;
        set
        {
            SetProperty(ref selectedInverterModel, value);
            if (value != null)
            {
                inverterService = ServiceHelper.GetInverterService(value.ItemValue);
                ShowUserName = inverterService.ShowUserName;
                ShowPassword = inverterService.ShowPassword;
                ShowApiUrl = inverterService.ShowApiUrl;
                ShowApiKey = inverterService.ShowApiKey;
                InverterGuideText = inverterService.InverterGuideText;
                ApiUrl = inverterService.DefaultApiUrl;
            }
        }
    }
    
    private string inverterGuideText="";
    public string InverterGuideText
    {
        get => inverterGuideText;
        set => SetProperty(ref inverterGuideText, value);
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
    private DateTime installDate = DateTime.Now;
    public DateTime InstallDate
    {
        get => installDate;
        set => SetProperty(ref installDate, value);
    }
    private string apiUrl="";
    public string ApiUrl
    {
        get => apiUrl;
        set => SetProperty(ref apiUrl, value);
    }
    private string apiKey= "";
    public string ApiKey
    {
        get => apiKey;
        set => SetProperty(ref apiKey, value);
    }



    private bool showSiteNodePicker;
    public bool ShowSiteNodePicker
    {
        get => showSiteNodePicker;
        set => SetProperty(ref showSiteNodePicker, value);
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
}

