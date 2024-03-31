namespace MySolarCells.Services;
public interface IEnergyChartService
{ 
    Task<Result<ChartDataResult>> GetChartData(ChartDataRequest chartDataRequest);
}
public class EnergyChartService : IEnergyChartService
{
    private readonly IHistoryDataService roiService;
    private readonly MscDbContext mscDbContext;
    public EnergyChartService(IHistoryDataService roiService, MscDbContext mscDbContext)
    {
        this.roiService = roiService;
        this.mscDbContext = mscDbContext;
    }
    public async Task<Result<ChartDataResult>> GetChartData(ChartDataRequest chartDataRequest)
    {
        ChartDataResult result = new ChartDataResult();
        string entryLabel = string.Empty;
        var calcParameter = await mscDbContext.EnergyCalculationParameter.FirstAsync();

        // ------ Create 4  ---------------
        //Production Sold
        List<ChartEntry> listProductionSold = new List<ChartEntry>();
        Color soldColor = Color.Parse("#640abf");
        //Production used
        List<ChartEntry> listProductionUsed = new List<ChartEntry>();
        Color usedColor = Color.Parse("#46a80d");

        //Consumption
        List<ChartEntry> listCunsumtionGrid = new List<ChartEntry>();
        Color consumGridColor = Color.Parse("#bf2522");
        //batteryUsedExist
        List<ChartEntry> listBatteryUsed = new List<ChartEntry>();
        Color batteryUsedColor = Color.Parse("#2685e3");
        //BatteryChargeExist
        List<ChartEntry> listBatteryCharge = new List<ChartEntry>();
        Color batteryChargeColor = Color.Parse("#2685e3");

        //Price buy
        List<ChartEntry> lisPriceBuy = new List<ChartEntry>();
        Color priceBuyColor = Color.Parse("#640abf");
        //price Sel
        List<ChartEntry> lisPriceSold = new List<ChartEntry>();
        Color priceSellColor = Color.Parse("#46a80d");

        int devPrice = 0;
        var dataRows = await mscDbContext.Energy.Where(x => x.Timestamp >= chartDataRequest.FilterStart && x.Timestamp < chartDataRequest.FilterEnd).ToListAsync();
        if (dataRows.Count == 0)
            return new Result<ChartDataResult>("No records", ErrorCode.NoEnergyEntryOnCurrentDate);
        foreach (var item in dataRows)
        {
            switch (chartDataRequest.ChartDataRange)
            {
                case ChartDataRange.Today:
                case ChartDataRange.Day:
                    entryLabel = item.Timestamp.ToString("HH:mm");
                    devPrice = 0;
                    break;
                case ChartDataRange.Week:
                    entryLabel = item.Timestamp.ToString("ddd");
                    devPrice = 24;
                    break;
                case ChartDataRange.Month:
                    entryLabel = item.Timestamp.ToString("dd");
                    devPrice = 24;
                    break;
                case ChartDataRange.Year:
                    entryLabel = item.Timestamp.ToString("MMM");
                    devPrice = 24 * 30;
                    break;
            }

            //Price buy
            var pricebuyExist = lisPriceBuy.FirstOrDefault(x => x.Label == entryLabel);
            if (pricebuyExist == null)
                lisPriceBuy.Add(new ChartEntry { Value = Convert.ToSingle(item.UnitPriceBuy + calcParameter.TransferFee + calcParameter.EnergyTax), Color = priceBuyColor, Label = entryLabel });
            else
            {
                pricebuyExist.Value = pricebuyExist.Value + Convert.ToSingle(item.UnitPriceBuy + calcParameter.TransferFee + calcParameter.EnergyTax);
            }
            //Price Sell
            var priceSoldExist = lisPriceSold.FirstOrDefault(x => x.Label == entryLabel);
            if (priceSoldExist == null)
                lisPriceSold.Add(new ChartEntry { Value = Convert.ToSingle(item.UnitPriceSold + calcParameter.ProdCompensationElectricityLowLoad + calcParameter.TaxReduction), Color = priceSellColor, Label = entryLabel });
            else
            {
                priceSoldExist.Value = priceSoldExist.Value + Convert.ToSingle(item.UnitPriceBuy + calcParameter.TransferFee + calcParameter.EnergyTax);
            }
            //TODO:All Profit that used in graph is base on Spotprise should also use fixed price if that is in calc params. 
            //Production Sold
            var prodSoldExist = listProductionSold.FirstOrDefault(x => x.Label == entryLabel);
            if (prodSoldExist == null)
                listProductionSold.Add(new ChartEntry { Value = chartDataRequest.ChartDataUnit == ChartDataUnit.KWh ? Convert.ToSingle(item.ProductionSold) : Convert.ToSingle(item.ProductionSoldProfit + (item.ProductionSold * (calcParameter.ProdCompensationElectricityLowLoad + calcParameter.TaxReduction))), Color = soldColor, Label = entryLabel });
            else
            {
                prodSoldExist.Value = prodSoldExist.Value + (chartDataRequest.ChartDataUnit == ChartDataUnit.KWh ? Convert.ToSingle(item.ProductionSold) : Convert.ToSingle(item.ProductionSoldProfit + (item.ProductionSold * (calcParameter.ProdCompensationElectricityLowLoad + calcParameter.TaxReduction))));
            }
            //BatteryCharge
            //var batteryChargeExist = listBatteryCharge.FirstOrDefault(x => x.Label == entryLabel);
            //if (batteryChargeExist == null)
            //    listBatteryCharge.Add(new ChartEntry { Value = chartDataRequest.ChartDataUnit == ChartDataUnit.kWh ? Convert.ToSingle(item.BatteryCharge) : Convert.ToSingle(item.BatteryChargeProfitFake + (item.BatteryCharge * (calcparms.ProdCompensationElectricityLowload + calcparms.TaxReduction))), Color = batteryChargeColor, Label = entryLabel });
            //else
            //{
            //    batteryChargeExist.Value = batteryChargeExist.Value + (chartDataRequest.ChartDataUnit == ChartDataUnit.kWh ? Convert.ToSingle(item.BatteryCharge) : Convert.ToSingle(item.BatteryChargeProfitFake + (item.BatteryCharge * (calcparms.ProdCompensationElectricityLowload + calcparms.TaxReduction))));
            //}
            //Production Used
            var prodUsedExist = listProductionUsed.FirstOrDefault(x => x.Label == entryLabel);
            if (prodUsedExist == null)
                listProductionUsed.Add(new ChartEntry { Value = chartDataRequest.ChartDataUnit == ChartDataUnit.KWh ? Convert.ToSingle(item.ProductionOwnUse) : Convert.ToSingle(item.ProductionOwnUseProfit + (item.ProductionOwnUse * (calcParameter.TransferFee + calcParameter.EnergyTax))), Color = usedColor, Label = entryLabel });
            else
            {
                prodUsedExist.Value = prodUsedExist.Value + (chartDataRequest.ChartDataUnit == ChartDataUnit.KWh ? Convert.ToSingle(item.ProductionOwnUse) : Convert.ToSingle(item.ProductionOwnUseProfit + (item.ProductionOwnUse * (calcParameter.TransferFee + calcParameter.EnergyTax))));
            }
            //BatteryUsed
            var batteryUsedExist = listBatteryUsed.FirstOrDefault(x => x.Label == entryLabel);
            if (batteryUsedExist == null)
                listBatteryUsed.Add(new ChartEntry
                {
                    Value = chartDataRequest.ChartDataUnit == ChartDataUnit.KWh ? Convert.ToSingle(item.BatteryUsed) : Convert.ToSingle(item.BatteryUsedProfit + ((item.BatteryUsed) * (calcParameter.TransferFee + calcParameter.EnergyTax))),
                    Color = batteryUsedColor,
                    Label = entryLabel
                });
            else
            {
                batteryUsedExist.Value = batteryUsedExist.Value + (chartDataRequest.ChartDataUnit == ChartDataUnit.KWh ? Convert.ToSingle(item.BatteryUsed) : Convert.ToSingle(item.BatteryUsedProfit + item.BatteryUsed + ((item.BatteryUsed) * (calcParameter.TransferFee + calcParameter.EnergyTax))));
            }

            //Consumed FromGrid
            var consumedGridExist = listCunsumtionGrid.FirstOrDefault(x => x.Label == entryLabel);
            if (consumedGridExist == null)
                listCunsumtionGrid.Add(new ChartEntry { Value = chartDataRequest.ChartDataUnit == ChartDataUnit.KWh ? Convert.ToSingle(item.Purchased) : Convert.ToSingle(item.PurchasedCost + ((item.Purchased) * (calcParameter.TransferFee + calcParameter.EnergyTax))), Color = consumGridColor, Label = entryLabel });
            else
            {
                consumedGridExist.Value = consumedGridExist.Value + (chartDataRequest.ChartDataUnit == ChartDataUnit.KWh ? Convert.ToSingle(item.Purchased) : Convert.ToSingle(item.PurchasedCost + item.ProductionOwnUseProfit + ((item.Purchased) * (calcParameter.TransferFee + calcParameter.EnergyTax))));
            }


        }
        //fix prices
        foreach (var item in lisPriceBuy)
            item.Value = devPrice != 0 ? item.Value / devPrice : item.Value;
        foreach (var item in lisPriceSold)
            item.Value = devPrice != 0 ? item.Value / devPrice : item.Value;

        //Fill missing days
        var currentDay = chartDataRequest.FilterStart;
        var enddayfil = chartDataRequest.FilterEnd;
        switch (chartDataRequest.ChartDataRange)
        {
            case ChartDataRange.Today:
            case ChartDataRange.Day:
                while (currentDay < enddayfil)
                {
                    entryLabel = currentDay.ToString("HH:mm");
                    if (listBatteryUsed.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listBatteryUsed.Add(new ChartEntry { Value = null, Color = batteryUsedColor, Label = entryLabel });
                    if (listProductionSold.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listProductionSold.Add(new ChartEntry { Value = null, Color = soldColor, Label = entryLabel });
                    if (listProductionUsed.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listProductionUsed.Add(new ChartEntry { Value = null, Color = usedColor, Label = entryLabel });
                    if (listBatteryCharge.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listBatteryCharge.Add(new ChartEntry { Value = null, Color = batteryChargeColor, Label = entryLabel });
                    if (listCunsumtionGrid.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listCunsumtionGrid.Add(new ChartEntry { Value = null, Color = consumGridColor, Label = entryLabel });

                    currentDay = currentDay.AddHours(1);
                }
                break;
            case ChartDataRange.Week:

                while (currentDay < chartDataRequest.FilterEnd)
                {
                    entryLabel = currentDay.ToString("ddd");
                    if (listBatteryUsed.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listBatteryUsed.Add(new ChartEntry { Value = null, Color = batteryUsedColor, Label = entryLabel });
                    if (listProductionSold.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listProductionSold.Add(new ChartEntry { Value = null, Color = soldColor, Label = entryLabel });
                    if (listProductionUsed.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listProductionUsed.Add(new ChartEntry { Value = null, Color = usedColor, Label = entryLabel });
                    if (listBatteryCharge.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listBatteryCharge.Add(new ChartEntry { Value = null, Color = batteryChargeColor, Label = entryLabel });
                    if (listCunsumtionGrid.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listCunsumtionGrid.Add(new ChartEntry { Value = null, Color = consumGridColor, Label = entryLabel });

                    currentDay = currentDay.AddDays(1);
                }

                break;
            case ChartDataRange.Month:

                while (currentDay < chartDataRequest.FilterEnd)
                {
                    entryLabel = currentDay.ToString("dd");
                    if (listBatteryUsed.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listBatteryUsed.Add(new ChartEntry { Value = null, Color = batteryUsedColor, Label = entryLabel });
                    if (listProductionSold.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listProductionSold.Add(new ChartEntry { Value = null, Color = soldColor, Label = entryLabel });
                    if (listProductionUsed.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listProductionUsed.Add(new ChartEntry { Value = null, Color = usedColor, Label = entryLabel });
                    if (listBatteryCharge.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listBatteryCharge.Add(new ChartEntry { Value = null, Color = batteryChargeColor, Label = entryLabel });
                    if (listCunsumtionGrid.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listCunsumtionGrid.Add(new ChartEntry { Value = null, Color = consumGridColor, Label = entryLabel });

                    currentDay = currentDay.AddDays(1);
                }

                break;
            case ChartDataRange.Year:

                while (currentDay < chartDataRequest.FilterEnd)
                {
                    entryLabel = currentDay.ToString("MMM");
                    if (listBatteryUsed.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listBatteryUsed.Add(new ChartEntry { Value = null, Color = batteryUsedColor, Label = entryLabel });
                    if (listProductionSold.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listProductionSold.Add(new ChartEntry { Value = null, Color = soldColor, Label = entryLabel });
                    if (listProductionUsed.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listProductionUsed.Add(new ChartEntry { Value = null, Color = usedColor, Label = entryLabel });
                    if (listBatteryCharge.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listBatteryCharge.Add(new ChartEntry { Value = null, Color = batteryChargeColor, Label = entryLabel });
                    if (listCunsumtionGrid.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listCunsumtionGrid.Add(new ChartEntry { Value = null, Color = consumGridColor, Label = entryLabel });

                    currentDay = currentDay.AddDays(1);
                }

                break;
        }


        var stats = await roiService.CalculateTotals(chartDataRequest.FilterStart, chartDataRequest.FilterEnd);
        //var stats = await roiService.CalculateTotals(chartDataRequest.FilterStart, chartDataRequest.FilterEnd);


        if (chartDataRequest.ChartDataUnit == ChartDataUnit.KWh)
        {
            result.BatteryChargeTitle = $"{AppResources.Battery} {Math.Round(stats.BatteryCharge, 1)}";
            result.ProductionSoldTile = $"{AppResources.Sold} {Math.Round(stats.ProductionSold, 1)}";
            result.ProductionUsedTile = $"{AppResources.Own_Use} {Math.Round(stats.ProductionOwnUse, 1)}";

            result.BatteryUsedTile = $"{AppResources.Battery} {Math.Round(stats.BatteryUsed)}";
            result.ConsumedGridTile = $"{AppResources.Purchased} {Math.Round(stats.Purchased, 1)}";

            result.ProductionChartTitle = $"{AppResources.Production} {Math.Round(stats.SumAllProduction, 1)} kWh";
            result.ConsumtionChartTitle = $"{AppResources.Consumption} {Math.Round(stats.SumAllConsumption, 1)} kWh";
            result.PriceChartTitle = AppResources.Prices; // "Prices";
            result.PriceBuyTile = AppResources.Buy_Transfer_Fee_Tax;// "Buy (transfer fee/tax)";
            result.PriceSellTile = AppResources.Sell_Tax_Red_Net_Ben;// "Sell (tax red../net ben..)";

        }
        else
        {
            //double used = stats.ProductionOwnUseSaved + stats.ProductionOwnUseTransferFeeSaved + stats.ProductionOwnUseEnergyTaxSaved;
            //double purchasedGrid = stats.PurchasedCost + stats.PurchasedTransferFeeCost + stats.PurchasedTaxCost;
            //double batteryUsed = stats.BatteryUsedSaved + stats.BatteryUseTransferFeeSaved + stats.BatteryUseEnergyTaxSaved;
            double batteryCharge = 0;

            result.BatteryChargeTitle = $"{AppResources.Battery} {Math.Round(batteryCharge, 1)}";
            result.ProductionSoldTile = $"{AppResources.Sold} {Math.Round(stats.SumProductionSoldProfit, 1)}";
            result.ProductionUsedTile = $"{AppResources.Own_Use} {Math.Round(stats.SumProductionOwnUseSaved, 1)}";

            result.BatteryUsedTile = $"{AppResources.Battery} {Math.Round(stats.SumBatteryUseSaved)}";
            result.ConsumedGridTile = $"{AppResources.Grid} {Math.Round(stats.SumPurchasedCost, 1)}";

            result.ProductionChartTitle = $"{AppResources.Production} {Math.Round(stats.SumProductionSoldProfit + stats.SumProductionOwnUseSaved, 1)} Sek";
            result.ConsumtionChartTitle = $"{AppResources.Consumption} {Math.Round(stats.SumPurchasedCost + stats.SumProductionOwnUseSaved + stats.SumBatteryUseSaved, 1)} Sek";
            result.PriceChartTitle = AppResources.Prices; // "Prices";
            result.PriceBuyTile = AppResources.Buy_Transfer_Fee_Tax;// "Buy (transfer fee/tax)";
            result.PriceSellTile = AppResources.Sell_Tax_Red_Net_Ben;// "Sell (tax red../net ben..)";

        }
        result.ChartSeriesPriceBuy = lisPriceBuy;
        result.ChartSeriesPriceSell = lisPriceSold;
        result.ChartSeriesProductionSold = listProductionSold;
        result.ChartSeriesBatteryCharge = listBatteryCharge;
        result.ChartSeriesConsumptionGrid = listCunsumtionGrid;
        result.ChartSeriesBatteryUsed = listBatteryUsed;
        result.ChartSeriesProductionUsed = listProductionUsed;
        //Round up
        result.MaxValueYAxes = Convert.ToSingle(Math.Ceiling(Convert.ToDouble(result.MaxValueYAxes * 1.2)));

        return new Result<ChartDataResult>(result);
    }


}
public class ChartEntry
{


    

    public float? Value { get; set; }

    /// <summary>
    /// Gets or sets the caption label.
    /// </summary>
    /// <value>The label.</value>
    public string Label { get; set; } = "";

    /// <summary>
    /// Gets or sets the label associated to the value.
    /// </summary>
    /// <value>The value label.</value>
    public string ValueLabel { get; set; }= "";

    /// <summary>
    /// Gets or sets the color of the fill.
    /// </summary>
    /// <value>The color of the fill.</value>
    public Color Color { get; set; } = Colors.Black;


    /// <summary>
    /// Gets or sets the color of the rest part
    /// </summary>
    /// <value>The color of the rest part.</value>
    public Color OtherColor { get; set; } = Colors.Blue;


    /// <summary>
    /// Gets or sets the color of the text (for the caption label).
    /// </summary>
    /// <value>The color of the text.</value>
    public Color TextColor { get; set; } = Colors.Gray;


    /// <summary>
    /// Gets or sets the color of the value label
    /// </summary>
    /// <value>The color of the value label.</value>
    public Color ValueLabelColor { get; set; } = Colors.Black;
   

}
public class ChartDataResult
{
    public string PriceChartTitle { get; set; }= "";
    public List<ChartEntry> ChartSeriesPriceBuy { get; set; } = new List<ChartEntry>();
    public List<ChartEntry> ChartSeriesPriceSell { get; set; } = new List<ChartEntry>();
    public string PriceBuyTile { get; set; }= "";
    public string PriceSellTile { get; set; }= "";

    public string ConsumtionChartTitle { get; set; }= "";
    public string ProductionChartTitle { get; set; }= "";
    public List<ChartEntry> ChartSeriesBatteryCharge { get; set; } = new List<ChartEntry>();
    public List<ChartEntry> ChartSeriesProductionUsed { get; set; } = new List<ChartEntry>();
    public List<ChartEntry> ChartSeriesProductionSold { get; set; } = new List<ChartEntry>();
    public List<ChartEntry> ChartSeriesBatteryUsed { get; set; } = new List<ChartEntry>();
    public List<ChartEntry> ChartSeriesConsumptionGrid { get; set; } = new List<ChartEntry>();
    public float MaxValueYAxes { get; set; }


    public string BatteryChargeTitle { get; set; }= "";
    public string ProductionSoldTile { get; set; }= "";
    public string ProductionUsedTile { get; set; }= "";
    public string BatteryUsedTile { get; set; }= "";
    public string ConsumedGridTile { get; set; }= "";
}
public class ChartDataRequest : ObservableObject
{
    public ChartDataRequest()
    {

        TimeStamp = DateTime.Today;
    }
    private ChartDataRange chartDataRange = ChartDataRange.Today;
    public ChartDataRange ChartDataRange
    {
        get => chartDataRange;
        set
        {
            SetProperty(ref chartDataRange, value);
            OnPropertyChanged(nameof(TodayBackgroundColor));
            OnPropertyChanged(nameof(TodayBorderColor));
            OnPropertyChanged(nameof(TodayTextColor));

            OnPropertyChanged(nameof(DayBackgroundColor));
            OnPropertyChanged(nameof(DayBorderColor));
            OnPropertyChanged(nameof(DayTextColor));

            OnPropertyChanged(nameof(WeekBackgroundColor));
            OnPropertyChanged(nameof(WeekBorderColor));
            OnPropertyChanged(nameof(WeekTextColor));

            OnPropertyChanged(nameof(MonthBackgroundColor));
            OnPropertyChanged(nameof(MonthBorderColor));
            OnPropertyChanged(nameof(MonthTextColor));

            OnPropertyChanged(nameof(YearBackgroundColor));
            OnPropertyChanged(nameof(YearBorderColor));
            OnPropertyChanged(nameof(YearTextColor));

            SetFilterDates();
            
        }
    }
    private ChartDataUnit chartDataUnit = ChartDataUnit.KWh;
    public ChartDataUnit ChartDataUnit
    {
        get => chartDataUnit;
        set
        {
            SetProperty(ref chartDataUnit, value);
            OnPropertyChanged(nameof(KwhBackgroundColor));
            OnPropertyChanged(nameof(KwhBorderColor));
            OnPropertyChanged(nameof(KwhTextColor));

            OnPropertyChanged(nameof(CurrencyBackgroundColor));
            OnPropertyChanged(nameof(CurrencyBorderColor));
            OnPropertyChanged(nameof(CurrencyTextColor));

        }
    }
    private DateTime filterStart = DateTime.Today;
    public DateTime FilterStart
    {
        get => filterStart;
        set => SetProperty(ref filterStart, value);
    }
    private DateTime filterEnd = DateTime.Today;
    public DateTime FilterEnd
    {
        get => filterEnd;
        set => SetProperty(ref filterEnd, value);
    }


    private DateTime timeStamp;
    public DateTime TimeStamp
    {
        get => timeStamp;
        set
        {
            SetProperty(ref timeStamp, value);
            SetFilterDates();
           
        }
    }
    private void SetFilterDates()
    {
        DateTime baseDate = new DateTime(timeStamp.Year, timeStamp.Month, timeStamp.Day);
        var resultDates = DateHelper.GetRelatedDates(baseDate);


        switch (chartDataRange)
        {
            case ChartDataRange.Today:
            case ChartDataRange.Day:
                FilterStart = baseDate;
                FilterEnd = filterStart.AddDays(1);
                break;
            case ChartDataRange.Week:
                FilterStart = resultDates.ThisWeekStart;
                FilterEnd = resultDates.ThisWeekEnd;
                break;
            case ChartDataRange.Month:
                FilterStart = resultDates.ThisMonthStart;
                FilterEnd = resultDates.ThisMonthEnd;
                break;
            case ChartDataRange.Year:
                FilterStart = resultDates.ThisYearStart;
                FilterEnd = resultDates.ThisYearEnd;
                break;
        }
        OnPropertyChanged(nameof(TimeStampTitle));
    }

    public string TimeStampTitle
    {
        get
        {
            string returnString;
            if (FilterStart.Year != DateTime.Now.Year)
                return timeStamp.ToShortDateString();

            switch (chartDataRange)
            {
                case ChartDataRange.Today:
                    returnString = AppResources.Today;
                    break;
                case ChartDataRange.Day:
                    returnString =
                        $"{FilterStart.ToString("ddd").ToUpper()} {FilterStart:dd} {FilterStart.ToString("MMM").ToUpper()}";
                    break;
                case ChartDataRange.Week:
                    returnString =
                        $"{FilterStart:dd}/{FilterStart.ToString("MMM").ToUpper()}-{FilterEnd:dd}/{FilterEnd.ToString("MMM").ToUpper()}";
                    break;
                case ChartDataRange.Month:
                    returnString = FilterStart.ToString("MMMM").ToUpper();
                    break;
                case ChartDataRange.Year:
                    returnString = FilterStart.ToString("yyyy");
                    break;
                default:
                    returnString = timeStamp.ToShortDateString();
                    break;
            }
            if (FilterStart.Year != DateTime.Now.Year)
                returnString = returnString + " " + FilterStart.ToString("yyyy");

            return returnString;
        }
    }



    public Color TodayBackgroundColor => chartDataRange == ChartDataRange.Today ? AppColors.SignalBlueColor : AppColors.TransparentColor;
    public Color TodayBorderColor => chartDataRange == ChartDataRange.Today ? AppColors.Gray500Color : AppColors.TransparentColor;
    public Color TodayTextColor => chartDataRange == ChartDataRange.Today ? AppColors.WhiteColor : AppColors.Gray900Color;

    public Color DayBackgroundColor => chartDataRange == ChartDataRange.Day ? AppColors.SignalBlueColor : AppColors.TransparentColor;
    public Color DayBorderColor => chartDataRange == ChartDataRange.Day ? AppColors.Gray500Color : AppColors.TransparentColor;
    public Color DayTextColor => chartDataRange == ChartDataRange.Day ? AppColors.WhiteColor : AppColors.Gray900Color;

    public Color WeekBackgroundColor => chartDataRange == ChartDataRange.Week ? AppColors.SignalBlueColor : AppColors.TransparentColor;
    public Color WeekBorderColor => chartDataRange == ChartDataRange.Week ? AppColors.Gray500Color : AppColors.TransparentColor;
    public Color WeekTextColor => chartDataRange == ChartDataRange.Week ? AppColors.WhiteColor : AppColors.Gray900Color;

    public Color MonthBackgroundColor => chartDataRange == ChartDataRange.Month ? AppColors.SignalBlueColor : AppColors.TransparentColor;
    public Color MonthBorderColor => chartDataRange == ChartDataRange.Month ? AppColors.Gray500Color : AppColors.TransparentColor;
    public Color MonthTextColor => chartDataRange == ChartDataRange.Month ? AppColors.WhiteColor : AppColors.Gray900Color;

    public Color YearBackgroundColor => chartDataRange == ChartDataRange.Year ? AppColors.SignalBlueColor : AppColors.TransparentColor;
    public Color YearBorderColor => chartDataRange == ChartDataRange.Year ? AppColors.Gray500Color : AppColors.TransparentColor;
    public Color YearTextColor => chartDataRange == ChartDataRange.Year ? AppColors.WhiteColor : AppColors.Gray900Color;

    public Color MoreBackgroundColor => chartDataRange == ChartDataRange.Year ? AppColors.SignalBlueColor : AppColors.TransparentColor;
    public Color MoreBorderColor => chartDataRange == ChartDataRange.Year ? AppColors.Gray500Color : AppColors.TransparentColor;
    public Color MoreTextColor => chartDataRange == ChartDataRange.Year ? AppColors.WhiteColor : AppColors.Gray900Color;


    public Color KwhBackgroundColor => ChartDataUnit == ChartDataUnit.KWh ? AppColors.SignalBlueColor : AppColors.TransparentColor;
    public Color KwhBorderColor => ChartDataUnit == ChartDataUnit.KWh ? AppColors.Gray500Color : AppColors.TransparentColor;
    public Color KwhTextColor => ChartDataUnit == ChartDataUnit.KWh ? AppColors.WhiteColor : AppColors.Gray900Color;

    public Color CurrencyBackgroundColor => ChartDataUnit == ChartDataUnit.Currency ? AppColors.SignalBlueColor : AppColors.TransparentColor;
    public Color CurrencyBorderColor => ChartDataUnit == ChartDataUnit.Currency ? AppColors.Gray500Color : AppColors.TransparentColor;
    public Color CurrencyTextColor => ChartDataUnit == ChartDataUnit.Currency ? AppColors.WhiteColor : AppColors.Gray900Color;
}
public enum ChartDataRange
{
    Today = 0,
    Day = 1,
    Week = 2,
    Month = 3,
    Year = 4
}
public enum ChartDataUnit
{
    KWh = 1,
    Currency = 2,

}