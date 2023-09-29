namespace MySolarCells.ViewModels.Roi;

public class RoiViewModel : BaseViewModel
{
    IRoiService roiService;
    IDataSyncService dataSyncService;
    public RoiViewModel(IRoiService roiService, IDataSyncService dataSyncService)
    {
        this.roiService = roiService;
        this.dataSyncService = dataSyncService;

    }
    public ICommand ReloadGraphDataCommand => new Command(async () => await ReloadData());
    public ICommand SyncCommand => new Command(async () => await Sync());
    public async override Task OnAppearingAsync()
    {
        await ReloadData();
        
    }
    private async Task Sync()
    {
        using var dlg = DialogService.GetProgress("");
        var result = await this.dataSyncService.Sync();
        if (!result.WasSuccessful)
        {
            await DialogService.ShowAlertAsync(result.ErrorMessage, AppResources.My_Solar_Cells, AppResources.Ok);
        }

        await ReloadData();
    }
    private async Task<bool> ReloadData()
    {
        using var dlg = DialogService.GetProgress("");
        RoiStats = await this.roiService.CalculateTotals(ChartDataRequest.FilterStart, ChartDataRequest.FilterEnd, false);
        return true;
    }
    private ChartDataRequest chartDataRequest = new ChartDataRequest();
    public ChartDataRequest ChartDataRequest
    {
        get { return chartDataRequest; }
        set { SetProperty(ref chartDataRequest, value); }
    }

    private RoiStats roiStats = new RoiStats();
    public RoiStats RoiStats
    {
        get { return roiStats; }
        set { SetProperty(ref roiStats, value); }
    }


}

