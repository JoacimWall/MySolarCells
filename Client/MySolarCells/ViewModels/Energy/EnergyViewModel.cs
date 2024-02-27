using Shiny.Jobs;
namespace MySolarCells.ViewModels.Energy;

public class EnergyViewModel : BaseViewModel
{
    private readonly IEnergyChartService energyChartService;
    private readonly IDataSyncService dataSyncService;
    private readonly MscDbContext mscDbContext;
    //private readonly IJobManager jobManager;
    public EnergyViewModel(IEnergyChartService energyChartService, IDataSyncService dataSyncService, MscDbContext mscDbContext) //,IJobManager jobManager
    {
        this.energyChartService = energyChartService;
        this.dataSyncService = dataSyncService;
        this.mscDbContext = mscDbContext;
        //this.jobManager = jobManager;
        this.ChartDataRequest = MySolarCellsGlobals.ChartDataRequest;
        //DateTime dateTime = new DateTime(2023, 4, 8);
        //long milliseconds = DateHelper.DateTimeToMillis(dateTime);
        //DateTime dattest = DateHelper.MillisToDateTime(1680926400000);



    }
    public ICommand SyncCommand => new Command(async () => await Sync());
    public ICommand ReloadGraphDataCommand => new Command(async () => await ReloadGraph());

    private async Task Sync()
    {
        using var dlg = DialogService.GetProgress(AppResources.Import_Data);
        await Task.Delay(200);
        var result = await this.dataSyncService.Sync();
        if (!result.WasSuccessful)
        {
            await DialogService.ShowAlertAsync(result.ErrorMessage, AppResources.My_Solar_Cells, AppResources.Ok);
        }
        else
        {
             DialogService.ShowToast(result.Model.Message);
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
            //await Sync();
            resultSeries = await energyChartService.GetChartData(ChartDataRequest);
        }
        if (!resultSeries.WasSuccessful)
        {
            await DialogService.ShowAlertAsync(AppResources.There_Is_No_Data_To_Show_On_This_Date, AppResources.My_Solar_Cells, AppResources.Ok);
            return false;
        }
        

       
        // ---------- Price Graph ---------------------------
        PriceChartTitle = resultSeries.Model.PriceChartTitle;
        //Price Buy
        PriceBuy.Clear();
        PriceChartPriceBuyTitle = resultSeries.Model.PriceBuyTile;
        Brush brushPriceBuy = resultSeries.Model.ChartSeriesPriceBuy.First().Color;
        ColorPriceBuy = resultSeries.Model.ChartSeriesPriceBuy.First().Color;
        PaletteBrushesPriceBuy.Add(brushPriceBuy);
        //if (resultSeries.Model.ChartSeriesPriceBuy.Any(x => x.Value.HasValue && x.Value.Value > 0))
            foreach (var item in resultSeries.Model.ChartSeriesPriceBuy)
                PriceBuy.Add(item);

        //Price Sell
        PriceSell.Clear();
        PriceChartPriceSellTitle = resultSeries.Model.PriceSellTile;
        Brush brushPriceSell = resultSeries.Model.ChartSeriesPriceSell.First().Color;
        ColorPriceSell = resultSeries.Model.ChartSeriesPriceSell.First().Color;
        PaletteBrushesPriceSell.Add(brushPriceSell);
        //if (resultSeries.Model.ChartSeriesPriceSell.Any(x => x.Value.HasValue && x.Value.Value > 0))
            foreach (var item in resultSeries.Model.ChartSeriesPriceSell)
                PriceSell.Add(item);



        // ---------- Production Graph ---------------------------
        ConsumtionChartTitle = resultSeries.Model.ConsumtionChartTitle;
        ProductionChartTitle = resultSeries.Model.ProductionChartTitle;


        //Production Sold
        DataSold.Clear();
        EnergyChartProdSoldTitle = resultSeries.Model.ProductionSoldTile;
        Brush brushProductionSold = resultSeries.Model.ChartSeriesProductionSold.First().Color;
        ColorSold = resultSeries.Model.ChartSeriesProductionSold.First().Color;
        PaletteBrushesProductionSold.Add(brushProductionSold);
        PaletteBrushesProductionSold.Add(brushPriceBuy);
        //if (resultSeries.Model.ChartSeriesProductionSold.Any(x => x.Value.HasValue && x.Value.Value > 0))
        foreach (var item in resultSeries.Model.ChartSeriesProductionSold)
                DataSold.Add(item);

        //Battery charge
        DataBatteryCharge.Clear();
        EnergyBatteryChargeChartTitle = resultSeries.Model.BatteryChargeTitle;
        Brush brushBatteryCharge = resultSeries.Model.ChartSeriesBatteryCharge.First().Color;
        ColorbatteryCharge = resultSeries.Model.ChartSeriesBatteryCharge.First().Color;
        PaletteBrushesBatteryCharge.Add(brushBatteryCharge); 
        //if (resultSeries.Model.ChartSeriesBatteryCharge.Any(x => x.Value.HasValue && x.Value.Value > 0))
            foreach (var item in resultSeries.Model.ChartSeriesBatteryCharge)
                DataBatteryCharge.Add(item);

        //----------Consumption Graph ------------------------------
        //Battery Used
        DataBatteryUsed.Clear();
        EnergyChartBatteryUsedTitle = resultSeries.Model.BatteryUsedTile;
        Brush brushBatteryUsed = resultSeries.Model.ChartSeriesBatteryUsed.First().Color;
        ColorbatteryUsed = resultSeries.Model.ChartSeriesBatteryUsed.First().Color;
        PaletteBrushesBatteryUsed.Add(brushBatteryUsed);
        //if (resultSeries.Model.ChartSeriesBatteryUsed.Any(x => x.Value.HasValue && x.Value.Value > 0))
            foreach (var item in resultSeries.Model.ChartSeriesBatteryUsed)
                DataBatteryUsed.Add(item);

        //Consumtion Grid
        DataPurchased.Clear();
        EnergyChartConsumedGridTitle = resultSeries.Model.ConsumedGridTile;
        Brush brushConsumedGrid = resultSeries.Model.ChartSeriesConsumtionGrid.First().Color;
        ColorPurchased = resultSeries.Model.ChartSeriesConsumtionGrid.First().Color;
        PaletteBrushesPurchased.Add(brushConsumedGrid);
       //if (resultSeries.Model.ChartSeriesConsumtionGrid.Any(x => x.Value.HasValue && x.Value.Value > 0))
            foreach (var item in resultSeries.Model.ChartSeriesConsumtionGrid)
                DataPurchased.Add(item);
        //-------- Production And Cosnumption Graph ------------------
        //Production Used
        DataUsed.Clear();
        EnergyChartProdUsedTitle = resultSeries.Model.ProductionUsedTile;
        Brush brushProdUsed = resultSeries.Model.ChartSeriesProductionUsed.First().Color;
        ColorUsed = resultSeries.Model.ChartSeriesProductionUsed.First().Color;
        PaletteBrushesProductionUsed.Add(brushProdUsed);
        //if (resultSeries.Model.ChartSeriesProductionUsed.Any(x => x.Value.HasValue && x.Value.Value > 0))
            foreach (var item in resultSeries.Model.ChartSeriesProductionUsed)
                DataUsed.Add(item);
        
        if (ChartDataRequest.ChartDataUnit == ChartDataUnit.kWh)
            ProductionChartXtitle = "kWh";
        else
            ProductionChartXtitle = "SEK";

        PriceChartXtitle = "SEK";
        IsRefreshing = false;
        return true;
    }
    public async override Task OnAppearingAsync()
    {
        this.ChartDataRequest = MySolarCellsGlobals.ChartDataRequest;
        await ReloadGraph(true);
        if (FirstTimeAppearing)
        {
           
        }
        



    }
    // ------------ Price Chart ------------------
    public IList<Brush> PaletteBrushesPriceBuy { get; set; } = new List<Brush>();
    public IList<Brush> PaletteBrushesPriceSell { get; set; } = new List<Brush>();

