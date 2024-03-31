namespace MySolarCellsSQLite.Sqlite.Models;

public class PowerTariffParameters : ObservableObject
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PowerTariffParametersId { get; set; }
    private DateTime fromDate = DateTime.Today;
    [Required]
    public DateTime FromDate
    {
        get => fromDate;
        set{  SetProperty(ref fromDate, new DateTime(value.Year, value.Month, 1));
            OnPropertyChanged(nameof(DisplayName));
        }
    }

    private int amountOfPeaksToUse = 1;
    [Required]
    public int AmountOfPeaksToUse
    {
        get => amountOfPeaksToUse;
        set => SetProperty(ref amountOfPeaksToUse, value);
    }

    private bool usePeaksFromSameDay ;
    [Required]
    public bool UsePeaksFromSameDay
    {
        get => usePeaksFromSameDay;
        set => SetProperty(ref usePeaksFromSameDay, value);
    }

    private double pricePerKwh;
    [Required]
    public double PricePerKwh
    {
        get => pricePerKwh;
        set => SetProperty(ref pricePerKwh, value);
    }

    // ----- PERIOD ----------
    private int periodMonthStart = 1;
    [Required]
    public int PeriodMonthStart
    {
        get => periodMonthStart;
        set => SetProperty(ref periodMonthStart, value);
    }
    //the calculation applies to the whole of this month
    private int periodMonthEnd = 12;
    [Required]
    public int PeriodMonthEnd
    {
        get => periodMonthEnd;
        set => SetProperty(ref periodMonthEnd, value);
    }

    // ----- WEEK DAY ----------
    private bool weekend =true;
    [Required]
    public bool Weekend
    {
        get => weekend;
        set => SetProperty(ref weekend, value);
    }
    private bool weekday = true;
    [Required]
    public bool Weekday
    {
        get => weekday;
        set => SetProperty(ref weekday, value);
    }

    // ----- HOURS EVERY DAY ----------
    private int dayTimeStart;
    [Required]
    public int DayTimeStart
    {
        get => dayTimeStart;
        set => SetProperty(ref dayTimeStart, value);
    }
    private int dayTimeEnd = 23;
    [Required]
    public int DayTimeEnd
    {
        get => dayTimeEnd;
        set => SetProperty(ref dayTimeEnd, value);
    }
   
    [Required]
    public int ElectricitySupplierId { get; set; }

    [NotMapped]
    public string DisplayName => fromDate.ToString("yyyy-MM");
}

