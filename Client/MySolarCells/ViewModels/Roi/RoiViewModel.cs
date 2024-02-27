namespace MySolarCells.ViewModels.Roi;

public class RoiViewModel : BaseViewModel
{
    IHistoryDataService roiService;
    IDataSyncService dataSyncService;
    public RoiViewModel(IHistoryDataService roiService, IDataSyncService dataSyncService)
    {
        this.roiService = roiService;
        this.dataSyncService = dataSyncService;
        this.ChartDataRequest = MySolarCellsGlobals.ChartDataRequest;
        WeakReferenceMessenger.Default.Register<RefreshRoiViewMessage>(this, async (r, m) =>
        {
            await ReloadData(true);

        });

    }
    public ICommand ReloadGraphDataCommand => new Command(async () => await ReloadData(true));
    public ICommand SyncCommand => new Command(async () => await Sync());
    public async override Task OnAppearingAsync()
    {
        this.ChartDataRequest = MySolarCellsGlobals.ChartDataRequest;
        await ReloadData(true);
        
    }
    private async Task Sync()
    {
        using var dlg = DialogService.GetProgress(AppResources.Import_Data);
        await Task.Delay(200);
        var result = await this.dataSyncService.Sync();
        if (!result.WasSuccessful)
        {
            await DialogService.ShowAlertAsync(result.ErrorMessage, AppResources.My_Solar_Cells, AppResources.Ok);
        }
        else
        {
            DialogService.ShowToast(result.Model.Message);
        }
        await ReloadData(false);
        IsRefreshing = false;
    }
    private async Task<bool> ReloadData(bool showProgressDlg)
    {
        //if (showProgressDlg)
        //    using var dlg = DialogService.GetProgress("");
        await Task.Delay(200);
        var diffrens = ChartDataRequest.FilterEnd - ChartDataRequest.FilterStart;
        if (diffrens.TotalDays > 31)
        {
            var ReportStats = await this.roiService.GenerateTotalPermonthReport(ChartDataRequest.FilterStart, ChartDataRequest.FilterEnd);
            RoiStats = ReportStats.Model.Item1.First().HistoryStats;
        }
        else
        {
            RoiStats = await this.roiService.CalculateTotals(ChartDataRequest.FilterStart, ChartDataRequest.FilterEnd, RoiSimulate);
        }
       
        return true;
    }
    
    private HistorySimulate roiSimulate = new HistorySimulate();
    public HistorySimulate RoiSimulate
    {
        get { return roiSimulate; }
        set { SetProperty(ref roiSimulate, value); }
    }
    //private ChartDataRequest chartDataRequest = new ChartDataRequest();
    //public ChartDataRequest ChartDataRequest
    //{
    //    get { return chartDataRequest; }
    //    set { SetProperty(ref chartDataRequest, value); }
    //}

    private HistoryStats roiStats = new HistoryStats();
    public HistoryStats RoiStats
    {
        get { return roiStats; }
        set { SetProperty(ref roiStats, value); }
    }


}

