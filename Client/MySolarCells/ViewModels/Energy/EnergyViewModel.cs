namespace MySolarCells.ViewModels.Energy;

public class EnergyViewModel : BaseViewModel
{
    IEnergyChartService energyChartService;
    public EnergyViewModel(IEnergyChartService energyChartService)
    {
        this.energyChartService = energyChartService;

    }

    public async override Task OnAppearingAsync()
    {
        RoiStats = new RoiStats(); // await  this.roiService.CalculateTotals();
        
    }


    private RoiStats roiStats = new RoiStats();
    public RoiStats RoiStats
    {
        get { return roiStats; }
        set { SetProperty(ref roiStats, value); }
    }


}

