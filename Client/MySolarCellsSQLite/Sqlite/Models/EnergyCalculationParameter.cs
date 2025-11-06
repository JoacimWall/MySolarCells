namespace MySolarCellsSQLite.Sqlite.Models;

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

    private double prodCompensationElectricityLowLoad = 0.078;
    [Required]  //Net benefit SEK 0.078/kWh
    public double ProdCompensationElectricityLowLoad
    {
        get => prodCompensationElectricityLowLoad;
        set => SetProperty(ref prodCompensationElectricityLowLoad, value);
    }

    private double transferFee = 0.3;
    [Required]  //Any transfer fee that costs you if we buy from or (we save for our own use) Ellevio SEK 0.3
    public double TransferFee
    {
        get => transferFee;
        set => SetProperty(ref transferFee, value);
    }

    private double taxReduction = 0.6;
    [Required] //0.60/kWh electricity sold Max SEK 18,0000 and not for more kWh than the house buys in
    public double TaxReduction
    {
        get => taxReduction;
        set => SetProperty(ref taxReduction, value);
    }

    private double energyTax = 0.49;
    [Required] //0.49/kWh sold (we save our own use)
    public double EnergyTax
    {
        get => energyTax;
        set => SetProperty(ref energyTax, value);
    }

    private double totalInstallKwhPanels = 10.5;
    [Required] //10.5 Kwh
    public double TotalInstallKwhPanels
    {
        get => totalInstallKwhPanels;
        set => SetProperty(ref totalInstallKwhPanels, value);
    }

    private double fixedPriceKwh;
    [Required] //fixed price 
    public double FixedPriceKwh
    {
        get => fixedPriceKwh;
        set => SetProperty(ref fixedPriceKwh, value);
    }

    private bool useSpotPrice = true;
    [Required] //fixed price 
    public bool UseSpotPrice
    {
        get => useSpotPrice;
        set
        {
            SetProperty(ref useSpotPrice, value);
            ShowFixedPrice = !value;
        }
    }
    [Required]
    public int ElectricitySupplierId { get; set; }



    [NotMapped]
    public string DisplayName => fromDate.ToString("yyyy-MM");

    private bool showFixedPrice;
    [NotMapped]
    public bool ShowFixedPrice
    {
        get => showFixedPrice;
        set => SetProperty(ref showFixedPrice, value);
    }
}

