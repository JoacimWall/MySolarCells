

namespace MySolarCells.ViewModels.OnBoarding
{
	public class InverterConnectViewModel : BaseViewModel
    {

        IKostalService kostalService;
        public InverterConnectViewModel(IKostalService kostalService)
		{
            this.kostalService = kostalService;
            InverterModels.Add(new PickerItem { ItemTitle = InverterTyp.Kostal.ToString(), ItemValue = (int)InverterTyp.Kostal });
            SelectedInverterModel = InverterModels.First();
            
        }

        public ICommand GetSitesCommand => new Command(async () => await GetSitesNodes());

        private async Task GetSitesNodes()
        {
            var resultLogin = await this.kostalService.LoginInUser(this.userName, this.password);
            if (!resultLogin.WasSuccessful)
            {
                await DialogService.ShowAlertAsync(resultLogin.ErrorMessage, AppResources.My_Solar_Cells, AppResources.Ok);
                return;
            }
            var resultSites = await this.kostalService.GetSites();
            if (!resultSites.WasSuccessful)
            {
                await DialogService.ShowAlertAsync(resultSites.ErrorMessage, AppResources.My_Solar_Cells, AppResources.Ok);
                return;
            }
            foreach (var node in resultSites.Model.data.sites.nodes)
            {
                SitesNodes.Add(node);
            }
            SelectedSiteNode = SitesNodes.First();
            ShowSiteNodePicker = true;
        }

        public ICommand SaveInverterCommand => new Command(async () => await SavAndMoveOn());

        private async Task SavAndMoveOn()
        {
            
            //Get Inverter
            var resultInverter = await this.kostalService.GetInverter(selectedSiteNode.id);
            if (!resultInverter.WasSuccessful)
            {
                await DialogService.ShowAlertAsync(resultInverter.ErrorMessage, AppResources.My_Solar_Cells, AppResources.Ok);
                return;
            }

            using var dbContext = new MscDbContext();
            //check if Home exist in db
            var InverterExist  = await dbContext.Inverter.FirstOrDefaultAsync(x => x.SubSystemEntityId == resultInverter.Model.id.ToString() && x.InverterTyp == SelectedInverterModel.ItemValue);
            if (InverterExist == null)
            {
                InverterExist = new Services.Sqlite.Models.Inverter
                {
                    SubSystemEntityId = resultInverter.Model.id.ToString(),
                    InverterTyp = (int)SelectedInverterModel.ItemValue,
                    FromDate = new DateTime(installDate.Year, installDate.Month, installDate.Day),
                    HomeId = MySolarCellsGlobals.SelectedHome.HomeId,
                    Name = resultInverter.Model.name,
                    UserName = this.userName,
                    Password = StringHelper.Encrypt(this.password,AppConstants.Secretkey)
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

        private ObservableCollection<Node> sitesNodes = new ObservableCollection<Node>();
        public ObservableCollection<Node> SitesNodes
        {
            get => sitesNodes;
            set
            {
                SetProperty(ref sitesNodes, value);

            }
        }
        private Node selectedSiteNode;
        public Node SelectedSiteNode
        {
            get => selectedSiteNode;
            set { SetProperty(ref selectedSiteNode, value); }
        }
        private ObservableCollection<PickerItem> inverterModels = new ObservableCollection<PickerItem>();
        public ObservableCollection<PickerItem> InverterModels
        {
            get => inverterModels;
            set {  SetProperty(ref inverterModels, value); }
        }
        private PickerItem selectedInverterModel;
        public PickerItem SelectedInverterModel
        {
            get => selectedInverterModel;
            set { SetProperty(ref selectedInverterModel, value);}
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
        private DateTime installDate= DateTime.Now;
        public DateTime InstallDate
        {
            get => installDate;
            set { SetProperty(ref installDate, value); }
        }
        private bool showSiteNodePicker;
        public bool ShowSiteNodePicker
        {
            get => showSiteNodePicker;
            set { SetProperty(ref showSiteNodePicker, value); }
        }

    }
}

