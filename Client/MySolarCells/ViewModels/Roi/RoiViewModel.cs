namespace MySolarCells.ViewModels.Roi;

public class RoiViewModel : BaseViewModel
{
    IRoiService roiService;
    public RoiViewModel(IRoiService roiService)
    {
        this.roiService = roiService;

    }

    public async override Task OnAppearingAsync()
    {
        using var dbContext = new MscDbContext();
        RoiStats = await  this.roiService.CalculateTotals(null,null,true);
        
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

