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
        var calcparms = await this.mscDbContext.EnergyCalculationParameter.FirstAsync();

        // ------ Create 4 serices ---------------
        //Production Sold
        List<ChartEntry> listProductionSold = new List<ChartEntry>();
        Color soldColor = Color.Parse("#640abf");
        //Production used
        List<ChartEntry> listProductionUsed = new List<ChartEntry>();
        Color usedColor = Color.Parse("#46a80d");

        //Consumtion
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
        var dataRows = await this.mscDbContext.Energy.Where(x => x.Timestamp >= chartDataRequest.FilterStart && x.Timestamp < chartDataRequest.FilterEnd).ToListAsync();
        if (dataRows != null && dataRows.Count == 0)
            return new Result<ChartDataResult>("No records", ErrorCodes.NoEnergyEntryOnCurrentDate);
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
                default:
                    break;
            }

            //Price buy
            var pricebuyExist = lisPriceBuy.FirstOrDefault(x => x.Label == entryLabel);
            if (pricebuyExist == null)
                lisPriceBuy.Add(new ChartEntry { Value = Convert.ToSingle(item.UnitPriceBuy + calcparms.TransferFee + calcparms.EnergyTax), Color = priceBuyColor, Label = entryLabel });
            else
            {
                pricebuyExist.Value = pricebuyExist.Value + Convert.ToSingle(item.UnitPriceBuy + calcparms.TransferFee + calcparms.EnergyTax);
            }
            //Price Sell
            var priceSoldExist = lisPriceSold.FirstOrDefault(x => x.Label == entryLabel);
            if (priceSoldExist == null)
                lisPriceSold.Add(new ChartEntry { Value = Convert.ToSingle(item.UnitPriceSold + calcparms.ProdCompensationElectricityLowload + calcparms.TaxReduction), Color = priceSellColor, Label = entryLabel });
            else
            {
                priceSoldExist.Value = priceSoldExist.Value + Convert.ToSingle(item.UnitPriceBuy + calcparms.TransferFee + calcparms.EnergyTax);
            }
            //TODO:All Profit that used in graph is base on Spotprise should also use fixed price if that is in calc params. 
            //Production Sold
            var prodSoldExist = listProductionSold.FirstOrDefault(x => x.Label == entryLabel);
            if (prodSoldExist == null)
                listProductionSold.Add(new ChartEntry { Value = chartDataRequest.ChartDataUnit == ChartDataUnit.kWh ? Convert.ToSingle(item.ProductionSold) : Convert.ToSingle(item.ProductionSoldProfit + (item.ProductionSold * (calcparms.ProdCompensationElectricityLowload + calcparms.TaxReduction))), Color = soldColor, Label = entryLabel });
            else
            {
                prodSoldExist.Value = prodSoldExist.Value + (chartDataRequest.ChartDataUnit == ChartDataUnit.kWh ? Convert.ToSingle(item.ProductionSold) : Convert.ToSingle(item.ProductionSoldProfit + (item.ProductionSold * (calcparms.ProdCompensationElectricityLowload + calcparms.TaxReduction))));
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
                listProductionUsed.Add(new ChartEntry { Value = chartDataRequest.ChartDataUnit == ChartDataUnit.kWh ? Convert.ToSingle(item.ProductionOwnUse) : Convert.ToSingle(item.ProductionOwnUseProfit + (item.ProductionOwnUse * (calcparms.TransferFee + calcparms.EnergyTax))), Color = usedColor, Label = entryLabel });
            else
            {
                prodUsedExist.Value = prodUsedExist.Value + (chartDataRequest.ChartDataUnit == ChartDataUnit.kWh ? Convert.ToSingle(item.ProductionOwnUse) : Convert.ToSingle(item.ProductionOwnUseProfit + (item.ProductionOwnUse * (calcparms.TransferFee + calcparms.EnergyTax))));
            }
            //BatteryUsed
            var batteryUsedExist = listBatteryUsed.FirstOrDefault(x => x.Label == entryLabel);
            if (batteryUsedExist == null)
                listBatteryUsed.Add(new ChartEntry
                {
                    Value = chartDataRequest.ChartDataUnit == ChartDataUnit.kWh ? Convert.ToSingle(item.BatteryUsed) : Convert.ToSingle(item.BatteryUsedProfit + ((item.BatteryUsed) * (calcparms.TransferFee + calcparms.EnergyTax))),
                    Color = batteryUsedColor,
                    Label = entryLabel
                });
            else
            {
                batteryUsedExist.Value = batteryUsedExist.Value + (chartDataRequest.ChartDataUnit == ChartDataUnit.kWh ? Convert.ToSingle(item.BatteryUsed) : Convert.ToSingle(item.BatteryUsedProfit + item.BatteryUsed + ((item.BatteryUsed) * (calcparms.TransferFee + calcparms.EnergyTax))));
            }

            //Consumed FromGrid
            var consumedGridExist = listCunsumtionGrid.FirstOrDefault(x => x.Label == entryLabel);
            if (consumedGridExist == null)
                listCunsumtionGrid.Add(new ChartEntry { Value = chartDataRequest.ChartDataUnit == ChartDataUnit.kWh ? Convert.ToSingle(item.Purchased) : Convert.ToSingle(item.PurchasedCost + ((item.Purchased) * (calcparms.TransferFee + calcparms.EnergyTax))), Color = consumGridColor, Label = entryLabel });
            else
            {
                consumedGridExist.Value = consumedGridExist.Value + (chartDataRequest.ChartDataUnit == ChartDataUnit.kWh ? Convert.ToSingle(item.Purchased) : Convert.ToSingle(item.PurchasedCost + item.ProductionOwnUseProfit + ((item.Purchased) * (calcparms.TransferFee + calcparms.EnergyTax))));
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
            default:
                break;
        }


        var stats = await roiService.CalculateTotals(chartDataRequest.FilterStart, chartDataRequest.FilterEnd);
        //var stats = await roiService.CalculateTotals(chartDataRequest.FilterStart, chartDataRequest.FilterEnd);


        if (chartDataRequest.ChartDataUnit == ChartDataUnit.kWh)
        {
            result.BatteryChargeTitle = string.Format("{0} {1}",AppResources.Battery, Math.Round(stats.BatteryCharge,1));
            result.ProductionSoldTile = string.Format("{0} {1}",AppResources.Sold, Math.Round(stats.ProductionSold,1));
            result.ProductionUsedTile = string.Format("{0} {1}",AppResources.Own_Use, Math.Round(stats.ProductionOwnUse,1));

            result.BatteryUsedTile = string.Format("{0} {1}",AppResources.Battery, Math.Round(stats.BatteryUsed),1);
            result.ConsumedGridTile = string.Format("{0} {1}",AppResources.Purchased, Math.Round(stats.Purchased,1));

            result.ProductionChartTitle = string.Format("{0} {1} kWh", AppResources.Production, Math.Round(stats.SumAllProduction,1));
            result.ConsumtionChartTitle = string.Format("{0} {1} kWh", AppResources.Consumption, Math.Round(stats.SumAllConsumption,1));
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

            result.BatteryChargeTitle = string.Format("{0} {1}",AppResources.Battery, Math.Round(batteryCharge,1));
            result.ProductionSoldTile = string.Format("{0} {1}",AppResources.Sold, Math.Round(stats.SumProductionSoldProfit,1));
            result.ProductionUsedTile = string.Format("{0} {1}",AppResources.Own_Use, Math.Round(stats.SumProductionOwnUseSaved,1));

            result.BatteryUsedTile = string.Format("{0} {1}",AppResources.Battery, Math.Round(stats.SumBatteryUseSaved),1);
            result.ConsumedGridTile = string.Format("{0} {1}",AppResources.Grid, Math.Round(stats.SumPurchasedCost,1));

            result.ProductionChartTitle = string.Format("{0} {1} Sek",AppResources.Production, Math.Round(stats.SumProductionSoldProfit + stats.SumProductionOwnUseSaved, 1));
            result.ConsumtionChartTitle = string.Format("{0} {1} Sek",AppResources.Consumption, Math.Round(stats.SumPurchasedCost + stats.SumProductionOwnUseSaved + stats.SumBatteryUseSaved, 1));
            result.PriceChartTitle = AppResources.Prices; // "Prices";
            result.PriceBuyTile = AppResources.Buy_Transfer_Fee_Tax;// "Buy (transfer fee/tax)";
            result.PriceSellTile = AppResources.Sell_Tax_Red_Net_Ben;// "Sell (tax red../net ben..)";

        }
        result.ChartSeriesPriceBuy = lisPriceBuy;
        result.ChartSeriesPriceSell = lisPriceSold;
        result.ChartSeriesProductionSold = listProductionSold;
        result.ChartSeriesBatteryCharge = listBatteryCharge;
        result.ChartSeriesConsumtionGrid = listCunsumtionGrid;
        result.ChartSeriesBatteryUsed = listBatteryUsed;
        result.ChartSeriesProductionUsed = listProductionUsed;

        //Calc MaxValueYaxes so all graphs has the same resolution
        //foreach (var item in result.ChartSeriesProductionTot)
        //{
        //    if (item.Entries.Max(x => x.Value) > result.MaxValueYaxes)
        //        result.MaxValueYaxes = item.Entries.Max(x => x.Value).Value;
        //}
        //foreach (var item in result.ChartSeriesConsumtionTot)
        //{
        //    if (item.Entries.Max(x => x.Value) > result.MaxValueYaxes)
        //        result.MaxValueYaxes = item.Entries.Max(x => x.Value).Value;
        //}
        //Round up
        result.MaxValueYaxes = Convert.ToSingle(Math.Ceiling(Convert.ToDouble(result.MaxValueYaxes * 1.2)));

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
    public string Label { get; set; }

    /// <summary>
    /// Gets or sets the label associated to the value.
    /// </summary>
    /// <value>The value label.</value>
    public string ValueLabel { get; set; }

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
    //public Microcharts.ChartEntry ChartEntry { get { return new Microcharts.ChartEntry(Value == 0 || Value == null ? null : Convert.ToSingle(Math.Round(Value.Value, 2))) { Label = Label, ValueLabel = ValueLabel, Color = Color, OtherColor = OtherColor, TextColor = TextColor, ValueLabelColor = ValueLabelColor }; } }


}
public class ChartDataResult
{
    public string PriceChartTitle { get; set; }
    public List<ChartEntry> ChartSeriesPriceBuy { get; set; } = new List<ChartEntry>();
    public List<ChartEntry> ChartSeriesPriceSell { get; set; } = new List<ChartEntry>();
    public string PriceBuyTile { get; set; }
    public string PriceSellTile { get; set; }

