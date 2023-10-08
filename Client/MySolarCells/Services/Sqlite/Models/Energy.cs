namespace MySolarCells.Services.Sqlite.Models;

public class Energy
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int EnergyId { get; set; }
    [Required]
    public DateTime Timestamp { get; set; }
    [Required]
    public double ProductionSold { get; set; } = 0;
    [Required]
    public double ProductionOwnUse { get; set; } = 0;
    [Required]
    public double Purchased { get; set; } = 0;
    [Required]
    public double BatteryCharge { get; set; } = 0;
    [Required]
    public double BatteryUsed { get; set; } = 0;
    [Required]
    public string Unit { get; set; } = "kWh";
    [Required]
    public string Currency { get; set; } = "SEK";
    [Required]
    public double ProductionSoldProfit { get; set; } = 0;
    [Required]
    public double ProductionOwnUseProfit { get; set; } = 0;
    [Required]
    public double PurchasedCost { get; set; } = 0;
    //[Required]
    //public double BatteryChargeProfit { get; set; } = 0;
    [Required]
    public double BatteryUsedProfit { get; set; } = 0;
    [Required]
    public double UnitPriceBuy { get; set; } = 0;
    [Required]
    public double UnitPriceVatBuy { get; set; } = 0;
    [Required]
    public double UnitPriceSold { get; set; } = 0;
    [Required]
    public double UnitPriceVatSold { get; set; } = 0;
    [Required]
    public int ElectricitySupplierProductionSold { get; set; } = 0;
    [Required]
    public int InverterTypProductionOwnUse { get; set; } = 0;
    [Required]
    public int ElectricitySupplierPurchased { get; set; } = 0;
    [Required]
    public bool ProductionSoldSynced { get; set; } = false;
    [Required]
    public bool ProductionOwnUseSynced { get; set; } = false;
    [Required]
    public bool PurchasedSynced { get; set; } = false;
    [Required]
    public bool BatterySynced { get; set; } = false;
    //FK's
    [Required]
    public int HomeId  { get; set; } 
}


