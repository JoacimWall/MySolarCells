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
       
    }

  

    private async Task NextPage(bool v)
    {
        SelReportPage = ReportData[1];
    }

    public async override Task OnAppearingAsync()
    {
        await ReloadData(true);
        
    }
   
    private async Task<bool> ReloadData(bool showProgressDlg)
    {
        //if (showProgressDlg)
        //    using var dlg = DialogService.GetProgress("");
        await Task.Delay(200);
        // var diffrens = ChartDataRequest.FilterEnd - ChartDataRequest.FilterStart;
        //if (diffrens.TotalDays > 31)
        //{
        //    var ReportStats = await this.roiService.GenerateTotalPermonthReport(ChartDataRequest.FilterStart, ChartDataRequest.FilterEnd);
        //    RoiStats = ReportStats.Model.Item1.First().HistoryStats;
        //}
        //else
        //{
        //    RoiStats = await this.roiService.CalculateTotals(ChartDataRequest.FilterStart, ChartDataRequest.FilterEnd, RoiSimulate);
        //}
        var result = await this.historyDataService.GenerateTotalPermonthReport(MySolarCellsGlobals.SelectedHome.FromDate, DateTime.Today);
        var resultRoi = this.roiService.CalcSavingsEstimate(result.Model);
        if (!result.WasSuccessful)
        {


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

