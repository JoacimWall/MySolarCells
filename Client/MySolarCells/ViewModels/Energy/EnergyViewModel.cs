
using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace MySolarCells.ViewModels.Energy;

public class EnergyViewModel : BaseViewModel
{
    private readonly IEnergyChartService energyChartService;
   
    private IDataSyncService dataSyncService;
    public EnergyViewModel(IEnergyChartService energyChartService, IDataSyncService dataSyncService)
    {
        this.energyChartService = energyChartService;
        this.dataSyncService = dataSyncService;
    }
    public ICommand SyncCommand => new Command(async () => await Sync());
    public ICommand ReloadGraphDataCommand => new Command(async () => await ReloadGraph());
    

    private async Task Sync()
    {
        using var dlg = DialogService.GetProgress("");
        var result = await this.dataSyncService.Sync();
        if (!result.WasSuccessful)
        {
            await DialogService.ShowAlertAsync(result.ErrorMessage, AppResources.My_Solar_Cells, AppResources.Ok);
        }

        await ReloadGraph();
    }

    private async Task<bool> ReloadGraph(bool fromOnAppearing = false)
    {
        using var dlg = DialogService.GetProgress("");
        var resultSeries = await energyChartService.GetChartData(ChartDataRequest);
        //If there is no data try to fetch
        if (fromOnAppearing && !resultSeries.WasSuccessful && resultSeries.ErrorCode == ErrorCodes.NoEnergyEntryOnCurrentDate)
        {
            await Sync();
            resultSeries = await energyChartService.GetChartData(ChartDataRequest);
        }
        if (!resultSeries.WasSuccessful)
        {
            await DialogService.ShowAlertAsync("There is no data to show on This date.", AppResources.My_Solar_Cells, AppResources.Ok);
            EnergyChartProdTot = null;
            EnergyChartConsumed = null;
            return false;
        }
        

        SKColor LineColor  = AppColors.Gray700Color.ToSKColor();
        SKPaint YAxesPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = LineColor,
            IsAntialias = true,
            TextSize = 20,
            Typeface = SkiaSharpHelper.OpenSansRegular
        };
        
        EnergyChartProdTotTitle = resultSeries.Model.ChartSeriesProduction[0].Name;
        if (resultSeries.Model.ChartSeriesProduction[0].Entries.Any(x => x.Value.HasValue && x.Value.Value > 0) || resultSeries.Model.ChartSeriesProduction[1].Entries.Any(x => x.Value.HasValue && x.Value.Value > 0))
            EnergyChartProdTot = new Microcharts.BarChart
            {
                Margin = 6,
                Series = resultSeries.Model.ChartSeriesProduction,
                CornerRadius = 6,
                ShowYAxisLines = true,
                ShowYAxisText = true,
                YAxisTextPaint = YAxesPaint,
                LabelTextSize = 24,
                SerieLabelTextSize = 24,
                ValueLabelTextSize = 24,
                MaxValue = resultSeries.Model.MaxValueYaxes,
                LegendOption = Microcharts.SeriesLegendOption.Bottom
            };
        else
            EnergyChartProdTot = null;

        EnergyChartProdSoldTitle = resultSeries.Model.ChartSeriesConsumtion[0].Name;
        if (resultSeries.Model.ChartSeriesConsumtion[0].Entries.Any(x => x.Value.HasValue && x.Value.Value > 0) || resultSeries.Model.ChartSeriesConsumtion[1].Entries.Any(x => x.Value.HasValue && x.Value.Value > 0))
            EnergyChartConsumed = new Microcharts.BarChart
            { 
                Margin = 6, 
                Series = resultSeries.Model.ChartSeriesConsumtion,
                CornerRadius = 6,
                ShowYAxisLines = true,
                ShowYAxisText = true,
                YAxisTextPaint = YAxesPaint,
                LabelTextSize = 24,
                SerieLabelTextSize = 24,
                ValueLabelTextSize = 24,
                MaxValue = resultSeries.Model.MaxValueYaxes,
                LegendOption= Microcharts.SeriesLegendOption.Bottom
                
            };
        else
            EnergyChartConsumed = null;

       
        return true;
    }
    public async override Task OnAppearingAsync()
    {
        if (FirstTimeAppearing)
            await ReloadGraph(true);



    }

    private ChartDataRequest chartDataRequest = new ChartDataRequest();
    public ChartDataRequest ChartDataRequest
    {
        get { return chartDataRequest; }
        set { SetProperty(ref chartDataRequest, value); }
    }

    private Microcharts.Chart energyChartProdTot;
    public Microcharts.Chart EnergyChartProdTot
    {
        get { return energyChartProdTot; }
        set { SetProperty(ref energyChartProdTot, value); }
    }
   
    private Microcharts.Chart energyChartConsumed;
    public Microcharts.Chart EnergyChartConsumed
    {
        get { return energyChartConsumed; }
        set { SetProperty(ref energyChartConsumed, value); }
    }


    private string energyChartProdTotTitle;
    public string EnergyChartProdTotTitle
    {
        get { return energyChartProdTotTitle; }
        set { SetProperty(ref energyChartProdTotTitle, value); }
    }
    private string energyChartProdSoldTitle;
    public string EnergyChartProdSoldTitle
    {
        get { return energyChartProdSoldTitle; }
        set { SetProperty(ref energyChartProdSoldTitle, value); }
    }
    private string energyChartProdUsedTitle;
    public string EnergyChartProdUsedTitle
    {
        get { return energyChartProdUsedTitle; }
        set { SetProperty(ref energyChartProdUsedTitle, value); }
    }
    private string energyChartProdConsumedTitle;
    public string EnergyChartProdConsumedTitle
    {
        get { return energyChartProdConsumedTitle; }
        set { SetProperty(ref energyChartProdConsumedTitle, value); }
    }

}