    private ObservableCollection<ChartEntry> priceBuy = new ObservableCollection<ChartEntry>();
    public ObservableCollection<ChartEntry> PriceBuy
    {
        get => priceBuy;
        set
        {
            SetProperty(ref priceBuy, value);

        }
    }
    private ObservableCollection<ChartEntry> priceSell = new ObservableCollection<ChartEntry>();
    public ObservableCollection<ChartEntry> PriceSell
    {
        get => priceSell;
        set
        {
            SetProperty(ref priceSell, value);

        }
    }


    // ------------ Energy Charts ---------------
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
    private HistorySimulate roiSimulate = new HistorySimulate();
    public HistorySimulate RoiSimulate
    {
        get { return roiSimulate; }
        set { SetProperty(ref roiSimulate, value); }
    }
    //private ChartDataRequest chartDataRequest = new ChartDataRequest();
    //public ChartDataRequest ChartDataRequest
    //{
    //    get { return chartDataRequest; }
    //    set { SetProperty(ref chartDataRequest, value); }
    //}

    private Color colorPriceBuy;
    public Color ColorPriceBuy
    {
        get { return colorPriceBuy; }
        set { SetProperty(ref colorPriceBuy, value); }
    }
    private Color colorPriceSell;
    public Color ColorPriceSell
    {
        get { return colorPriceSell; }
        set { SetProperty(ref colorPriceSell, value); }
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
    private string priceChartXtitle;
    public string PriceChartXtitle
    {
        get { return priceChartXtitle; }
        set { SetProperty(ref priceChartXtitle, value); }
    }
    private string priceChartPriceBuyTitle;
    public string PriceChartPriceBuyTitle
    {
        get { return priceChartPriceBuyTitle; }
        set { SetProperty(ref priceChartPriceBuyTitle, value); }
    }
    private string priceChartPriceSellTitle;
    public string PriceChartPriceSellTitle
    {
        get { return priceChartPriceSellTitle; }
        set { SetProperty(ref priceChartPriceSellTitle, value); }
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
    private string priceChartTitle;
    public string PriceChartTitle
    {
        get { return priceChartTitle; }
        set { SetProperty(ref priceChartTitle, value); }
    }
}

