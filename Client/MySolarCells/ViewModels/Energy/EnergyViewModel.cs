
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
        //GradientStopCollection prod = new GradientStopCollection();
        //prod.Add(new GradientStop(Color.FromArgb("#fa9702"), (float)0.1));
        //prod.Add(new GradientStop(Color.FromArgb("#c9841c"), (float)1.0));
        //LinearGradientBrush brushProduction1 = new LinearGradientBrush(prod);
        //brushProduction1.EndPoint = new Point(0, 1);
        //LinearGradientBrush brushProduction2 = Color.FromArgb("#489c2a");
        ColorSold = Color.FromArgb("#fa9702");
        ColorUsed = Color.FromArgb("#22a81b");
        ColorPurchased = Color.FromArgb("#1b25b3");
        BrushSold = Color.FromArgb("#fa9702");
        BrushUsed = Color.FromArgb("#22a81b");
        BrushPurchased = Color.FromArgb("#1b25b3");

        PaletteBrushesProductionSold.Add(BrushSold);
        PaletteBrushesProductionUsed.Add(BrushUsed);
        PaletteBrushesPurchased.Add(BrushPurchased);
    }
    public ICommand SyncCommand => new Command(async () => await Sync());
    public ICommand ReloadGraphDataCommand => new Command(async () => await ReloadGraph());

    public Brush BrushSold { get; set; }
    public Brush BrushUsed { get; set; }
    public Brush BrushPurchased { get; set; }
    public Color ColorSold { get; set; }
    public Color ColorUsed { get; set; }
    public Color ColorPurchased { get; set; }
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
            EnergyChartBatteryCharge = null;
            EnergyChartProdSold = null;
            EnergyChartProdUsed = null;
            EnergyChartBatteryUsed = null;
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

        //Battery charge
        EnergyBatteryChargeChartTitle = resultSeries.Model.ChartSeriesBatteryCharge[0].Name;
        if (resultSeries.Model.ChartSeriesBatteryCharge[0].Entries.Any(x => x.Value.HasValue && x.Value.Value > 0))
            EnergyChartBatteryCharge = new Microcharts.BarChart
            {
                Margin = 6,
                Entries = resultSeries.Model.ChartSeriesBatteryCharge[0].Entries,
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
            EnergyChartBatteryCharge = null;
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

        //Battery Used
        EnergyChartBatteryUsedTitle = resultSeries.Model.ChartSeriesBatteryUsed[0].Name;
        if (resultSeries.Model.ChartSeriesBatteryUsed[0].Entries.Any(x => x.Value.HasValue && x.Value.Value > 0))
            EnergyChartBatteryUsed = new Microcharts.BarChart
            {
                Margin = 6,
                Entries = resultSeries.Model.ChartSeriesBatteryUsed[0].Entries,
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
            EnergyChartBatteryUsed = null;
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


        ConsumtionChartTitle = resultSeries.Model.ConsumtionChartTitle;
        ProductionChartTitle = resultSeries.Model.ProductionChartTitle;
        DataSold.Clear();
        foreach (var item in resultSeries.Model.ChartSeriesProductionSold[0].Entries)
            DataSold.Add(item);

        DataBatteryCharge.Clear();
        foreach (var item in resultSeries.Model.ChartSeriesBatteryCharge[0].Entries)
            DataBatteryCharge.Add(item);

        DataUsed.Clear();
        foreach (var item in resultSeries.Model.ChartSeriesProductionUsed[0].Entries)
            DataUsed.Add(item);

        DataBatteryUsed.Clear();
        foreach (var item in resultSeries.Model.ChartSeriesBatteryUsed[0].Entries)
            DataBatteryUsed.Add(item);

        DataPurchased.Clear();
        foreach (var item in resultSeries.Model.ChartSeriesConsumtionGrid[0].Entries)
            DataPurchased.Add(item);
        
        if (ChartDataRequest.ChartDataUnit == ChartDataUnit.kWh)
            ProductionChartXtitle = "kWh";
        else
            ProductionChartXtitle = "SEK";
        return true;
    }
    public async override Task OnAppearingAsync()
    {
        if (FirstTimeAppearing)
            await ReloadGraph(true);



    }

    public IList<Brush> PaletteBrushesProductionSold { get; set; } = new List<Brush>();
    public IList<Brush> PaletteBrushesProductionUsed { get; set; } = new List<Brush>();
    public IList<Brush> PaletteBrushesPurchased { get; set; } = new List<Brush>();
   

    private ObservableCollection<ChartEntry> dataSold = new ObservableCollection<ChartEntry>();
    public ObservableCollection<ChartEntry> DataSold
    {
        get => dataSold;
        set
        {
            SetProperty(ref dataSold, value);

        }
    }
    private ObservableCollection<ChartEntry> dataBatteryCharge = new ObservableCollection<ChartEntry>();
    public ObservableCollection<ChartEntry> DataBatteryCharge
    {
        get => dataBatteryCharge;
        set
        {
            SetProperty(ref dataBatteryCharge, value);

        }
    }
    private ObservableCollection<ChartEntry> dataUsed = new ObservableCollection<ChartEntry>();
    public ObservableCollection<ChartEntry> DataUsed
    {
        get => dataUsed;
        set
        {
            SetProperty(ref dataUsed, value);

        }
    }
    private ObservableCollection<ChartEntry> dataBatteryUsed = new ObservableCollection<ChartEntry>();
    public ObservableCollection<ChartEntry> DataBatteryUsed
    {
        get => dataBatteryUsed;
        set
        {
            SetProperty(ref dataBatteryUsed, value);

        }
    }
    private ObservableCollection<ChartEntry> dataPurchased = new ObservableCollection<ChartEntry>();
    public ObservableCollection<ChartEntry> DataPurchased
    {
        get => dataPurchased;
        set
        {
            SetProperty(ref dataPurchased, value);

        }
    }

    private ChartDataRequest chartDataRequest = new ChartDataRequest();
    public ChartDataRequest ChartDataRequest
    {
        get { return chartDataRequest; }
        set { SetProperty(ref chartDataRequest, value); }
    }

    private Microcharts.Chart energyChartBatteryCharge;
    public Microcharts.Chart EnergyChartBatteryCharge
    {
        get { return energyChartBatteryCharge; }
        set { SetProperty(ref energyChartBatteryCharge, value); }
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
    private Microcharts.Chart energyChartBatteryUsed;
    public Microcharts.Chart EnergyChartBatteryUsed
    {
        get { return energyChartBatteryUsed; }
        set { SetProperty(ref energyChartBatteryUsed, value); }
    }
    private Microcharts.Chart energyChartConsumedGrid;
    public Microcharts.Chart EnergyChartConsumedGrid
    {
        get { return energyChartConsumedGrid; }
        set { SetProperty(ref energyChartConsumedGrid, value); }
    }

    private string energyBatteryChargeChartTitle;
    public string EnergyBatteryChargeChartTitle
    {
        get { return energyBatteryChargeChartTitle; }
        set { SetProperty(ref energyBatteryChargeChartTitle, value); }
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
    private string energyChartBatteryUsedTitle;
    public string EnergyChartBatteryUsedTitle
    {
        get { return energyChartBatteryUsedTitle; }
        set { SetProperty(ref energyChartBatteryUsedTitle, value); }
    }
    private string energyChartConsumedGridTitle;
    public string EnergyChartConsumedGridTitle
    {
        get { return energyChartConsumedGridTitle; }
        set { SetProperty(ref energyChartConsumedGridTitle, value); }
    }
    private string productionChartXtitle;
    public string ProductionChartXtitle
    {
        get { return productionChartXtitle; }
        set { SetProperty(ref productionChartXtitle, value); }
    }
    private string productionChartTitle;
    public string ProductionChartTitle
    {
        get { return productionChartTitle; }
        set { SetProperty(ref productionChartTitle, value); }
    }
    private string consumtionChartTitle;
    public string ConsumtionChartTitle
    {
        get { return consumtionChartTitle; }
        set { SetProperty(ref consumtionChartTitle, value); }
    }
}