    public string ConsumtionChartTitle { get; set; }
    public string ProductionChartTitle { get; set; }
    public List<ChartEntry> ChartSeriesBatteryCharge { get; set; } = new List<ChartEntry>();
    public List<ChartEntry> ChartSeriesProductionUsed { get; set; } = new List<ChartEntry>();
    public List<ChartEntry> ChartSeriesProductionSold { get; set; } = new List<ChartEntry>();
    public List<ChartEntry> ChartSeriesBatteryUsed { get; set; } = new List<ChartEntry>();
    public List<ChartEntry> ChartSeriesConsumtionGrid { get; set; } = new List<ChartEntry>();
    public float MaxValueYaxes { get; set; }


    public string BatteryChargeTitle { get; set; }
    public string ProductionSoldTile { get; set; }
    public string ProductionUsedTile { get; set; }
    public string BatteryUsedTile { get; set; }
    public string ConsumedGridTile { get; set; }
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
        get
        {
            return chartDataRange;
        }
        set
        {
            SetProperty(ref chartDataRange, value);
            OnPropertyChanged(nameof(TodayBackgrundColor));
            OnPropertyChanged(nameof(TodayBorderColor));
            OnPropertyChanged(nameof(TodayTextColor));

            OnPropertyChanged(nameof(DayBackgrundColor));
            OnPropertyChanged(nameof(DayBorderColor));
            OnPropertyChanged(nameof(DayTextColor));

            OnPropertyChanged(nameof(WeekBackgrundColor));
            OnPropertyChanged(nameof(WeekBorderColor));
            OnPropertyChanged(nameof(WeekTextColor));

            OnPropertyChanged(nameof(MonthBackgrundColor));
            OnPropertyChanged(nameof(MonthBorderColor));
            OnPropertyChanged(nameof(MonthTextColor));

            OnPropertyChanged(nameof(YearBackgrundColor));
            OnPropertyChanged(nameof(YearBorderColor));
            OnPropertyChanged(nameof(YearTextColor));

            SetFilterDates();
            
        }
    }
    private ChartDataUnit chartDataUnit = ChartDataUnit.kWh;
    public ChartDataUnit ChartDataUnit
    {
        get
        {
            return chartDataUnit;
        }
        set
        {
            SetProperty(ref chartDataUnit, value);
            OnPropertyChanged(nameof(KwhBackgrundColor));
            OnPropertyChanged(nameof(KwhBorderColor));
            OnPropertyChanged(nameof(KwhTextColor));

            OnPropertyChanged(nameof(CurrencyBackgrundColor));
            OnPropertyChanged(nameof(CurrencyBorderColor));
            OnPropertyChanged(nameof(CurrencyTextColor));

        }
    }
    private DateTime filterStart = DateTime.Today;
    public DateTime FilterStart
    {
        get { return filterStart; }
        set { SetProperty(ref filterStart, value); }
    }
    private DateTime filterEnd = DateTime.Today;
    public DateTime FilterEnd
    {
        get { return filterEnd; }
        set { SetProperty(ref filterEnd, value); }
    }


    private DateTime timeStamp;
    public DateTime TimeStamp
    {
        get
        {
            return timeStamp;
        }
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
                FilterEnd = resultDates.ThisYearhEnd;
                break;
            default:
                break;
        }
        OnPropertyChanged(nameof(TimeStampTitle));
    }

    public string TimeStampTitle
    {
        get
        {
            string returnstr = "";
            if (FilterStart.Year != DateTime.Now.Year)
                return timeStamp.ToShortDateString();

            switch (chartDataRange)
            {
                case ChartDataRange.Today:
                    returnstr = AppResources.Today;
                    break;
                case ChartDataRange.Day:
                    returnstr = string.Format("{0} {1} {2}", FilterStart.ToString("ddd").ToUpper(), FilterStart.ToString("dd"), FilterStart.ToString("MMM").ToUpper());
                    break;
                case ChartDataRange.Week:
                    returnstr = string.Format("{0}{1}{2}{3}{4}{5}{6}", FilterStart.ToString("dd"),"/", FilterStart.ToString("MMM").ToUpper(), "-", FilterEnd.ToString("dd"),"/", FilterEnd.ToString("MMM").ToUpper());
                    break;
                case ChartDataRange.Month:
                    returnstr = FilterStart.ToString("MMMM").ToUpper();
                    break;
                case ChartDataRange.Year:
                    returnstr = FilterStart.ToString("yyyy");
                    break;
                default:
                    returnstr = timeStamp.ToShortDateString();
                    break;
            }
            if (FilterStart.Year != DateTime.Now.Year)
                returnstr = returnstr + " " + FilterStart.ToString("yyyy");

            return returnstr;
        }
    }



    public Color TodayBackgrundColor { get { return chartDataRange == ChartDataRange.Today ? AppColors.SignalBlueColor : AppColors.TransparentColor; } }
    public Color TodayBorderColor { get { return chartDataRange == ChartDataRange.Today ? AppColors.Gray500Color : AppColors.TransparentColor; } }
    public Color TodayTextColor { get { return chartDataRange == ChartDataRange.Today ? AppColors.WhiteColor : AppColors.Gray900Color; } }

    public Color DayBackgrundColor { get { return chartDataRange == ChartDataRange.Day ? AppColors.SignalBlueColor : AppColors.TransparentColor; } }
    public Color DayBorderColor { get { return chartDataRange == ChartDataRange.Day ? AppColors.Gray500Color : AppColors.TransparentColor; } }
    public Color DayTextColor { get { return chartDataRange == ChartDataRange.Day ? AppColors.WhiteColor : AppColors.Gray900Color; } }

    public Color WeekBackgrundColor { get { return chartDataRange == ChartDataRange.Week ? AppColors.SignalBlueColor : AppColors.TransparentColor; } }
    public Color WeekBorderColor { get { return chartDataRange == ChartDataRange.Week ? AppColors.Gray500Color : AppColors.TransparentColor; } }
    public Color WeekTextColor { get { return chartDataRange == ChartDataRange.Week ? AppColors.WhiteColor : AppColors.Gray900Color; } }

    public Color MonthBackgrundColor { get { return chartDataRange == ChartDataRange.Month ? AppColors.SignalBlueColor : AppColors.TransparentColor; } }
    public Color MonthBorderColor { get { return chartDataRange == ChartDataRange.Month ? AppColors.Gray500Color : AppColors.TransparentColor; } }
    public Color MonthTextColor { get { return chartDataRange == ChartDataRange.Month ? AppColors.WhiteColor : AppColors.Gray900Color; } }

    public Color YearBackgrundColor { get { return chartDataRange == ChartDataRange.Year ? AppColors.SignalBlueColor : AppColors.TransparentColor; } }
    public Color YearBorderColor { get { return chartDataRange == ChartDataRange.Year ? AppColors.Gray500Color : AppColors.TransparentColor; } }
    public Color YearTextColor { get { return chartDataRange == ChartDataRange.Year ? AppColors.WhiteColor : AppColors.Gray900Color; } }

    public Color MoreBackgrundColor { get { return chartDataRange == ChartDataRange.Year ? AppColors.SignalBlueColor : AppColors.TransparentColor; } }
    public Color MoreBorderColor { get { return chartDataRange == ChartDataRange.Year ? AppColors.Gray500Color : AppColors.TransparentColor; } }
    public Color MoreTextColor { get { return chartDataRange == ChartDataRange.Year ? AppColors.WhiteColor : AppColors.Gray900Color; } }


    public Color KwhBackgrundColor { get { return ChartDataUnit == ChartDataUnit.kWh ? AppColors.SignalBlueColor : AppColors.TransparentColor; } }
    public Color KwhBorderColor { get { return ChartDataUnit == ChartDataUnit.kWh ? AppColors.Gray500Color : AppColors.TransparentColor; } }
    public Color KwhTextColor { get { return ChartDataUnit == ChartDataUnit.kWh ? AppColors.WhiteColor : AppColors.Gray900Color; } }

    public Color CurrencyBackgrundColor { get { return ChartDataUnit == ChartDataUnit.Currency ? AppColors.SignalBlueColor : AppColors.TransparentColor; } }
    public Color CurrencyBorderColor { get { return ChartDataUnit == ChartDataUnit.Currency ? AppColors.Gray500Color : AppColors.TransparentColor; } }
    public Color CurrencyTextColor { get { return ChartDataUnit == ChartDataUnit.Currency ? AppColors.WhiteColor : AppColors.Gray900Color; } }

    
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
    kWh = 1,
    Currency = 2,

}