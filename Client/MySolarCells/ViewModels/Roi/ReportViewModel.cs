namespace MySolarCells.ViewModels.Roi;

public class ReportViewModel : BaseViewModel
{
    IHistoryDataService historyDataService;
    IDataSyncService dataSyncService;
    IEnergyChartService energyChartService;
    IRoiService roiService;
    public ReportViewModel(IHistoryDataService historyDataService, IRoiService roiService, IDataSyncService dataSyncService, IEnergyChartService energyChartService)
    {
        this.historyDataService = historyDataService;
        this.roiService = roiService;
        this.dataSyncService = dataSyncService;
        this.energyChartService = energyChartService; 

        //WeakReferenceMessenger.Default.Register<RefreshRoiViewMessage>(this, async (r, m) =>
        //{
        //    await ReloadData(true);

        //});

    }
    public ICommand ReloadGraphDataCommand => new Command(async () => await ReloadData(true));
    public ICommand PrevPageCommand => new Command(async () => await PrevPage(true));
    public ICommand NextPageCommand => new Command(async () => await NextPage(true));
    private async  Task PrevPage(bool v)
    {
        using var dlg = DialogService.GetProgress(AppResources.Loading);
        await Task.Delay(300);
        for (int i = ReportData.Count - 1; i >= 0; i--)
        {
            if (ReportData[i] == SelReportPage && i > 0)
            {
                SelReportPage = ReportData[i-1];
                return;
            }
        }
        
    }

  

    private async Task NextPage(bool v)
    {
        using var dlg = DialogService.GetProgress(AppResources.Loading);
        await Task.Delay(300);
        for (int i = 0; i < ReportData.Count; i++)
        {
            if (ReportData[i] == SelReportPage && i < ReportData.Count - 1)
            {
                SelReportPage = ReportData[i+1];
                return;
            }
        }
        
    }
    public async override Task RefreshAsync(object layoutState)
    {
        await ReloadData(true);
    }
    
   
    private async Task<bool> ReloadData(bool showProgressDlg)
    {
        using var dlg = DialogService.GetProgress(AppResources.Generating_Report_Please_Wait);
        await Task.Delay(300);
        var result = await this.historyDataService.GenerateTotalPermonthReport(MySolarCellsGlobals.SelectedHome.FromDate, DateTime.Today);
        var resultRoi = this.roiService.CalcSavingsEstimate(result.Model);
        if (!result.WasSuccessful)
        {
            await DialogService.ShowAlertAsync(result.ErrorMessage, AppResources.My_Solar_Cells, AppResources.Ok);
        }
        else
        {
            ReportData.Add(new ReportModel { EstimatRoi = resultRoi.Model, ReportPageTyp = ReportPageTyp.SavingEssitmate , ReportTitle = AppResources.Savings_Calculation});
            ReportData.Add(new ReportModel { Stats = result.Model.Item1, ReportPageTyp = ReportPageTyp.YearsOverview, ReportTitle = AppResources.Year_Overview });
            foreach (var item in result.Model.Item2)
            {
                ReportData.Add(new ReportModel { Stats = item, ReportPageTyp = ReportPageTyp.YearDetails, ReportTitle = string.Format(AppResources.Details_Year,item.First().FromDate.Year) });
            }
            await Task.Delay(300);
            SelReportPage = ReportData.First();
        }
        return true;
    }

    //private HistorySimulate roiSimulate = new HistorySimulate();
    //public HistorySimulate RoiSimulate
    //{
    //    get { return roiSimulate; }
    //    set { SetProperty(ref roiSimulate, value); }
    //}
    //private ChartDataRequest chartDataRequest = new ChartDataRequest();
    //public ChartDataRequest ChartDataRequest
    //{
    //    get { return chartDataRequest; }
    //    set { SetProperty(ref chartDataRequest, value); }
    //}

    //private HistoryStats roiStats = new HistoryStats();
    //public HistoryStats RoiStats
    //{
    //    get { return roiStats; }
    //    set { SetProperty(ref roiStats, value); }
    //}
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

