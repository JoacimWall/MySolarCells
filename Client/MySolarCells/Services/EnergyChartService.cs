using System.Collections.Generic;
using System.Threading.Tasks;
using Microcharts;
using SkiaSharp;

namespace MySolarCells.Services;

public interface IEnergyChartService
{
    Task<Result<ChartDataResult>> GetChartData(ChartDataRequest chartDataRequest);
}

public class EnergyChartService : IEnergyChartService
{
	public EnergyChartService()
    {
	}
    public async Task<Result<ChartDataResult>> GetChartData(ChartDataRequest chartDataRequest)
    {
        using var dbContext = new MscDbContext();
        ChartDataResult result = new ChartDataResult();
        DateTime start = new DateTime();
        DateTime end = new DateTime();
        string entryLabel = string.Empty;

        DateTime baseDate = new DateTime(chartDataRequest.TimeStamp.Year, chartDataRequest.TimeStamp.Month, chartDataRequest.TimeStamp.Day);

        var today = baseDate;
        var yesterday = baseDate.AddDays(-1);
        var thisWeekStart = baseDate.AddDays(-(int)baseDate.DayOfWeek);
        //fix for sunday
        if (thisWeekStart.DayOfWeek != DayOfWeek.Monday)
            thisWeekStart = thisWeekStart.AddDays(1);

        var thisWeekEnd = thisWeekStart.AddDays(7).AddSeconds(-1);
        var lastWeekStart = thisWeekStart.AddDays(-7);
        var lastWeekEnd = thisWeekStart.AddSeconds(-1);
        var thisMonthStart = baseDate.AddDays(1 - baseDate.Day);
        var thisMonthEnd = thisMonthStart.AddMonths(1).AddSeconds(-1);
        var lastMonthStart = thisMonthStart.AddMonths(-1);
        var lastMonthEnd = thisMonthStart.AddSeconds(-1);
        var thisYearStart = new DateTime(thisMonthStart.Year, 1, 1);
        var thisYearhEnd = new DateTime(thisMonthStart.Year, 12, 31).AddDays(1);

        switch (chartDataRequest.ChartDataRange)
        {
            case ChartDataRange.Today:
            case ChartDataRange.Day:
                start = new DateTime(chartDataRequest.TimeStamp.Year, chartDataRequest.TimeStamp.Month, chartDataRequest.TimeStamp.Day);
                end = start.AddDays(1);
                break;
            case ChartDataRange.Week:
                start = thisWeekStart;
                end = thisWeekEnd;
                break;
            case ChartDataRange.Month:
                start = thisMonthStart;
                end = thisMonthEnd;
                break;
            case ChartDataRange.Year:
                start = thisYearStart;
                end = thisYearhEnd;
                break;
            default:
                break;
        }

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

        var dataRows = await dbContext.Energy.Where(x => x.Timestamp >= start && x.Timestamp < end).ToListAsync();
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
                    listProductionTot.Add(new DummyEntry { Value = Convert.ToSingle(item.ProductionOwnUse + item.ProductionSold), Color = totColor, Label = entryLabel });
                else
                {
                    totExist.Value = totExist.Value + Convert.ToSingle(item.ProductionOwnUse + item.ProductionSold);
                }

                var soldExist = listProductionSold.FirstOrDefault(x => x.Label == entryLabel);
                if (soldExist == null)
                    listProductionSold.Add(new DummyEntry { Value = Convert.ToSingle(item.ProductionSold), Color = soldColor, Label = entryLabel });
                else
                {
                    soldExist.Value = soldExist.Value + Convert.ToSingle(item.ProductionSold);
                }

                var usedExist = listProductionUsed.FirstOrDefault(x => x.Label == entryLabel);
                if (usedExist == null)
                    listProductionUsed.Add(new DummyEntry { Value = Convert.ToSingle(item.ProductionOwnUse), Color = usedColor, Label = entryLabel });
                else
                {
                    usedExist.Value = usedExist.Value + Convert.ToSingle(item.ProductionOwnUse);
                }

                var consumedExist = listCunsumtion.FirstOrDefault(x => x.Label == entryLabel);
                if (consumedExist == null)
                    listCunsumtion.Add(new DummyEntry { Value = Convert.ToSingle(item.Purchased), Color = consumColor, Label = entryLabel });
                else
                {
                    consumedExist.Value = consumedExist.Value + Convert.ToSingle(item.Purchased);
                }

            }
        
        //Fill missing days
        var currentDay = start;
        var enddayfil = start.AddDays(1);
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
               
                while (currentDay < end)
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

                while (currentDay < end)
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

                while (currentDay < end)
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


        List<ChartEntry> totlist =  new List<ChartEntry>();
        foreach (var item in listProductionTot)
            totlist.Add(item.ChartEntry);
        result.ChartSeries.Add(new ChartSerie { Name = "Production", Color = SkiaSharp.SKColor.Parse("#e8900c"), Entries = totlist });

        List<ChartEntry> soldlist = new List<ChartEntry>();
        foreach (var item in listProductionSold)
            soldlist.Add(item.ChartEntry);
        result.ChartSeries.Add(new ChartSerie { Name = "Sold", Color = SkiaSharp.SKColor.Parse("#640abf"), Entries = soldlist });

        List<ChartEntry> usedlist = new List<ChartEntry>();
        foreach (var item in listProductionUsed)
            usedlist.Add(item.ChartEntry);
        result.ChartSeries.Add(new ChartSerie { Name = "used", Color = SkiaSharp.SKColor.Parse("#3498db"), Entries = usedlist });

        List<ChartEntry> consumedlist = new List<ChartEntry>();
        foreach (var item in listCunsumtion)
            consumedlist.Add(item.ChartEntry);
        result.ChartSeries.Add(new ChartSerie { Name = "Cunsumtion", Color = SkiaSharp.SKColor.Parse("#5b46e3"), Entries = consumedlist });


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
        }
    }

    private DateTime timeStamp =  DateTime.Now;

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
        }
    }
   
    public Color TodayBackgrundColor { get { return chartDataRange == ChartDataRange.Today ? AppColors.Gray200Color : AppColors.TransparentColor;}  }
    
    public Color DayBackgrundColor { get{ return chartDataRange == ChartDataRange.Day ? AppColors.Gray200Color : AppColors.TransparentColor; } }

    public Color WeekBackgrundColor { get { return chartDataRange == ChartDataRange.Week ? AppColors.Gray200Color : AppColors.TransparentColor; }}

    public Color MonthBackgrundColor { get { return chartDataRange == ChartDataRange.Month ? AppColors.Gray200Color : AppColors.TransparentColor;} }

    public Color YearBackgrundColor { get { return chartDataRange == ChartDataRange.Year ? AppColors.Gray200Color : AppColors.TransparentColor; } }

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