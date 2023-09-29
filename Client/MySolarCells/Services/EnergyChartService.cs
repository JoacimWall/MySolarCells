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
        List<DummyEntry> listProductionTot = new List<DummyEntry>();
        SkiaSharp.SKColor totColor = SkiaSharp.SKColor.Parse("#e8900c");
        //Production Sold
        List<DummyEntry> listProductionSold = new List<DummyEntry>();
        SkiaSharp.SKColor soldColor = SkiaSharp.SKColor.Parse("#640abf");
        //Production used
        List<DummyEntry> listProductionUsed = new List<DummyEntry>();
        SkiaSharp.SKColor usedColor = SkiaSharp.SKColor.Parse("#3498db");
        //Consumtion
        List<DummyEntry> listCunsumtion = new List<DummyEntry>();
        SkiaSharp.SKColor consumColor = SkiaSharp.SKColor.Parse("#5b46e3");

        var dataRows = await dbContext.Energy.Where(x => x.Timestamp >= chartDataRequest.FilterStart && x.Timestamp < chartDataRequest.FilterEnd).ToListAsync();
        if (dataRows != null && dataRows.Count == 0)
            return new Result<ChartDataResult>("No records",ErrorCodes.NoEnergyEntryOnCurrentDate);
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
            
                var totExist = listProductionTot.FirstOrDefault(x => x.Label == entryLabel);
                if (totExist == null)
                    listProductionTot.Add(new DummyEntry
                    { Value = chartDataRequest.ChartDataUnit == ChartDataUnit.kWh ? Convert.ToSingle(item.ProductionOwnUse + item.ProductionSold)
                    : Convert.ToSingle(item.ProductionOwnUseProfit + item.ProductionSoldProfit + (item.ProductionOwnUse * (calcparms.TransferFee + calcparms.EnergyTax) + (item.ProductionSold * (calcparms.ProdCompensationElectricityLowload + calcparms.TaxReduction)))), Color = totColor, Label = entryLabel });
                else
                {
                    totExist.Value = totExist.Value + (chartDataRequest.ChartDataUnit == ChartDataUnit.kWh ? Convert.ToSingle(item.ProductionOwnUse + item.ProductionSold)
                    : Convert.ToSingle(item.ProductionOwnUseProfit + item.ProductionSoldProfit + (item.ProductionOwnUse * (calcparms.TransferFee + calcparms.EnergyTax) + (item.ProductionSold * (calcparms.ProdCompensationElectricityLowload + calcparms.TaxReduction)))));
                }

                var soldExist = listProductionSold.FirstOrDefault(x => x.Label == entryLabel);
                if (soldExist == null)
                    listProductionSold.Add(new DummyEntry { Value = chartDataRequest.ChartDataUnit == ChartDataUnit.kWh ? Convert.ToSingle(item.ProductionSold): Convert.ToSingle(item.ProductionSoldProfit + (item.ProductionSold * (calcparms.ProdCompensationElectricityLowload + calcparms.TaxReduction))), Color = soldColor, Label = entryLabel });
                else
                {
                    soldExist.Value = soldExist.Value + (chartDataRequest.ChartDataUnit == ChartDataUnit.kWh ? Convert.ToSingle(item.ProductionSold): Convert.ToSingle(item.ProductionSoldProfit + (item.ProductionSold * (calcparms.ProdCompensationElectricityLowload + calcparms.TaxReduction)))) ;
                }

                var usedExist = listProductionUsed.FirstOrDefault(x => x.Label == entryLabel);
                if (usedExist == null)
                    listProductionUsed.Add(new DummyEntry { Value = chartDataRequest.ChartDataUnit == ChartDataUnit.kWh ? Convert.ToSingle(item.ProductionOwnUse): Convert.ToSingle(item.ProductionOwnUseProfit + (item.ProductionOwnUse * (calcparms.TransferFee + calcparms.EnergyTax))), Color = usedColor, Label = entryLabel });
                else
                {
                    usedExist.Value = usedExist.Value + (chartDataRequest.ChartDataUnit == ChartDataUnit.kWh ? Convert.ToSingle(item.ProductionOwnUse) : Convert.ToSingle(item.ProductionOwnUseProfit + (item.ProductionOwnUse * (calcparms.TransferFee + calcparms.EnergyTax))));
                }

                var consumedExist = listCunsumtion.FirstOrDefault(x => x.Label == entryLabel);
                if (consumedExist == null)
                    listCunsumtion.Add(new DummyEntry { Value = chartDataRequest.ChartDataUnit == ChartDataUnit.kWh ? Convert.ToSingle(item.Purchased): Convert.ToSingle(item.PurchasedCost + (item.Purchased * (calcparms.TransferFee + calcparms.EnergyTax))), Color = consumColor, Label = entryLabel });
                else
                {
                    consumedExist.Value = consumedExist.Value + (chartDataRequest.ChartDataUnit == ChartDataUnit.kWh ? Convert.ToSingle(item.Purchased): Convert.ToSingle(item.PurchasedCost + (item.Purchased * (calcparms.TransferFee + calcparms.EnergyTax))));
                }

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
                    if (listProductionTot.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listProductionTot.Add(new DummyEntry { Value = null, Color = totColor, Label = entryLabel });
                    if (listProductionSold.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listProductionSold.Add(new DummyEntry { Value = null, Color = totColor, Label = entryLabel });
                    if (listProductionUsed.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listProductionUsed.Add(new DummyEntry { Value = null, Color = totColor, Label = entryLabel });
                    if (listCunsumtion.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listCunsumtion.Add(new DummyEntry { Value = null, Color = totColor, Label = entryLabel });

                    currentDay = currentDay.AddHours(1);
                }
                break;
            case ChartDataRange.Week:
               
                while (currentDay < chartDataRequest.FilterEnd)
                {
                    entryLabel = currentDay.ToString("ddd");
                    if (listProductionTot.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listProductionTot.Add(new DummyEntry { Value = null, Color = totColor, Label = entryLabel });
                    if (listProductionSold.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listProductionSold.Add(new DummyEntry { Value = null, Color = totColor, Label = entryLabel });
                    if (listProductionUsed.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listProductionUsed.Add(new DummyEntry { Value = null, Color = totColor, Label = entryLabel });
                    if (listCunsumtion.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listCunsumtion.Add(new DummyEntry { Value = null, Color = totColor, Label = entryLabel });

                    currentDay = currentDay.AddDays(1);
                }
                
                break;
            case ChartDataRange.Month:

                while (currentDay < chartDataRequest.FilterEnd)
                {
                    entryLabel = currentDay.ToString("dd");
                    if (listProductionTot.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listProductionTot.Add(new DummyEntry { Value = null, Color = totColor, Label = entryLabel });
                    if (listProductionSold.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listProductionSold.Add(new DummyEntry { Value = null, Color = totColor, Label = entryLabel });
                    if (listProductionUsed.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listProductionUsed.Add(new DummyEntry { Value = null, Color = totColor, Label = entryLabel });
                    if (listCunsumtion.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listCunsumtion.Add(new DummyEntry { Value = null, Color = totColor, Label = entryLabel });

                    currentDay = currentDay.AddDays(1);
                }

                break;
            case ChartDataRange.Year:

                while (currentDay < chartDataRequest.FilterEnd)
                {
                    entryLabel = currentDay.ToString("MMM");
                    if (listProductionTot.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listProductionTot.Add(new DummyEntry { Value = null, Color = totColor, Label = entryLabel });
                    if (listProductionSold.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listProductionSold.Add(new DummyEntry { Value = null, Color = totColor, Label = entryLabel });
                    if (listProductionUsed.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listProductionUsed.Add(new DummyEntry { Value = null, Color = totColor, Label = entryLabel });
                    if (listCunsumtion.FirstOrDefault(x => x.Label == entryLabel) == null)
                        listCunsumtion.Add(new DummyEntry { Value = null, Color = totColor, Label = entryLabel });

                    currentDay = currentDay.AddDays(1);
                }

                break;
            default:
                break;
        }


        var stats = await roiService.CalculateTotals(chartDataRequest.FilterStart, chartDataRequest.FilterEnd, false);
        
        string totalProductionTitle;
        string productionSoldTile;
        string productionUsedTile;
        string ConsumedTile = "Consumed";
        if (chartDataRequest.ChartDataUnit == ChartDataUnit.kWh)
        {
            totalProductionTitle = string.Format("Total production {0} kwh", Math.Round(stats.TotalProductionSold + stats.TotalProductionOwnUse, 2));
            productionSoldTile = string.Format("Production sold {0} kwh", Math.Round(stats.TotalProductionSold, 2));
            productionUsedTile = string.Format("Production used {0} kwh", Math.Round(stats.TotalProductionOwnUse, 2));
            ConsumedTile = string.Format("Consumed({0} kwh)", Math.Round(stats.TotalPurchased, 2));
        }
        else
        {

            totalProductionTitle = string.Format("Total production {0} SEK (ink tax/fee)", Math.Round(stats.TotalProductionSoldProfit + stats.TotalProductionOwnUseProfit + stats.TotalCompensationForProductionToGrid + stats.TotalSavedTransferFeeProductionOwnUse + stats.TotalSavedEnergyTaxProductionOwnUse + stats.TotalSavedEnergyTaxReductionProductionToGrid, 2));
            productionSoldTile = string.Format("Production sold {0}  SEK (ink tax/fee)", Math.Round(stats.TotalProductionSoldProfit + stats.TotalCompensationForProductionToGrid + stats.TotalSavedEnergyTaxReductionProductionToGrid, 2));
            productionUsedTile = string.Format("Production used {0}  SEK (ink tax/fee)", Math.Round(stats.TotalProductionOwnUseProfit + stats.TotalSavedTransferFeeProductionOwnUse + stats.TotalSavedEnergyTaxProductionOwnUse, 2));
            ConsumedTile = string.Format("Consumed {0} SEK (ink tax/fee)", Math.Round(stats.TotalPurchasedCost + stats.TotalPurchasedTransferFee + stats.TotalPurchasedTax, 2));

        }
        List<ChartEntry> totlist =  new List<ChartEntry>();
        foreach (var item in listProductionTot)
            totlist.Add(item.ChartEntry);
        result.ChartSeries.Add(new ChartSerie { Name = totalProductionTitle, Color = SkiaSharp.SKColor.Parse("#e8900c"), Entries = totlist });

        List<ChartEntry> soldlist = new List<ChartEntry>();
        foreach (var item in listProductionSold)
            soldlist.Add(item.ChartEntry);
        result.ChartSeries.Add(new ChartSerie { Name = productionSoldTile, Color = SkiaSharp.SKColor.Parse("#640abf"), Entries = soldlist });

        List<ChartEntry> usedlist = new List<ChartEntry>();
        foreach (var item in listProductionUsed)
            usedlist.Add(item.ChartEntry);
        result.ChartSeries.Add(new ChartSerie { Name = productionUsedTile, Color = SkiaSharp.SKColor.Parse("#3498db"), Entries = usedlist });

        List<ChartEntry> consumedlist = new List<ChartEntry>();
        foreach (var item in listCunsumtion)
            consumedlist.Add(item.ChartEntry);
        result.ChartSeries.Add(new ChartSerie { Name = ConsumedTile, Color = SkiaSharp.SKColor.Parse("#5b46e3"), Entries = consumedlist });


        //Calc MaxValueYaxes so all graphs has the same resolution
        foreach (var item in result.ChartSeries)
        {
            if (item.Entries.Max(x => x.Value) > result.MaxValueYaxes)
                result.MaxValueYaxes = item.Entries.Max(x => x.Value).Value;
        }
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
    public ChartEntry ChartEntry { get { return new ChartEntry(Value == 0 ? null : Value) { Label = Label, ValueLabel = ValueLabel, Color = Color, OtherColor = OtherColor, TextColor = TextColor,  ValueLabelColor = ValueLabelColor }; } }
   

}
public class ChartDataResult
{
    public List<ChartSerie> ChartSeries { get; set; } = new List<ChartSerie>();
    public float MaxValueYaxes { get; set; }
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
        get{ return filterStart; }
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





    public Color TodayBackgrundColor { get { return chartDataRange == ChartDataRange.Today ? AppColors.Gray200Color : AppColors.TransparentColor;}  }
    
    public Color DayBackgrundColor { get{ return chartDataRange == ChartDataRange.Day ? AppColors.Gray200Color : AppColors.TransparentColor; } }

    public Color WeekBackgrundColor { get { return chartDataRange == ChartDataRange.Week ? AppColors.Gray200Color : AppColors.TransparentColor; }}

    public Color MonthBackgrundColor { get { return chartDataRange == ChartDataRange.Month ? AppColors.Gray200Color : AppColors.TransparentColor;} }

    public Color YearBackgrundColor { get { return chartDataRange == ChartDataRange.Year ? AppColors.Gray200Color : AppColors.TransparentColor; } }

    public Color KwhBackgrundColor { get { return ChartDataUnit == ChartDataUnit.kWh ? AppColors.Gray200Color : AppColors.TransparentColor; } }

    public Color CurrencyBackgrundColor { get { return ChartDataUnit == ChartDataUnit.Currency ? AppColors.Gray200Color : AppColors.TransparentColor; } }

    public string TimeStampTitle { get { return timeStamp.ToLongDateString(); } }
}
public enum ChartDataRange
{
    Today=0,
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