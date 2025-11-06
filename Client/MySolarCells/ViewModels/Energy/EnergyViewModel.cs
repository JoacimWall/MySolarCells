namespace MySolarCells.ViewModels.Energy;

public class EnergyViewModel : BaseViewModel
{
    private readonly IEnergyChartService energyChartService;
    private readonly IDataSyncService dataSyncService;
    private readonly IThemeService themeService;

    public EnergyViewModel(IEnergyChartService energyChartService, IDataSyncService dataSyncService,
        IDialogService dialogService, IAnalyticsService analyticsService, IInternetConnectionService internetConnectionService, ILogService logService,
        ISettingsService settingsService, IHomeService homeService, IThemeService themeService) : base(dialogService, analyticsService, internetConnectionService,
        logService, settingsService, homeService)
    {
        this.energyChartService = energyChartService;
        this.dataSyncService = dataSyncService;
        this.themeService = themeService;
        ChartDataRequest = homeService.CurrentChartDataRequest();

        // Subscribe to theme changes
        themeService.ThemeChanged += OnThemeChanged;
    }

    private async void OnThemeChanged(object? sender, AppTheme theme)
    {
        // Reload graph with new theme colors
        await ReloadGraph();
    }

    public ICommand SyncCommand => new Command(async () => await Sync());
    public ICommand ReloadGraphDataCommand => new Command(async () => await ReloadGraph());

    private async Task Sync()
    {
        using var dlg = (ProgressDialog)DialogService.GetProgress(AppResources.Import_Data);
        await Task.Delay(200);
        var result = await dataSyncService.Sync();
        if (result is { WasSuccessful: false })
        {
            await DialogService.ShowAlertAsync(result.ErrorMessage, AppResources.My_Solar_Cells, AppResources.Ok);
        }
        else
        {
            if (result.Model != null) DialogService.ShowToast(result.Model.Message);
        }

        await ReloadGraph();
    }

