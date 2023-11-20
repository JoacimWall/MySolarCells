using MySolarCellsSQLite.Sqlite.Models;

namespace MySolarCells.ViewModels.Roi;

public class ReportViewModel : BaseViewModel
{
    IHistoryDataService historyDataService;
    IDataSyncService dataSyncService;
    IEnergyChartService energyChartService;
    IRoiService roiService;
    private readonly MscDbContext mscDbContext;
    public ReportViewModel(IHistoryDataService historyDataService, IRoiService roiService, IDataSyncService dataSyncService, IEnergyChartService energyChartService, MscDbContext mscDbContext)
    {
        this.historyDataService = historyDataService;
        this.roiService = roiService;
        this.dataSyncService = dataSyncService;
        this.energyChartService = energyChartService;
        this.mscDbContext = mscDbContext;
        var exist = this.mscDbContext.SavingEssitmateParameters.FirstOrDefault();
        if (exist is null)
        {
            exist = new SavingEssitmateParameters();
            this.mscDbContext.SavingEssitmateParameters.Add(exist);
            this.mscDbContext.SaveChanges();
        }
        SavingEssitmateParameters = exist;
    }
    public ICommand MoreButtonCommand => new Command(async () => await MoreButton());

    private async Task MoreButton()
    {
        ShowSavingEsstimateIsVisible = !showSavingEsstimateIsVisible;
    }
    
    public ICommand RefreshSavingEssitmateCommand => new Command(async () => await ReloadSavingsEstimate(false));
    public ICommand ReloadGraphDataCommand => new Command(async () => await RefreshAsync(null));
    public ICommand PrevPageCommand => new Command(async () => await PrevPage(true));
    public ICommand NextPageCommand => new Command(async () => await NextPage(true));
    private async Task PrevPage(bool v)
    {
        using var dlg = DialogService.GetProgress(AppResources.Loading);
        await Task.Delay(300);
        for (int i = ReportData.Count - 1; i >= 0; i--)
        {
            if (ReportData[i] == SelReportPage && i > 0)
            {
                SelReportPage = ReportData[i - 1];
                return;
            }
        }

    }

    public async override Task OnDisappearingAsync()
    {
        //Save cahnges to the SavingEssitmateParameters 
        await this.mscDbContext.SaveChangesAsync();
    }

    private async Task NextPage(bool v)
    {
        using var dlg = DialogService.GetProgress(AppResources.Loading);
        await Task.Delay(300);
        for (int i = 0; i < ReportData.Count; i++)
        {
            if (ReportData[i] == SelReportPage && i < ReportData.Count - 1)
            {
                SelReportPage = ReportData[i + 1];
                return;
            }
        }

    }
    public async override Task RefreshAsync(object layoutState)
    {
        using var dlg = DialogService.GetProgress(AppResources.Generating_Report_Please_Wait);
        await Task.Delay(300);
        await ReloadHistoryData(true);
        await ReloadSavingsEstimate(true);
    }

    private Result<Tuple<List<ReportHistoryStats>, List<List<ReportHistoryStats>>>> resultHistory;
    private async Task<bool> ReloadHistoryData(bool showProgressDlg)
    {
        resultHistory = await this.historyDataService.GenerateTotalPermonthReport(MySolarCellsGlobals.SelectedHome.FromDate, DateTime.Today);
        if (!resultHistory.WasSuccessful)
        {
            await DialogService.ShowAlertAsync(resultHistory.ErrorMessage, AppResources.My_Solar_Cells, AppResources.Ok);
        }
        return true;
    }
    private async Task<bool> ReloadSavingsEstimate(bool firstTime)
    {
        using var dlg = DialogService.GetProgress(AppResources.Generating_Report_Please_Wait);
        await Task.Delay(300);

        var resultRoi = this.roiService.CalcSavingsEstimate(resultHistory.Model, SavingEssitmateParameters);
        if (!resultRoi.WasSuccessful)
        {
            await DialogService.ShowAlertAsync(resultRoi.ErrorMessage, AppResources.My_Solar_Cells, AppResources.Ok);
        }
        else
        {
            if (firstTime)
            {
                ReportData.Add(new ReportModel { EstimatRoi = resultRoi.Model, ReportPageTyp = ReportPageTyp.SavingEssitmate, ReportTitle = AppResources.Savings_Calculation });
                ReportData.Add(new ReportModel { Stats = resultHistory.Model.Item1, ReportPageTyp = ReportPageTyp.YearsOverview, ReportTitle = AppResources.Year_Overview });
                foreach (var item in resultHistory.Model.Item2)
                {
                    ReportData.Add(new ReportModel { Stats = item, ReportPageTyp = ReportPageTyp.YearDetails, ReportTitle = string.Format(AppResources.Details_Year, item.First().FromDate.Year) });
                }
                await Task.Delay(300);
                SelReportPage = ReportData.First();
            }
            else
            {
                ReportData[0] = new ReportModel { EstimatRoi = resultRoi.Model, ReportPageTyp = ReportPageTyp.SavingEssitmate, ReportTitle = AppResources.Savings_Calculation };
                SelReportPage = ReportData.First();
            }
           
        }
        return true;
    }
    //private HistorySimulate roiSimulate = new HistorySimulate();
    //public HistorySimulate RoiSimulate
    //{
    //    get { return roiSimulate; }
    //    set { SetProperty(ref roiSimulate, value); }
    //}

    private bool showSavingEsstimateIsVisible = false;
    public bool ShowSavingEsstimateIsVisible
    {
        get { return showSavingEsstimateIsVisible; }
        set { SetProperty(ref showSavingEsstimateIsVisible, value); }
    }

    private SavingEssitmateParameters savingEssitmateParameters;
    public SavingEssitmateParameters SavingEssitmateParameters
    {
        get { return savingEssitmateParameters; }
        set { SetProperty(ref savingEssitmateParameters, value); }
    }
    private ReportModel selReportPage;
    public ReportModel SelReportPage
    {
        get { return selReportPage; }
        set { SetProperty(ref selReportPage, value); }
    }
    private List<ReportModel> reportData = new List<ReportModel>();
    public List<ReportModel> ReportData
    {
        get { return reportData; }
        set { SetProperty(ref reportData, value); }
    }

}

