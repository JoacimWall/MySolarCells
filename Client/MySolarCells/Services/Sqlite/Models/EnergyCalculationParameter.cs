namespace MySolarCells.Services.Sqlite.Models;

public class EnergyCalculationParameter : ObservableObject
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int EnergyCalculationParameterId { get; set; }
    
    private DateTime fromDate = DateTime.Today;
    [Required]
    public DateTime FromDate
    {
        get => fromDate;
        set
        {
            SetProperty(ref fromDate, new DateTime(value.Year, value.Month, 1));

        }
    }
    [Required]  //Nätnytta 0.078 kr/kWh
    public double ProdCompensationElectricityLowload { get; set; } = 0.078;
    [Required]  //Eventuell överföringsavgift som du kostar vi köp av eller (sparar vi egen användning) Ellevio 0.3 kr
    public double TransferFee { get; set; } = 0.3;
    [Required] //0.60/kWh såld el Max 18 0000 kr och inte för fler kWh än huset köper in
    public double TaxReduction { get; set; } = 0.6;
    [Required] //0.49/kWh såld (sparar vi egen användning)
    public double EnergyTax { get; set; } = 0.49;
    [Required] //10.5 Kwh
    public double TotalInstallKwhPanels { get; set; } = 10.5;
    [Required] //fixed price 
    public double FixedPriceKwh { get; set; } = 0;

    private bool useSpotPrice = true;
    [Required] //fixed price 
    public bool UseSpotPrice
    {
        get => useSpotPrice;
        set
        {
            SetProperty(ref useSpotPrice, value);
            ShowFixedPrice = value ? false : true;
        }
    }
    //FK's
    [Required]
    public int HomeId { get; set; }

   
    private string displayName;
    [NotMapped]
    public string DisplayName
    {
        get { return displayName; }
        set
        {
            SetProperty(ref displayName, value);
            
        }
    }

    private bool showFixedPrice;
    [NotMapped]
    public bool ShowFixedPrice
    {
        get => showFixedPrice;
        set { SetProperty(ref showFixedPrice, value); }
    }
}

