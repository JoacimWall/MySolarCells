namespace MySolarCells.ViewModels.Roi;

public class RoiViewModel : BaseViewModel
{
    IRoiService roiService;
    public RoiViewModel(IRoiService roiService)
    {
        this.roiService = roiService;

    }
    public ICommand ReloadGraphDataCommand => new Command(async () => await ReloadData());
    public async override Task OnAppearingAsync()
    {
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

