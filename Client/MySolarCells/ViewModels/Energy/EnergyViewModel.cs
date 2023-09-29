
using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace MySolarCells.ViewModels.Energy;

public class EnergyViewModel : BaseViewModel
{
    private readonly IEnergyChartService energyChartService;
    private readonly ITibberService tibberService;
    private IInverterServiceInterface inverterService;
    public EnergyViewModel(IEnergyChartService energyChartService, ITibberService tibberService)
    {
        this.energyChartService = energyChartService;
        this.tibberService = tibberService;
        using var dbContext = new MscDbContext();
        this.inverterService = ServiceHelper.GetInverterService(dbContext.Inverter.First().InverterTyp);
    }
    public ICommand SyncCommand => new Command(async () => await Sync());
    public ICommand TodayCommand => new Command(async () => await TodayView());
    public ICommand DayCommand => new Command(async () => await DayView());
    public ICommand WeekCommand => new Command(async () => await WeekView());
    public ICommand MonthCommand => new Command(async () => await MonthView());
    public ICommand YearCommand => new Command(async () => await YearView());
    public ICommand BackCommand => new Command(async () => await BackView());
    public ICommand ForwardCommand => new Command(async () => await ForwardView());

    public ICommand ShowKwhCommand => new Command(async () => await ShowKwhViews());
    public ICommand ShowCurrencyCommand => new Command(async () => await ShowCurrencyViews());

   
    private async Task BackView()
    {
        switch (ChartDataRequest.ChartDataRange)
        {
            case ChartDataRange.Today:
            case ChartDataRange.Day:
                ChartDataRequest.TimeStamp = ChartDataRequest.TimeStamp.AddDays(-1);
                if (ChartDataRequest.TimeStamp.Date == DateTime.Now.Date)
                    ChartDataRequest.ChartDataRange = ChartDataRange.Today;
                else
                    ChartDataRequest.ChartDataRange = ChartDataRange.Day;
                break;
            case ChartDataRange.Week:
                ChartDataRequest.TimeStamp = ChartDataRequest.TimeStamp.AddDays(-7);
                break;

            case ChartDataRange.Month:
                ChartDataRequest.TimeStamp = ChartDataRequest.TimeStamp.AddMonths(-1);
                break;

            case ChartDataRange.Year:
                ChartDataRequest.TimeStamp = ChartDataRequest.TimeStamp.AddYears(-1);
                break;

        }
        await ReloadGraph();
    }
    private async Task ForwardView()
    {

        switch (ChartDataRequest.ChartDataRange)
        {
            case ChartDataRange.Today:
            case ChartDataRange.Day:
                ChartDataRequest.TimeStamp = ChartDataRequest.TimeStamp.AddDays(1);
                if (ChartDataRequest.TimeStamp.Date == DateTime.Now.Date)
                    ChartDataRequest.ChartDataRange = ChartDataRange.Today;
                else
                    ChartDataRequest.ChartDataRange = ChartDataRange.Day;
                break;
            case ChartDataRange.Week:
                ChartDataRequest.TimeStamp = ChartDataRequest.TimeStamp.AddDays(7);
                break;

            case ChartDataRange.Month:
                ChartDataRequest.TimeStamp = ChartDataRequest.TimeStamp.AddMonths(1);
                break;

            case ChartDataRange.Year:
                ChartDataRequest.TimeStamp = ChartDataRequest.TimeStamp.AddYears(1);
                break;

        }
        await ReloadGraph();
    }
    private async Task<bool> TodayView()
    {
        ChartDataRequest.TimeStamp = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
        ChartDataRequest.ChartDataRange = ChartDataRange.Today;
        await ReloadGraph();
        return true;
    }
    private async Task<bool> DayView()
    {
        ChartDataRequest.ChartDataRange = ChartDataRange.Day;
        await ReloadGraph();
        return true;
    }
    private async Task<bool> WeekView()
    {
        ChartDataRequest.ChartDataRange = ChartDataRange.Week;
        await ReloadGraph();
        return true;
    }
    private async Task<bool> MonthView()
    {
        ChartDataRequest.ChartDataRange = ChartDataRange.Month;
        await ReloadGraph();
        return true;
    }
    private async Task<bool> YearView()
    {
        ChartDataRequest.ChartDataRange = ChartDataRange.Year;
        await ReloadGraph();
        return true;
    }
    private async Task ShowCurrencyViews()
    {
        ChartDataRequest.ChartDataUnit = ChartDataUnit.Currency;
        await ReloadGraph();
    }
    private async Task ShowKwhViews()
    {
        ChartDataRequest.ChartDataUnit = ChartDataUnit.kWh;
        await ReloadGraph();
    }

    private void CalculateProgress(long completed, long total)
    {
        var comp = Convert.ToDouble(completed);
        var tot = Convert.ToDouble(total);
        var percentage = comp / tot;
        //UploadProgress.ProgressTo(percentage, 100, Easing.Linear);
        //ProgressProcent = (float)percentage * 100;
        //ProgressSubStatus = "saved rows " + completed.ToString();
    }

