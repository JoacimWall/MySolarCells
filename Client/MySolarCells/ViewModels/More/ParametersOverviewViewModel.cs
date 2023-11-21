namespace MySolarCells.ViewModels.More;

public class ParametersOverviewViewModel : BaseViewModel
{
    private readonly IHistoryDataService historyService;
    private readonly IRoiService roiService;
    private readonly MscDbContext mscDbContext;
    public ParametersOverviewViewModel(IHistoryDataService historyService, IRoiService roiService, MscDbContext mscDbContext)
    {
        this.historyService = historyService;
        this.roiService = roiService;
        this.mscDbContext = mscDbContext;
       
    }

    public ICommand AddInvestAndLonCommand => new Command(async () => await ShowInvestAndLon());
    public ICommand AddCalcParametersCommand => new Command(async () => await ShowCalcParameters());
    //public ICommand ElectricitySupplierCommand => new Command(async () => await ShowElectricitySupplier());
    //public ICommand ExportExcelCommand => new Command(async () => await ExportExcelGroupYear(MySolarCellsGlobals.SelectedHome.HomeId));
    //public ICommand InverterSettingsCommand => new Command(async () => await ShowInverterSettings());
    //public ICommand ShowReportCommand => new Command(async () => await ShowReport());

    private async Task ShowCalcParameters()
    {
        await GoToAsync(nameof(EnergyCalculationParameterView));
    }
    public async override Task OnAppearingAsync()
    {
        var list = new ObservableCollection<ParamOverviewDto>();
        var calclist = await mscDbContext.EnergyCalculationParameter.ToListAsync();
        foreach (var item in calclist)
        {
            list.Add(new ParamOverviewDto { FromDate = item.FromDate, Icon = IconFont.Bargraph, Name = AppResources.Calculation_Parameters });
        }
        var investlist = await mscDbContext.InvestmentAndLon.ToListAsync();
        foreach (var item in investlist)
        {
            list.Add(new ParamOverviewDto { FromDate = item.FromDate, Icon = IconFont.Money, Name = AppResources.Investment_And_Loan });
        }
        ParamOverviewDtos = new ObservableCollection<ParamOverviewDto>(list.OrderByDescending(x => x.FromDate)); 
    }
    private ObservableCollection<ParamOverviewDto> paramOverviewDtos = new ObservableCollection<ParamOverviewDto>();
    public ObservableCollection<ParamOverviewDto> ParamOverviewDtos
    {
        get { return paramOverviewDtos; }
        set { SetProperty(ref paramOverviewDtos, value); }
    }
   
    private async Task ShowInvestAndLon()
    {
        await GoToAsync(nameof(InvestmentAndLoanView));
    }
    
    //private string _appInfoVersion;
    //public string AppInfoVersion
    //{
    //    get { return _appInfoVersion; }
    //    set { SetProperty(ref _appInfoVersion, value); }
    //}
}

