using System;
using Microcharts;
using SkiaSharp;

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
        //poduction Total
        //List<DummyEntry> listProductionTot = new List<DummyEntry>();
        //SkiaSharp.SKColor totColor = SkiaSharp.SKColor.Parse("#e8900c");
        //Consumtion Total
        //List<DummyEntry> listCunsumtionTot = new List<DummyEntry>();
        //SkiaSharp.SKColor consumTotColor = SkiaSharp.SKColor.Parse("#5b46e3");
        //Production Sold
        List<DummyEntry> listProductionSold = new List<DummyEntry>();
        SkiaSharp.SKColor soldColor = SkiaSharp.SKColor.Parse("#640abf");
        //Production used
        List<DummyEntry> listProductionUsed = new List<DummyEntry>();
        SkiaSharp.SKColor usedColor = SkiaSharp.SKColor.Parse("#3498db");
        
        //Consumtion
        List<DummyEntry> listCunsumtionGrid = new List<DummyEntry>();
        SkiaSharp.SKColor consumGridColor = SkiaSharp.SKColor.Parse("#5b46e3");
        //batteryUsedExist
        List<DummyEntry> listBatteryUsed = new List<DummyEntry>();
        SkiaSharp.SKColor batteryUsedColor = SkiaSharp.SKColor.Parse("#5b26e3");
        //BatteryChargeExist
        List<DummyEntry> listBatteryCharge = new List<DummyEntry>();
        SkiaSharp.SKColor batteryChargeColor = SkiaSharp.SKColor.Parse("#1b46e3");
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
                    break;
                case ChartDataRange.Week:
                    entryLabel = item.Timestamp.ToString("ddd");
                    break;
                case ChartDataRange.Month:
                    entryLabel = item.Timestamp.ToString("dd");
                    break;
                case ChartDataRange.Year:
                    entryLabel = item.Timestamp.ToString("MMM");
                    break;
                default:
                    break;
            }

            
            //Production Sold
            var prodSoldExist = listProductionSold.FirstOrDefault(x => x.Label == entryLabel);
            if (prodSoldExist == null)
                listProductionSold.Add(new DummyEntry { Value = chartDataRequest.ChartDataUnit == ChartDataUnit.kWh ? Convert.ToSingle(item.ProductionSold) : Convert.ToSingle(item.ProductionSoldProfit + (item.ProductionSold * (calcparms.ProdCompensationElectricityLowload + calcparms.TaxReduction))), Color = soldColor, Label = entryLabel });
            else
            {
                prodSoldExist.Value = prodSoldExist.Value + (chartDataRequest.ChartDataUnit == ChartDataUnit.kWh ? Convert.ToSingle(item.ProductionSold) : Convert.ToSingle(item.ProductionSoldProfit + (item.ProductionSold * (calcparms.ProdCompensationElectricityLowload + calcparms.TaxReduction))));
            }
            //BatteryCharge
            var batteryChargeExist = listBatteryCharge.FirstOrDefault(x => x.Label == entryLabel);
            if (batteryChargeExist == null)
                listBatteryCharge.Add(new DummyEntry { Value = chartDataRequest.ChartDataUnit == ChartDataUnit.kWh ? Convert.ToSingle(item.BatteryCharge) : 0, Color = batteryUsedColor, Label = entryLabel });
            else
            {
                batteryChargeExist.Value = batteryChargeExist.Value + (chartDataRequest.ChartDataUnit == ChartDataUnit.kWh ? Convert.ToSingle(item.BatteryCharge) : 0);
            }
            //Production Used
            var prodUsedExist = listProductionUsed.FirstOrDefault(x => x.Label == entryLabel);
            if (prodUsedExist == null)
                listProductionUsed.Add(new DummyEntry { Value = chartDataRequest.ChartDataUnit == ChartDataUnit.kWh ? Convert.ToSingle(item.ProductionOwnUse) : Convert.ToSingle(item.ProductionOwnUseProfit + (item.ProductionOwnUse * (calcparms.TransferFee + calcparms.EnergyTax))), Color = consumGridColor, Label = entryLabel });
            else
            {
                prodUsedExist.Value = prodUsedExist.Value + (chartDataRequest.ChartDataUnit == ChartDataUnit.kWh ? Convert.ToSingle(item.ProductionOwnUse) : Convert.ToSingle(item.ProductionOwnUseProfit + (item.ProductionOwnUse * (calcparms.TransferFee + calcparms.EnergyTax))));
            }
            //BatteryUsed
            var batteryUsedExist = listBatteryUsed.FirstOrDefault(x => x.Label == entryLabel);
            if (batteryUsedExist == null)
                listBatteryUsed.Add(new DummyEntry
                {
                    Value = chartDataRequest.ChartDataUnit == ChartDataUnit.kWh ? Convert.ToSingle(item.BatteryUsed) : Convert.ToSingle(item.BatteryUsedProfit + ((item.BatteryUsed) * (calcparms.TransferFee + calcparms.EnergyTax))),Color = batteryUsedColor, Label = entryLabel });
            else
            {
                batteryUsedExist.Value = batteryUsedExist.Value + (chartDataRequest.ChartDataUnit == ChartDataUnit.kWh ? Convert.ToSingle(item.BatteryUsed) : Convert.ToSingle(item.BatteryUsedProfit + item.BatteryUsed + ((item.BatteryUsed) * (calcparms.TransferFee + calcparms.EnergyTax))));
            }
            
            //Consumed FromGrid
            var consumedGridExist = listCunsumtionGrid.FirstOrDefault(x => x.Label == entryLabel);
            if (consumedGridExist == null)
                listCunsumtionGrid.Add(new DummyEntry { Value = chartDataRequest.ChartDataUnit == ChartDataUnit.kWh ? Convert.ToSingle(item.Purchased) : Convert.ToSingle(item.PurchasedCost + ((item.Purchased) * (calcparms.TransferFee + calcparms.EnergyTax))), Color = consumGridColor, Label = entryLabel });
            else
            {
                consumedGridExist.Value = consumedGridExist.Value + (chartDataRequest.ChartDataUnit == ChartDataUnit.kWh ? Convert.ToSingle(item.Purchased) : Convert.ToSingle(item.PurchasedCost + item.ProductionOwnUseProfit + ((item.Purchased) * (calcparms.TransferFee + calcparms.EnergyTax))));
            }

            //Consumed Total
            //var consumedTotExist = listCunsumtionTot.FirstOrDefault(x => x.Label == entryLabel);
            //if (consumedTotExist == null)
            //    listCunsumtionTot.Add(new DummyEntry
            //    {
            //        Value = chartDataRequest.ChartDataUnit == ChartDataUnit.kWh ? Convert.ToSingle(item.Purchased + item.ProductionOwnUse) :
            //            Convert.ToSingle(item.PurchasedCost + item.ProductionOwnUseProfit + ((item.Purchased + item.ProductionOwnUse) * (calcparms.TransferFee + calcparms.EnergyTax))),
            //        Color = consumTotColor,
            //        Label = entryLabel
            //    });
            //else
            //{
            //    consumedTotExist.Value = consumedTotExist.Value + (chartDataRequest.ChartDataUnit == ChartDataUnit.kWh ? Convert.ToSingle(item.Purchased + item.ProductionOwnUse) :
            //        Convert.ToSingle(item.PurchasedCost + item.ProductionOwnUseProfit + ((item.Purchased + item.ProductionOwnUse) * (calcparms.TransferFee + calcparms.EnergyTax))));
            //}
            //Poduction Total
            //var prodTotExist = listProductionTot.FirstOrDefault(x => x.Label == entryLabel);
            //if (prodTotExist == null)
            //    listProductionTot.Add(new DummyEntry
            //    {
            //        Value = chartDataRequest.ChartDataUnit == ChartDataUnit.kWh ? Convert.ToSingle(item.ProductionOwnUse + item.ProductionSold)
            //    : Convert.ToSingle(item.ProductionOwnUseProfit + item.ProductionSoldProfit + (item.ProductionOwnUse * (calcparms.TransferFee + calcparms.EnergyTax) + (item.ProductionSold * (calcparms.ProdCompensationElectricityLowload + calcparms.TaxReduction)))),
            //        Color = totColor,
            //        Label = entryLabel
            //    });
            //else
            //{
            //    prodTotExist.Value = prodTotExist.Value + (chartDataRequest.ChartDataUnit == ChartDataUnit.kWh ? Convert.ToSingle(item.ProductionOwnUse + item.ProductionSold)
            //    : Convert.ToSingle(item.ProductionOwnUseProfit + item.ProductionSoldProfit + (item.ProductionOwnUse * (calcparms.TransferFee + calcparms.EnergyTax) + (item.ProductionSold * (calcparms.ProdCompensationElectricityLowload + calcparms.TaxReduction)))));
            //}
        }

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
                        listBatteryUsed.Add(new DummyEntry { Value = null, Color = batteryUsedColor, Label = entryLabel });
                    if (listProductionSold.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listProductionSold.Add(new DummyEntry { Value = null, Color = soldColor, Label = entryLabel });
                    if (listProductionUsed.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listProductionUsed.Add(new DummyEntry { Value = null, Color = usedColor, Label = entryLabel });
                    if (listBatteryCharge.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listBatteryCharge.Add(new DummyEntry { Value = null, Color = batteryChargeColor, Label = entryLabel });
                    if (listCunsumtionGrid.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listCunsumtionGrid.Add(new DummyEntry { Value = null, Color = consumGridColor, Label = entryLabel });

                    currentDay = currentDay.AddHours(1);
                }
                break;
            case ChartDataRange.Week:

                while (currentDay < chartDataRequest.FilterEnd)
                {
                    entryLabel = currentDay.ToString("ddd");
                    if (listBatteryUsed.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listBatteryUsed.Add(new DummyEntry { Value = null, Color = batteryUsedColor, Label = entryLabel });
                    if (listProductionSold.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listProductionSold.Add(new DummyEntry { Value = null, Color = soldColor, Label = entryLabel });
                    if (listProductionUsed.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listProductionUsed.Add(new DummyEntry { Value = null, Color = usedColor, Label = entryLabel });
                    if (listBatteryCharge.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listBatteryCharge.Add(new DummyEntry { Value = null, Color = batteryChargeColor, Label = entryLabel });
                    if (listCunsumtionGrid.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listCunsumtionGrid.Add(new DummyEntry { Value = null, Color = consumGridColor, Label = entryLabel });

                    currentDay = currentDay.AddDays(1);
                }

                break;
            case ChartDataRange.Month:

                while (currentDay < chartDataRequest.FilterEnd)
                {
                    entryLabel = currentDay.ToString("dd");
                    if (listBatteryUsed.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listBatteryUsed.Add(new DummyEntry { Value = null, Color = batteryUsedColor, Label = entryLabel });
                    if (listProductionSold.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listProductionSold.Add(new DummyEntry { Value = null, Color = soldColor, Label = entryLabel });
                    if (listProductionUsed.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listProductionUsed.Add(new DummyEntry { Value = null, Color = usedColor, Label = entryLabel });
                    if (listBatteryCharge.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listBatteryCharge.Add(new DummyEntry { Value = null, Color = batteryChargeColor, Label = entryLabel });
                    if (listCunsumtionGrid.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listCunsumtionGrid.Add(new DummyEntry { Value = null, Color = consumGridColor, Label = entryLabel });

                    currentDay = currentDay.AddDays(1);
                }

                break;
            case ChartDataRange.Year:

                while (currentDay < chartDataRequest.FilterEnd)
                {
                    entryLabel = currentDay.ToString("MMM");
                    if (listBatteryUsed.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listBatteryUsed.Add(new DummyEntry { Value = null, Color = batteryUsedColor, Label = entryLabel });
                    if (listProductionSold.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listProductionSold.Add(new DummyEntry { Value = null, Color = soldColor, Label = entryLabel });
                    if (listProductionUsed.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listProductionUsed.Add(new DummyEntry { Value = null, Color = usedColor, Label = entryLabel });
                    if (listBatteryCharge.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listBatteryCharge.Add(new DummyEntry { Value = null, Color = batteryChargeColor, Label = entryLabel });
                    if (listCunsumtionGrid.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listCunsumtionGrid.Add(new DummyEntry { Value = null, Color = consumGridColor, Label = entryLabel });

                    currentDay = currentDay.AddDays(1);
                }

                break;
            default:
                break;
        }


        var stats = await roiService.CalculateTotals(chartDataRequest.FilterStart, chartDataRequest.FilterEnd, false);

        string batteryChargeTitle;
        string productionSoldTile;
        string productionUsedTile;
        string batteryUsedTile;
        string consumedGridTile;
        if (chartDataRequest.ChartDataUnit == ChartDataUnit.kWh)
        {
            batteryChargeTitle = string.Format("Battery {0}", Math.Round(stats.TotalBatteryCharge, 2));
            productionSoldTile = string.Format("Sold {0}", Math.Round(stats.TotalProductionSold, 2));
            productionUsedTile = string.Format("Used {0}", Math.Round(stats.TotalProductionOwnUse, 2));

            batteryUsedTile = string.Format("Battery {0}", Math.Round(stats.TotalBatteryUsed, 2));
            consumedGridTile = string.Format("Grid {0}", Math.Round(stats.TotalPurchased, 2));

            result.ProductionChartTitle = string.Format("Production {0} kWh", Math.Round(stats.TotalBatteryCharge + stats.TotalProductionOwnUse + stats.TotalProductionSold, 0));
            result.ConsumtionChartTitle = string.Format("Consumtion {0} kWh", Math.Round(stats.TotalBatteryUsed + stats.TotalPurchased + stats.TotalProductionOwnUse, 0));
        }
        else
        {
            batteryChargeTitle = string.Format("Battery {0}", 0);
            productionSoldTile = string.Format("Sold {0}", Math.Round(stats.TotalProductionSoldProfit + stats.TotalCompensationForProductionToGrid + stats.TotalSavedEnergyTaxReductionProductionToGrid, 2));
            productionUsedTile = string.Format("Used {0}", Math.Round(stats.TotalProductionOwnUseProfit + stats.TotalSavedTransferFeeProductionOwnUse + stats.TotalSavedEnergyTaxProductionOwnUse, 2));

            batteryUsedTile = string.Format("Battery {0}", Math.Round(stats.TotalBatteryUsedProfit));
            consumedGridTile = string.Format("Grid {0}", Math.Round(stats.TotalPurchasedCost + stats.TotalPurchasedTransferFee + stats.TotalPurchasedTax, 2));

            result.ProductionChartTitle = string.Format("Production {0} Sek", 0);
            result.ConsumtionChartTitle = string.Format("Consumtion {0} Sek", 0);

        }
        List<ChartEntry> totlist = new List<ChartEntry>();
        foreach (var item in listBatteryCharge)
            totlist.Add(item.ChartEntry);
        result.ChartSeriesBatteryCharge.Add(new ChartSerie { Name = batteryChargeTitle, Color = batteryChargeColor, Entries = totlist });

        List<ChartEntry> soldlist = new List<ChartEntry>();
        foreach (var item in listProductionSold)
            soldlist.Add(item.ChartEntry);
        result.ChartSeriesProductionSold.Add(new ChartSerie { Name = productionSoldTile, Color = soldColor, Entries = soldlist });

        List<ChartEntry> usedlist = new List<ChartEntry>();
        foreach (var item in listProductionUsed)
            usedlist.Add(item.ChartEntry);
        result.ChartSeriesProductionUsed.Add(new ChartSerie { Name = productionUsedTile, Color = usedColor, Entries = usedlist });


        List<ChartEntry> batterUsedlist = new List<ChartEntry>();
        foreach (var item in listBatteryUsed)
            batterUsedlist.Add(item.ChartEntry);
        result.ChartSeriesBatteryUsed.Add(new ChartSerie { Name = batteryUsedTile, Color = batteryUsedColor, Entries = batterUsedlist });

        List<ChartEntry> consumedGridlist = new List<ChartEntry>();
        foreach (var item in listCunsumtionGrid)
            consumedGridlist.Add(item.ChartEntry);
        result.ChartSeriesConsumtionGrid.Add(new ChartSerie { Name = consumedGridTile, Color = consumGridColor, Entries = consumedGridlist });




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
public class DummyEntry
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
    public SKColor Color { get; set; } = SKColors.Black;


    /// <summary>
    /// Gets or sets the color of the rest part
    /// </summary>
    /// <value>The color of the rest part.</value>
    public SKColor OtherColor { get; set; } = SKColor.Empty;


    /// <summary>
    /// Gets or sets the color of the text (for the caption label).
    /// </summary>
    /// <value>The color of the text.</value>
    public SKColor TextColor { get; set; } = SKColors.Gray;


    /// <summary>
    /// Gets or sets the color of the value label
    /// </summary>
    /// <value>The color of the value label.</value>
    public SKColor ValueLabelColor { get; set; } = SKColors.Black;
    public ChartEntry ChartEntry { get { return new ChartEntry(Value == 0 || Value == null ? null : Convert.ToSingle(Math.Round(Value.Value, 2))) { Label = Label, ValueLabel = ValueLabel, Color = Color, OtherColor = OtherColor, TextColor = TextColor, ValueLabelColor = ValueLabelColor }; } }


}
public class ChartDataResult
{
    public List<ChartSerie> ChartSeriesBatteryCharge { get; set; } = new List<ChartSerie>();
    public List<ChartSerie> ChartSeriesProductionUsed { get; set; } = new List<ChartSerie>();
    public List<ChartSerie> ChartSeriesProductionSold { get; set; } = new List<ChartSerie>();
    public List<ChartSerie> ChartSeriesBatteryUsed { get; set; } = new List<ChartSerie>();
    public List<ChartSerie> ChartSeriesConsumtionGrid { get; set; } = new List<ChartSerie>();
    public float MaxValueYaxes { get; set; }
    public string ConsumtionChartTitle { get; set; }

    public string ProductionChartTitle { get; set; }
    
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