    private async Task Sync()
    {
        using var dlg = DialogService.GetProgress("");
        using var dbContext = new MscDbContext();
        //Get last Sync Time
        var lastSyncTime = dbContext.Energy.Where(x => x.PurchasedSynced == true).OrderByDescending(o => o.Timestamp).First();
        var difference = DateTime.Now - lastSyncTime.Timestamp;

        var days = difference.Days;
        var hours = difference.Hours;
        var totalhours = (days * 24) + hours;

        var progress = new Progress<int>(currentDay =>
        {
            CalculateProgress(currentDay, totalhours);
        });

        await Task.Delay(200);
        //keepUploading = true;

        //ShowProgressStatus = true;
        //ProgressStatus = "Import consumation and sold production.";
        //ProgressSubStatus = "saved rows 0";
        await Task.Delay(200);
        var result = await this.tibberService.SyncConsumptionAndProductionFirstTime(lastSyncTime.Timestamp, progress, 0);
        if (!result)
        {

            await DialogService.ShowAlertAsync("Error import consumation and sold production.", AppResources.My_Solar_Cells, AppResources.Ok);
            return;
        }


        //ShowProgressStatus = true;
        //ProgressStatus = "Import solar own use and calculate profit.";
        //ProgressSubStatus = "saved rows 0";
        await Task.Delay(200);

        // var inverter = await dbContext.Inverter.FirstOrDefaultAsync(x => x.HomeId == MySolarCellsGlobals.SelectedHome.HomeId);
        lastSyncTime = dbContext.Energy.Where(x => x.ProductionOwnUseSynced == false).OrderByDescending(o => o.Timestamp).First();
        var differenceInverter = DateTime.Now - lastSyncTime.Timestamp;

        var daysInv = differenceInverter.Days;
        var hoursInv = differenceInverter.Hours;
        var totalhoursInv = (daysInv * 24) + hoursInv;
        progress = new Progress<int>(currentDay =>
        {
            CalculateProgress(currentDay, totalhoursInv);
        });
        var resultInverter = await this.inverterService.SyncProductionOwnUse(lastSyncTime.Timestamp, progress, 0);
        if (!resultInverter)
        {
            await DialogService.ShowAlertAsync("Error import solar own use and calculate profit", AppResources.My_Solar_Cells, AppResources.Ok);
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
            EnergyChartProdConsumed = null;
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
        List<Microcharts.ChartSerie> tests = new List<Microcharts.ChartSerie>();
        tests.Add(resultSeries.Model.ChartSeries[1]);
        tests.Add(resultSeries.Model.ChartSeries[2]);
        EnergyChartProdTotTitle = resultSeries.Model.ChartSeries[0].Name;
        if (resultSeries.Model.ChartSeries[0].Entries.Any(x => x.Value.HasValue && x.Value.Value > 0))
            EnergyChartProdTot = new Microcharts.BarChart
            {
                
                Entries = resultSeries.Model.ChartSeries[0].Entries,
                CornerRadius = 6,
                ShowYAxisLines = true,
                ShowYAxisText = true,
                YAxisTextPaint = YAxesPaint,
                LabelTextSize = 24,
                SerieLabelTextSize = 24,
                ValueLabelTextSize = 24,
                MaxValue = resultSeries.Model.MaxValueYaxes
            };
        else
            EnergyChartProdTot = null;

        EnergyChartProdSoldTitle = resultSeries.Model.ChartSeries[1].Name;
        if (resultSeries.Model.ChartSeries[1].Entries.Any(x => x.Value.HasValue && x.Value.Value > 0))
            EnergyChartProdSold = new Microcharts.BarChart
            {
                Entries = resultSeries.Model.ChartSeries[1].Entries,
                CornerRadius = 6,
                ShowYAxisLines = true,
                ShowYAxisText = true,
                YAxisTextPaint = YAxesPaint,
                LabelTextSize = 24,
                SerieLabelTextSize = 24,
                ValueLabelTextSize = 24,
                MaxValue = resultSeries.Model.MaxValueYaxes

            };
        else
            EnergyChartProdSold = null;

        EnergyChartProdUsedTitle = resultSeries.Model.ChartSeries[2].Name;
        if (resultSeries.Model.ChartSeries[2].Entries.Any(x => x.Value.HasValue && x.Value.Value > 0))
            EnergyChartProdUsed = new Microcharts.BarChart
            {
                Series = tests,
                CornerRadius = 6,
                ShowYAxisLines = true,
                ShowYAxisText = true,
                YAxisTextPaint = YAxesPaint,
                LabelTextSize = 24,
                SerieLabelTextSize = 24,
                ValueLabelTextSize = 24,
                MaxValue = resultSeries.Model.MaxValueYaxes

            };
        else
            EnergyChartProdUsed = null;

        EnergyChartProdConsumedTitle = resultSeries.Model.ChartSeries[3].Name;
        if (resultSeries.Model.ChartSeries[3].Entries.Any(x => x.Value.HasValue && x.Value.Value > 0))
            EnergyChartProdConsumed = new Microcharts.BarChart
            {
                Entries = resultSeries.Model.ChartSeries[3].Entries,
                CornerRadius = 6,
                ShowYAxisLines = true,
                ShowYAxisText = true,
                YAxisTextPaint = YAxesPaint,
                LabelTextSize = 24,
                SerieLabelTextSize = 24,
                ValueLabelTextSize = 24,
                MaxValue = resultSeries.Model.MaxValueYaxes

            };
        else
            EnergyChartProdConsumed = null;
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
    private Microcharts.Chart energyChartProdConsumed;
    public Microcharts.Chart EnergyChartProdConsumed
    {
        get { return energyChartProdConsumed; }
        set { SetProperty(ref energyChartProdConsumed, value); }
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

