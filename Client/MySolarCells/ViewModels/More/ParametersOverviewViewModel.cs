using MySolarCellsSQLite.Sqlite;
namespace MySolarCells.ViewModels.More;

public class ParametersOverviewViewModel : BaseViewModel
{
    private readonly MscDbContext mscDbContext;
    public ParametersOverviewViewModel( MscDbContext mscDbContext,IDialogService dialogService,
        IAnalyticsService analyticsService, IInternetConnectionService internetConnectionService, ILogService logService,ISettingsService settingsService,IHomeService homeService): base(dialogService, analyticsService, internetConnectionService,
        logService,settingsService,homeService)
    {
        this.mscDbContext = mscDbContext;
    }

    public ICommand AddInvestAndLonCommand => new Command(async () => await ShowInvestAndLon());
    public ICommand AddCalcParametersCommand => new Command(async () => await ShowCalcParameters());
   
    private async Task ShowCalcParameters()
    {
        await GoToAsync(nameof(EnergyCalculationParameterView));
    }
    public override async Task OnAppearingAsync()
    {
        var list = new ObservableCollection<ParamOverviewDto>();
        var calculationList = await mscDbContext.EnergyCalculationParameter.ToListAsync();
        foreach (var item in calculationList)
        {
            list.Add(new ParamOverviewDto { FromDate = item.FromDate, Icon = IconFont.Bargraph, Name = AppResources.Calculation_Parameters });
        }
        var investmentList = await mscDbContext.InvestmentAndLon.ToListAsync();
        foreach (var item in investmentList)
        {
            list.Add(new ParamOverviewDto { FromDate = item.FromDate, Icon = IconFont.Money, Name = AppResources.Investment_And_Loan });
        }
        ParamOverviewDtos = new ObservableCollection<ParamOverviewDto>(list.OrderByDescending(x => x.FromDate)); 
    }
    private ObservableCollection<ParamOverviewDto> paramOverviewDtos = new();
    public ObservableCollection<ParamOverviewDto> ParamOverviewDtos
    {
        get => paramOverviewDtos;
        set => SetProperty(ref paramOverviewDtos, value);
    }
   
    private async Task ShowInvestAndLon()
    {
        await GoToAsync(nameof(InvestmentAndLoanView));
    }
}

