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
        RoiStats = await  this.roiService.CalculateTotals();
        
    }


    private RoiStats roiStats = new RoiStats();
    public RoiStats RoiStats
    {
        get { return roiStats; }
        set { SetProperty(ref roiStats, value); }
    }


}