    private async Task<bool> ReloadGraph(bool fromOnAppearing = false)
    {
        using var dlg = (ProgressDialog)DialogService.GetProgress("");
        var resultSeries = await energyChartService.GetChartData(ChartDataRequest);
        //If there is no data try to fetch
        if (fromOnAppearing && resultSeries is { WasSuccessful: false, ErrorCode: ErrorCode.NoEnergyEntryOnCurrentDate })
        {
            //await Sync();
            resultSeries = await energyChartService.GetChartData(ChartDataRequest);
        }

        if (!resultSeries.WasSuccessful || resultSeries.Model == null)
        {
            await DialogService.ShowAlertAsync(AppResources.There_Is_No_Data_To_Show_On_This_Date,
                AppResources.My_Solar_Cells, AppResources.Ok);
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
        ConsumptionChartTitle = resultSeries.Model.ConsumtionChartTitle;
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
        ColorBatteryCharge = resultSeries.Model.ChartSeriesBatteryCharge.First().Color;
        PaletteBrushesBatteryCharge.Add(brushBatteryCharge);
        //if (resultSeries.Model.ChartSeriesBatteryCharge.Any(x => x.Value.HasValue && x.Value.Value > 0))
        foreach (var item in resultSeries.Model.ChartSeriesBatteryCharge)
            DataBatteryCharge.Add(item);

        //----------Consumption Graph ------------------------------
        //Battery Used
        DataBatteryUsed.Clear();
        EnergyChartBatteryUsedTitle = resultSeries.Model.BatteryUsedTile;
        Brush brushBatteryUsed = resultSeries.Model.ChartSeriesBatteryUsed.First().Color;
        ColorBatteryUsed = resultSeries.Model.ChartSeriesBatteryUsed.First().Color;
        PaletteBrushesBatteryUsed.Add(brushBatteryUsed);
        //if (resultSeries.Model.ChartSeriesBatteryUsed.Any(x => x.Value.HasValue && x.Value.Value > 0))
        foreach (var item in resultSeries.Model.ChartSeriesBatteryUsed)
            DataBatteryUsed.Add(item);

        //Consumption Grid
        DataPurchased.Clear();
        EnergyChartConsumedGridTitle = resultSeries.Model.ConsumedGridTile;
        Brush brushConsumedGrid = resultSeries.Model.ChartSeriesConsumptionGrid.First().Color;
        ColorPurchased = resultSeries.Model.ChartSeriesConsumptionGrid.First().Color;
        PaletteBrushesPurchased.Add(brushConsumedGrid);
        foreach (var item in resultSeries.Model.ChartSeriesConsumptionGrid)
            DataPurchased.Add(item);
        //-------- Production And Consumption Graph ------------------
        //Production Used
        DataUsed.Clear();
        EnergyChartProdUsedTitle = resultSeries.Model.ProductionUsedTile;
        Brush brushProdUsed = resultSeries.Model.ChartSeriesProductionUsed.First().Color;
        ColorUsed = resultSeries.Model.ChartSeriesProductionUsed.First().Color;
        PaletteBrushesProductionUsed.Add(brushProdUsed);
        //if (resultSeries.Model.ChartSeriesProductionUsed.Any(x => x.Value.HasValue && x.Value.Value > 0))
        foreach (var item in resultSeries.Model.ChartSeriesProductionUsed)
            DataUsed.Add(item);

        ProductionChartXTitle = ChartDataRequest.ChartDataUnit == ChartDataUnit.KWh ? "kWh" : "SEK";

        PriceChartXTitle = "SEK";
        IsRefreshing = false;
        return true;
    }

    public override async Task OnAppearingAsync()
    {
        ChartDataRequest = HomeService.CurrentChartDataRequest(); ;
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
        set => SetProperty(ref priceBuy, value);
    }

    private ObservableCollection<ChartEntry> priceSell = new ObservableCollection<ChartEntry>();

    public ObservableCollection<ChartEntry> PriceSell
    {
        get => priceSell;
        set => SetProperty(ref priceSell, value);
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
        set => SetProperty(ref dataSold, value);
    }

    private ObservableCollection<ChartEntry> dataBatteryCharge = new ObservableCollection<ChartEntry>();

    public ObservableCollection<ChartEntry> DataBatteryCharge
    {
        get => dataBatteryCharge;
        set => SetProperty(ref dataBatteryCharge, value);
    }

    private ObservableCollection<ChartEntry> dataUsed = new ObservableCollection<ChartEntry>();

    public ObservableCollection<ChartEntry> DataUsed
    {
        get => dataUsed;
        set => SetProperty(ref dataUsed, value);
    }

    private ObservableCollection<ChartEntry> dataBatteryUsed = new ObservableCollection<ChartEntry>();

    public ObservableCollection<ChartEntry> DataBatteryUsed
    {
        get => dataBatteryUsed;
        set => SetProperty(ref dataBatteryUsed, value);
    }

    private ObservableCollection<ChartEntry> dataPurchased = new ObservableCollection<ChartEntry>();

    public ObservableCollection<ChartEntry> DataPurchased
    {
        get => dataPurchased;
        set => SetProperty(ref dataPurchased, value);
    }


    private Color colorPriceBuy = Colors.Transparent;

    public Color ColorPriceBuy
    {
        get => colorPriceBuy;
        set => SetProperty(ref colorPriceBuy, value);
    }

    private Color colorPriceSell = Colors.Transparent;

    public Color ColorPriceSell
    {
        get => colorPriceSell;
        set => SetProperty(ref colorPriceSell, value);
    }

    private Color colorSold = Colors.Transparent;

    public Color ColorSold
    {
        get => colorSold;
        set => SetProperty(ref colorSold, value);
    }

    private Color colorUsed = Colors.Transparent;

    public Color ColorUsed
    {
        get => colorUsed;
        set => SetProperty(ref colorUsed, value);
    }

    private Color colorPurchased = Colors.Transparent;

    public Color ColorPurchased
    {
        get => colorPurchased;
        set => SetProperty(ref colorPurchased, value);
    }

    private Color colorBatteryUsed = Colors.Transparent;

    public Color ColorBatteryUsed
    {
        get => colorBatteryUsed;
        set => SetProperty(ref colorBatteryUsed, value);
    }

    private Color colorBatteryCharge = Colors.Transparent;

    public Color ColorBatteryCharge
    {
        get => colorBatteryCharge;
        set => SetProperty(ref colorBatteryCharge, value);
    }

    private string energyBatteryChargeChartTitle = "";

    public string EnergyBatteryChargeChartTitle
    {
        get => energyBatteryChargeChartTitle;
        set => SetProperty(ref energyBatteryChargeChartTitle, value);
    }

    private string energyChartProdSoldTitle = "";

    public string EnergyChartProdSoldTitle
    {
        get => energyChartProdSoldTitle;
        set => SetProperty(ref energyChartProdSoldTitle, value);
    }

    private string energyChartProdUsedTitle = "";

    public string EnergyChartProdUsedTitle
    {
        get => energyChartProdUsedTitle;
        set => SetProperty(ref energyChartProdUsedTitle, value);
    }

    private string energyChartBatteryUsedTitle = "";

    public string EnergyChartBatteryUsedTitle
    {
        get => energyChartBatteryUsedTitle;
        set => SetProperty(ref energyChartBatteryUsedTitle, value);
    }

    private string energyChartConsumedGridTitle = "";

    public string EnergyChartConsumedGridTitle
    {
        get => energyChartConsumedGridTitle;
        set => SetProperty(ref energyChartConsumedGridTitle, value);
    }

    private string productionChartXTitle = "";

    public string ProductionChartXTitle
    {
        get => productionChartXTitle;
        set => SetProperty(ref productionChartXTitle, value);
    }

    private string priceChartXTitle = "";

    public string PriceChartXTitle
    {
        get => priceChartXTitle;
        set => SetProperty(ref priceChartXTitle, value);
    }

    private string priceChartPriceBuyTitle = "";

    public string PriceChartPriceBuyTitle
    {
        get => priceChartPriceBuyTitle;
        set => SetProperty(ref priceChartPriceBuyTitle, value);
    }

    private string priceChartPriceSellTitle = "";

    public string PriceChartPriceSellTitle
    {
        get => priceChartPriceSellTitle;
        set => SetProperty(ref priceChartPriceSellTitle, value);
    }


    private string productionChartTitle = "";

    public string ProductionChartTitle
    {
        get => productionChartTitle;
        set => SetProperty(ref productionChartTitle, value);
    }

    private string consumptionChartTitle = "";

    public string ConsumptionChartTitle
    {
        get => consumptionChartTitle;
        set => SetProperty(ref consumptionChartTitle, value);
    }

    private string priceChartTitle = "";

    public string PriceChartTitle
    {
        get => priceChartTitle;
        set => SetProperty(ref priceChartTitle, value);
    }
}