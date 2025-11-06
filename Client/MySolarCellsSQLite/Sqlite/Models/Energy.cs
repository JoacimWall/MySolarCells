namespace MySolarCellsSQLite.Sqlite.Models;

public class Energy
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int EnergyId { get; set; }
    [Required]
    public DateTime Timestamp { get; set; }
    [Required]
    public double ProductionSold { get; set; }
    [Required]
    public double ProductionOwnUse { get; set; }
    [Required]
    public double Purchased { get; set; }
    [Required]
    public double BatteryCharge { get; set; }
    [Required]
    public double BatteryUsed { get; set; }
    // [Required,StringLength(10)]
    // public string Unit { get; set; } = "kWh";
    // [Required,StringLength(10)]
    // public string Currency { get; set; } = "SEK";
    [Required]
    public double ProductionSoldProfit { get; set; }
    [Required]
    public double ProductionOwnUseProfit { get; set; }
    [Required]
    public double PurchasedCost { get; set; }
    [Required]
    public double BatteryUsedProfit { get; set; }
    [Required]
    public double UnitPriceBuy { get; set; }
    [Required]
    public double UnitPriceVatBuy { get; set; }
    [Required]
    public double UnitPriceSold { get; set; }
    [Required]
    public double UnitPriceVatSold { get; set; }
    [Required, StringLength(20)]
    public string PriceLevel { get; set; } = "";
    [Required]
    public int ElectricitySupplierProductionSold { get; set; }
    [Required]
    public int InverterTypeProductionOwnUse { get; set; }
    [Required]
    public int ElectricitySupplierPurchased { get; set; }
    [Required]
    public bool ProductionSoldSynced { get; set; }
    [Required]
    public bool ProductionOwnUseSynced { get; set; }
    [Required]
    public bool PurchasedSynced { get; set; }
    [Required]
    public bool BatterySynced { get; set; }

    [Required]
    public int HomeId { get; set; }
}


