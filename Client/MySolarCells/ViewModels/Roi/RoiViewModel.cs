namespace MySolarCells.ViewModels.Roi;

public class RoiViewModel : BaseViewModel
{
    readonly IHistoryDataService roiService;
    readonly IDataSyncService dataSyncService;
    public RoiViewModel(IHistoryDataService roiService, IDataSyncService dataSyncService,IDialogService dialogService,
        IAnalyticsService analyticsService, IInternetConnectionService internetConnectionService, ILogService logService,ISettingsService settingsService,IHomeService homeService): base(dialogService, analyticsService, internetConnectionService,
        logService,settingsService,homeService)
    {
        this.roiService = roiService;
        this.dataSyncService = dataSyncService;
        ChartDataRequest = HomeService.CurrentChartDataRequest();
        WeakReferenceMessenger.Default.Register<RefreshRoiViewMessage>(this, async (_, _) =>
        {
            await ReloadData();

        });

    }
    public ICommand ReloadGraphDataCommand => new Command(async () => await ReloadData());
    public ICommand SyncCommand => new Command(async () => await Sync());
    public override async Task OnAppearingAsync()
    {
        ChartDataRequest = HomeService.CurrentChartDataRequest();
        await ReloadData();
        
    }
    private async Task Sync()
    {
        _ = (ProgressDialog)DialogService.GetProgress(AppResources.Import_Data);
        await Task.Delay(200);
        var result = await dataSyncService.Sync();
        if (!result.WasSuccessful || result.Model == null)
        {
            await DialogService.ShowAlertAsync(result.ErrorMessage, AppResources.My_Solar_Cells, AppResources.Ok);
        }
        else
        {
            DialogService.ShowToast(result.Model.Message);
        }
        await ReloadData();
        IsRefreshing = false;
    }
    private async Task ReloadData()
    {
        //if (showProgressDlg)
        //    using var dlg = DialogService.GetProgress("");
        await Task.Delay(200);
        var difference = ChartDataRequest.FilterEnd - ChartDataRequest.FilterStart;
        if (difference.TotalDays > 31)
        {
            var reportStats = await roiService.GenerateTotalPerMonthReport(ChartDataRequest.FilterStart, ChartDataRequest.FilterEnd);
            if (reportStats.Model != null) 
                RoiStats = reportStats.Model.Item1.First().HistoryStats;
        }
        else
        {
            RoiStats = await roiService.CalculateTotals(ChartDataRequest.FilterStart, ChartDataRequest.FilterEnd, RoiSimulate);
        }
    }
   
    private HistoryStats roiStats = new HistoryStats();
    public HistoryStats RoiStats
    {
        get => roiStats;
        set => SetProperty(ref roiStats, value);
    }


}

