namespace MySolarCells.ViewModels.OnBoarding;

public class InverterViewModel : BaseViewModel
{
    private InverterLoginResponse inverterLoginResponse;
    private readonly MscDbContext mscDbContext;
    IInverterServiceInterface InverterService;
    public InverterViewModel(MscDbContext mscDbContext)
    {
        this.mscDbContext = mscDbContext;
        //InverterModels.Add(new PickerItem { ItemTitle = InverterTyp.HomeAssistent.ToString(), ItemValue = (int)InverterTyp.HomeAssistent });
        InverterModels.Add(new PickerItem { ItemTitle = InverterTyp.Huawei.ToString(), ItemValue = (int)InverterTyp.Huawei });
        InverterModels.Add(new PickerItem { ItemTitle = InverterTyp.Kostal.ToString(), ItemValue = (int)InverterTyp.Kostal });
        InverterModels.Add(new PickerItem { ItemTitle = InverterTyp.SolarEdge.ToString(), ItemValue = (int)InverterTyp.SolarEdge });
       
        var inverter = this.mscDbContext.Inverter.FirstOrDefault();
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

            UserName = inverter.UserName;
            Password = string.IsNullOrEmpty(inverter.Password) ? "" : StringHelper.Decrypt(inverter.Password, AppConstants.Secretkey); 
            ApiKey = string.IsNullOrEmpty(inverter.ApiKey) ? "" : StringHelper.Decrypt(inverter.ApiKey, AppConstants.Secretkey); 
        }
    }

    public ICommand TestConnectionCommand => new Command(async () => await TestConnection());
    public ICommand SaveCommand => new Command(async () => await SaveInverter());
    private async Task TestConnection()
    {
        var resultLogin = await this.InverterService.TestConnection(this.userName, this.password,this.apiUrl,this.apiKey);
        if (!resultLogin.WasSuccessful)
        {
            await DialogService.ShowAlertAsync(resultLogin.ErrorMessage, AppResources.My_Solar_Cells, AppResources.Ok);
            return;
        }
        inverterLoginResponse = resultLogin.Model;
        var resultSites = await this.InverterService.GetPickerOne();
        if (!resultSites.WasSuccessful)
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

        //Get Inverter
        var resultInverter = await this.InverterService.GetInverter(selectedSiteNode);
        if (!resultInverter.WasSuccessful)
        {
            await DialogService.ShowAlertAsync(resultInverter.ErrorMessage, AppResources.My_Solar_Cells, AppResources.Ok);
            return;
        }

        
        //check if Home exist in db
        var InverterExist = await this.mscDbContext.Inverter.FirstOrDefaultAsync(x => x.SubSystemEntityId == resultInverter.Model.InverterId.ToString() && x.InverterTyp == SelectedInverterModel.ItemValue);
        if (InverterExist == null)
        {
            InverterExist = new SQLite.Sqlite.Models.Inverter
            {
                SubSystemEntityId = resultInverter.Model.InverterId,
                InverterTyp = (int)SelectedInverterModel.ItemValue,
                FromDate = new DateTime(installDate.Year, installDate.Month, installDate.Day),
                HomeId = MySolarCellsGlobals.SelectedHome.HomeId,
                Name = resultInverter.Model.Name,
                UserName = this.userName,
                Password = string.IsNullOrEmpty(this.password) ? "" : StringHelper.Encrypt(this.password, AppConstants.Secretkey),
                ApiUrl = "",
                ApiKey = string.IsNullOrEmpty(this.inverterLoginResponse.token) ? "" : StringHelper.Encrypt(this.inverterLoginResponse.token, AppConstants.Secretkey)
            };
            //TODO:Do we neeed more info from Inverter
            await this.mscDbContext.Inverter.AddAsync(InverterExist);
            await this.mscDbContext.SaveChangesAsync();
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
        set
        {
            SetProperty(ref sitesNodes, value);
           
        }
    }
    private InverterSite selectedSiteNode;
    public InverterSite SelectedSiteNode
    {
        get => selectedSiteNode;
        set {
            SetProperty(ref selectedSiteNode, value);
            InstallDate = value.InstallationDate;
            }
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
    private string apiKey= "";
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

