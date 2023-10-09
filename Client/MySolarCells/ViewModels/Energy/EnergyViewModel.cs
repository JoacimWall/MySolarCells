
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


        ConsumtionChartTitle = resultSeries.Model.ConsumtionChartTitle;
        ProductionChartTitle = resultSeries.Model.ProductionChartTitle;

        // ---------- Production Graph ---------------------------
        //Production Sold
        DataSold.Clear();
        EnergyChartProdSoldTitle = resultSeries.Model.ProductionSoldTile;
        Brush brushProductionSold = resultSeries.Model.ChartSeriesProductionSold.First().Color;
        ColorSold = resultSeries.Model.ChartSeriesProductionSold.First().Color;
        PaletteBrushesProductionSold.Add(brushProductionSold);
        if (resultSeries.Model.ChartSeriesProductionSold.Any(x => x.Value.HasValue && x.Value.Value > 0))
            foreach (var item in resultSeries.Model.ChartSeriesProductionSold)
                DataSold.Add(item);

        //Battery charge
        DataBatteryCharge.Clear();
        EnergyBatteryChargeChartTitle = resultSeries.Model.BatteryChargeTitle;
        Brush brushBatteryCharge = resultSeries.Model.ChartSeriesBatteryCharge.First().Color;
        ColorbatteryCharge = resultSeries.Model.ChartSeriesBatteryCharge.First().Color;
        PaletteBrushesBatteryCharge.Add(brushBatteryCharge); 
        if (resultSeries.Model.ChartSeriesBatteryCharge.Any(x => x.Value.HasValue && x.Value.Value > 0))
            foreach (var item in resultSeries.Model.ChartSeriesBatteryCharge)
                DataBatteryCharge.Add(item);

        //----------Consumption Graph ------------------------------
        //Battery Used
        DataBatteryUsed.Clear();
        EnergyChartBatteryUsedTitle = resultSeries.Model.BatteryUsedTile;
        Brush brushBatteryUsed = resultSeries.Model.ChartSeriesBatteryUsed.First().Color;
        ColorbatteryUsed = resultSeries.Model.ChartSeriesBatteryUsed.First().Color;
        PaletteBrushesBatteryUsed.Add(brushBatteryUsed);
        if (resultSeries.Model.ChartSeriesBatteryUsed.Any(x => x.Value.HasValue && x.Value.Value > 0))
            foreach (var item in resultSeries.Model.ChartSeriesBatteryUsed)
                DataBatteryUsed.Add(item);

        //Consumtion Grid
        DataPurchased.Clear();
        EnergyChartConsumedGridTitle = resultSeries.Model.ConsumedGridTile;
        Brush brushConsumedGrid = resultSeries.Model.ChartSeriesConsumtionGrid.First().Color;
        ColorPurchased = resultSeries.Model.ChartSeriesConsumtionGrid.First().Color;
        PaletteBrushesPurchased.Add(brushConsumedGrid);
        if (resultSeries.Model.ChartSeriesConsumtionGrid.Any(x => x.Value.HasValue && x.Value.Value > 0))
            foreach (var item in resultSeries.Model.ChartSeriesConsumtionGrid)
                DataPurchased.Add(item);
        //-------- Production And Cosnumption Graph ------------------
        //Production Used
        DataUsed.Clear();
        EnergyChartProdUsedTitle = resultSeries.Model.ProductionUsedTile;
        Brush brushProdUsed = resultSeries.Model.ChartSeriesProductionUsed.First().Color;
        ColorUsed = resultSeries.Model.ChartSeriesProductionUsed.First().Color;
        PaletteBrushesProductionUsed.Add(brushProdUsed);
        if (resultSeries.Model.ChartSeriesProductionUsed.Any(x => x.Value.HasValue && x.Value.Value > 0))
            foreach (var item in resultSeries.Model.ChartSeriesProductionUsed)
                DataUsed.Add(item);








       

       
        
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
    public IList<Brush> PaletteBrushesBatteryCharge { get; set; } = new List<Brush>();
    public IList<Brush> PaletteBrushesBatteryUsed { get; set; } = new List<Brush>();

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
    private Color colorSold;
    public Color ColorSold
    {
        get { return colorSold; }
        set { SetProperty(ref colorSold, value); }
    }
    private Color colorUsed;
    public Color ColorUsed
    {
        get { return colorUsed; }
        set { SetProperty(ref colorUsed, value); }
    }
    private Color colorPurchased;
    public Color ColorPurchased
    {
        get { return colorPurchased; }
        set { SetProperty(ref colorPurchased, value); }
    }
    private Color colorbatteryUsed;
    public Color ColorbatteryUsed
    {
        get { return colorbatteryUsed; }
        set { SetProperty(ref colorbatteryUsed, value); }
    }
    private Color colorbatteryCharge;
    public Color ColorbatteryCharge
    {
        get { return colorbatteryCharge; }
        set { SetProperty(ref colorbatteryCharge, value); }
    }
    //private Chart energyChartBatteryCharge;
    //public .Chart EnergyChartBatteryCharge
    //{
    //    get { return energyChartBatteryCharge; }
    //    set { SetProperty(ref energyChartBatteryCharge, value); }
    //}
    //private Microcharts.Chart energyChartProdSold;
    //public Microcharts.Chart EnergyChartProdSold
    //{
    //    get { return energyChartProdSold; }
    //    set { SetProperty(ref energyChartProdSold, value); }
    //}
    //private Microcharts.Chart energyChartProdUsed;
    //public Microcharts.Chart EnergyChartProdUsed
    //{
    //    get { return energyChartProdUsed; }
    //    set { SetProperty(ref energyChartProdUsed, value); }
    //}
    //private Microcharts.Chart energyChartBatteryUsed;
    //public Microcharts.Chart EnergyChartBatteryUsed
    //{
    //    get { return energyChartBatteryUsed; }
    //    set { SetProperty(ref energyChartBatteryUsed, value); }
    //}
    //private Microcharts.Chart energyChartConsumedGrid;
    //public Microcharts.Chart EnergyChartConsumedGrid
    //{
    //    get { return energyChartConsumedGrid; }
    //    set { SetProperty(ref energyChartConsumedGrid, value); }
    //}

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

