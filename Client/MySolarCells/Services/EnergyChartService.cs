namespace MySolarCells.Services;

public interface IEnergyChartService
{
    Task<Result<ChartDataResult>> GetChartData(ChartDataRequest chartDataRequest);
}

public class EnergyChartService : IEnergyChartService
{
    private readonly IRoiService roiService;
    public EnergyChartService(IRoiService roiService)
    {
        this.roiService = roiService;
    }
    public async Task<Result<ChartDataResult>> GetChartData(ChartDataRequest chartDataRequest)
    {
        using var dbContext = new MscDbContext();
        ChartDataResult result = new ChartDataResult();
        string entryLabel = string.Empty;
        var calcparms = await dbContext.EnergyCalculationParameter.FirstAsync();

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
        var dataRows = await dbContext.Energy.Where(x => x.Timestamp >= chartDataRequest.FilterStart && x.Timestamp < chartDataRequest.FilterEnd).ToListAsync();
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
            var batteryChargeExist = listBatteryCharge.FirstOrDefault(x => x.Label == entryLabel);
            if (batteryChargeExist == null)
                listBatteryCharge.Add(new ChartEntry { Value = chartDataRequest.ChartDataUnit == ChartDataUnit.kWh ? Convert.ToSingle(item.BatteryCharge) : Convert.ToSingle(item.BatteryChargeProfitFake + (item.BatteryCharge * (calcparms.ProdCompensationElectricityLowload + calcparms.TaxReduction))), Color = batteryChargeColor, Label = entryLabel });
            else
            {
                batteryChargeExist.Value = batteryChargeExist.Value + (chartDataRequest.ChartDataUnit == ChartDataUnit.kWh ? Convert.ToSingle(item.BatteryCharge) : Convert.ToSingle(item.BatteryChargeProfitFake + (item.BatteryCharge * (calcparms.ProdCompensationElectricityLowload + calcparms.TaxReduction))));
            }
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


        var stats = await roiService.CalculateTotals(chartDataRequest.FilterStart, chartDataRequest.FilterEnd, false);


        if (chartDataRequest.ChartDataUnit == ChartDataUnit.kWh)
        {
            result.BatteryChargeTitle = string.Format("Battery {0}", Math.Round(stats.TotalBatteryCharge, 2));
            result.ProductionSoldTile = string.Format("Sold {0}", Math.Round(stats.TotalProductionSold, 2));
            result.ProductionUsedTile = string.Format("Used {0}", Math.Round(stats.TotalProductionOwnUse, 2));

            result.BatteryUsedTile = string.Format("Battery {0}", Math.Round(stats.TotalBatteryUsed, 2));
            result.ConsumedGridTile = string.Format("Grid {0}", Math.Round(stats.TotalPurchased, 2));

            result.ProductionChartTitle = string.Format("Production {0} kWh", Math.Round(stats.TotalBatteryCharge + stats.TotalProductionOwnUse + stats.TotalProductionSold, 2));
            result.ConsumtionChartTitle = string.Format("Consumtion {0} kWh", Math.Round(stats.TotalBatteryUsed + stats.TotalPurchased + stats.TotalProductionOwnUse, 2));
            result.PriceChartTitle = "Prices";
            result.PriceBuyTile = "Buy (transfer fee/tax)";
            result.PriceSellTile = "Sell (tax red../net ben..)";

        }
        else
        {
            double sold = stats.TotalProductionSoldProfit + stats.TotalCompensationForProductionToGrid + stats.TotalSavedEnergyTaxReductionProductionToGrid;
            double used = stats.TotalProductionOwnUseProfit + stats.TotalSavedTransferFeeProductionOwnUse + stats.TotalSavedEnergyTaxProductionOwnUse;
            double purchasedGrid = stats.TotalPurchasedCost + stats.TotalPurchasedTransferFee + stats.TotalPurchasedTax;
            double batteryUsed = stats.TotalBatteryUsedProfit + stats.TotalSavedTransferFeeBatteryUse + stats.TotalSavedEnergyTaxBatteryUse;
            double batteryCharge = stats.TotalBatteryChargeProfitFake + stats.TotalCompensationForProductionToGridChargeBatteryFake + stats.TotalSavedEnergyTaxReductionBatteryChargeFakeToGrid;

            result.BatteryChargeTitle = string.Format("Battery {0}", Math.Round(batteryCharge, 2));
            result.ProductionSoldTile = string.Format("Sold {0}", Math.Round(sold, 2));
            result.ProductionUsedTile = string.Format("Used {0}", Math.Round(used, 2));

            result.BatteryUsedTile = string.Format("Battery {0}", Math.Round(batteryUsed, 2));
            result.ConsumedGridTile = string.Format("Grid {0}", Math.Round(purchasedGrid, 2));

            result.ProductionChartTitle = string.Format("Production {0} Sek", Math.Round(sold + used, 2));
            result.ConsumtionChartTitle = string.Format("Consumtion {0} Sek", Math.Round(purchasedGrid + used + batteryUsed, 2));
            result.PriceChartTitle = "Prices";
            result.PriceBuyTile = "Buy (transfer fee/tax)";
            result.PriceSellTile = "Sell (tax red../net ben..)";
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
            OnPropertyChanged(nameof(DayBackgrundColor));
            OnPropertyChanged(nameof(WeekBackgrundColor));
            OnPropertyChanged(nameof(MonthBackgrundColor));
            OnPropertyChanged(nameof(YearBackgrundColor));
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
            OnPropertyChanged(nameof(CurrencyBackgrundColor));

        }
    }
    private DateTime filterStart = DateTime.Now;
    public DateTime FilterStart
    {
        get { return filterStart; }
        set { SetProperty(ref filterStart, value); }
    }
    private DateTime filterEnd = DateTime.Now;
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
            OnPropertyChanged(nameof(TimeStampTitle));
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

    }





    public Color TodayBackgrundColor { get { return chartDataRange == ChartDataRange.Today ? AppColors.Gray200Color : AppColors.TransparentColor; } }

    public Color DayBackgrundColor { get { return chartDataRange == ChartDataRange.Day ? AppColors.Gray200Color : AppColors.TransparentColor; } }

    public Color WeekBackgrundColor { get { return chartDataRange == ChartDataRange.Week ? AppColors.Gray200Color : AppColors.TransparentColor; } }

    public Color MonthBackgrundColor { get { return chartDataRange == ChartDataRange.Month ? AppColors.Gray200Color : AppColors.TransparentColor; } }

    public Color YearBackgrundColor { get { return chartDataRange == ChartDataRange.Year ? AppColors.Gray200Color : AppColors.TransparentColor; } }

    public Color KwhBackgrundColor { get { return ChartDataUnit == ChartDataUnit.kWh ? AppColors.Gray200Color : AppColors.TransparentColor; } }

    public Color CurrencyBackgrundColor { get { return ChartDataUnit == ChartDataUnit.Currency ? AppColors.Gray200Color : AppColors.TransparentColor; } }

    public string TimeStampTitle { get { return timeStamp.ToLongDateString(); } }
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