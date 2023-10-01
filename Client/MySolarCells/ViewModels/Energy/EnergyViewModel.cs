
using Microcharts;
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
            EnergyChartProdSold = null;
            EnergyChartProdUsed = null;
            EnergyChartConsumedTot = null;
            EnergyChartConsumedGrid = null;
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
        //Production Total
        EnergyChartProdTotTitle = resultSeries.Model.ChartSeriesProductionTot[0].Name;
        if (resultSeries.Model.ChartSeriesProductionTot[0].Entries.Any(x => x.Value.HasValue && x.Value.Value > 0) )
            EnergyChartProdTot = new Microcharts.BarChart
            {
                Margin = 6,
                Entries = resultSeries.Model.ChartSeriesProductionTot[0].Entries,
                CornerRadius = 6,
                ShowYAxisLines = true,
                ShowYAxisText = true,
                YAxisTextPaint = YAxesPaint,
                LabelTextSize = 24,
                SerieLabelTextSize = 24,
                ValueLabelTextSize = 24,
                MaxValue = resultSeries.Model.MaxValueYaxes,
               // LegendOption = Microcharts.SeriesLegendOption.Bottom
            };
        else
            EnergyChartProdTot = null;

        //Production Sold
        EnergyChartProdSoldTitle = resultSeries.Model.ChartSeriesProductionSold[0].Name;
        if (resultSeries.Model.ChartSeriesProductionSold[0].Entries.Any(x => x.Value.HasValue && x.Value.Value > 0))
            EnergyChartProdSold = new Microcharts.BarChart
            {
                Margin = 6,
                Entries = resultSeries.Model.ChartSeriesProductionSold[0].Entries,
                CornerRadius = 6,
                ShowYAxisLines = true,
                ShowYAxisText = true,
                YAxisTextPaint = YAxesPaint,
                LabelTextSize = 24,
                SerieLabelTextSize = 24,
                ValueLabelTextSize = 24,
                MaxValue = resultSeries.Model.MaxValueYaxes,
               // LegendOption = Microcharts.SeriesLegendOption.Bottom
            };
        else
            EnergyChartProdSold = null;

        //Production Used
        EnergyChartProdUsedTitle = resultSeries.Model.ChartSeriesProductionUsed[0].Name;
        if (resultSeries.Model.ChartSeriesProductionUsed[0].Entries.Any(x => x.Value.HasValue && x.Value.Value > 0))
            EnergyChartProdUsed = new Microcharts.BarChart
            {
                Margin = 6,
                Entries = resultSeries.Model.ChartSeriesProductionUsed[0].Entries,
                CornerRadius = 6,
                ShowYAxisLines = true,
                ShowYAxisText = true,
                YAxisTextPaint = YAxesPaint,
                LabelTextSize = 24,
                SerieLabelTextSize = 24,
                ValueLabelTextSize = 24,
                MaxValue = resultSeries.Model.MaxValueYaxes,
               // LegendOption = Microcharts.SeriesLegendOption.Bottom
            };
        else
            EnergyChartProdUsed = null;

        //Consumtion Total
        EnergyChartConsumedTotTitle = resultSeries.Model.ChartSeriesConsumtionTot[0].Name;
        if (resultSeries.Model.ChartSeriesConsumtionTot[0].Entries.Any(x => x.Value.HasValue && x.Value.Value > 0))
            EnergyChartConsumedTot = new Microcharts.BarChart
            {
                Margin = 6,
                Entries = resultSeries.Model.ChartSeriesConsumtionTot[0].Entries,
                CornerRadius = 6,
                ShowYAxisLines = true,
                ShowYAxisText = true,
                YAxisTextPaint = YAxesPaint,
                LabelTextSize = 24,
                SerieLabelTextSize = 24,
                ValueLabelTextSize = 24,
                MaxValue = resultSeries.Model.MaxValueYaxes,
               // LegendOption = Microcharts.SeriesLegendOption.Bottom
            };
        else
            EnergyChartConsumedTot = null;
        //Consumtion Grid
        EnergyChartConsumedGridTitle = resultSeries.Model.ChartSeriesConsumtionGrid[0].Name;
        if (resultSeries.Model.ChartSeriesConsumtionGrid[0].Entries.Any(x => x.Value.HasValue && x.Value.Value > 0))
            EnergyChartConsumedGrid = new Microcharts.BarChart
            {
                Margin = 6,
                Entries = resultSeries.Model.ChartSeriesConsumtionGrid[0].Entries,
                CornerRadius = 6,
                ShowYAxisLines = true,
                ShowYAxisText = true,
                YAxisTextPaint = YAxesPaint,
                LabelTextSize = 24,
                SerieLabelTextSize = 24,
                ValueLabelTextSize = 24,
                MaxValue = resultSeries.Model.MaxValueYaxes,
                //LegendOption = Microcharts.SeriesLegendOption.Bottom
            };
        else
            energyChartConsumedGrid = null;
        return true;
    }
    public async override Task OnAppearingAsync()
    {
        if (FirstTimeAppearing)
            await ReloadGraph(true);



    }
    private ObservableCollection<ChartEntry> testData = new ObservableCollection<ChartEntry>();
    public ObservableCollection<ChartEntry> TestData
    {
        get => testData;
        set
        {
            SetProperty(ref testData, value);

        }
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
    private Microcharts.Chart energyChartProdSold;
    public Microcharts.Chart EnergyChartProdSold
    {
        get { return energyChartProdSold; }
        set { SetProperty(ref energyChartProdSold, value); }
    }
    private Microcharts.Chart energyChartProdUsed;
    public Microcharts.Chart EnergyChartProdUsed
    {
        get { return energyChartProdUsed; }
        set { SetProperty(ref energyChartProdUsed, value); }
    }
    private Microcharts.Chart energyChartConsumedTot;
    public Microcharts.Chart EnergyChartConsumedTot
    {
        get { return energyChartConsumedTot; }
        set { SetProperty(ref energyChartConsumedTot, value); }
    }
    private Microcharts.Chart energyChartConsumedGrid;
    public Microcharts.Chart EnergyChartConsumedGrid
    {
        get { return energyChartConsumedGrid; }
        set { SetProperty(ref energyChartConsumedGrid, value); }
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
    private string energyChartConsumedTotTitle;
    public string EnergyChartConsumedTotTitle
    {
        get { return energyChartConsumedTotTitle; }
        set { SetProperty(ref energyChartConsumedTotTitle, value); }
    }
    private string energyChartConsumedGridTitle;
    public string EnergyChartConsumedGridTitle
    {
        get { return energyChartConsumedGridTitle; }
        set { SetProperty(ref energyChartConsumedGridTitle, value); }
    }
}

