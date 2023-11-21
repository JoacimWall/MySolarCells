namespace MySolarCells.SQLite.Sqlite.Models;

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
            OnPropertyChanged(nameof(DisplayName));
        }
    }
    
    private double prodCompensationElectricityLowload = 0.078;
    [Required]  //Nätnytta 0.078 kr/kWh
    public double ProdCompensationElectricityLowload
    {
        get => prodCompensationElectricityLowload;
        set  {  SetProperty(ref prodCompensationElectricityLowload, value); }
    }

    private double transferFee = 0.3;
    [Required]  //Eventuell överföringsavgift som du kostar vi köp av eller (sparar vi egen användning) Ellevio 0.3 kr
    public double TransferFee
    {
        get => transferFee;
        set { SetProperty(ref transferFee, value); }
    }

    private double taxReduction = 0.6;
    [Required] //0.60/kWh såld el Max 18 0000 kr och inte för fler kWh än huset köper in
    public double TaxReduction
    {
        get => taxReduction;
        set { SetProperty(ref taxReduction, value); }
    }

    private double energyTax = 0.49;
    [Required] //0.49/kWh såld (sparar vi egen användning)
    public double EnergyTax
    {
        get => energyTax;
        set { SetProperty(ref energyTax, value); }
    }

    private double totalInstallKwhPanels = 10.5;
    [Required] //10.5 Kwh
    public double TotalInstallKwhPanels
    {
        get => totalInstallKwhPanels;
        set { SetProperty(ref totalInstallKwhPanels, value); }
    }

    private double fixedPriceKwh = 0;
    [Required] //fixed price 
    public double FixedPriceKwh
    {
        get => fixedPriceKwh;
        set { SetProperty(ref fixedPriceKwh, value); }
    }

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

   
    
    [NotMapped]
    public string DisplayName
    {
        get { return fromDate.ToString("yyyy-MM"); }
        
    }

    private bool showFixedPrice;
    [NotMapped]
    public bool ShowFixedPrice
    {
        get => showFixedPrice;
        set { SetProperty(ref showFixedPrice, value); }
    }
}

