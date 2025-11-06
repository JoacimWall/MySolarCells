namespace MySolarCellsSQLite.Sqlite.Models;
public class ElectricitySupplier : ObservableObject
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ElectricitySupplierId { get; set; }
    [Required, StringLength(50)]
    public required string Name { get; set; }
    private DateTime fromDate = DateTime.Today;
    [Required]
    public DateTime FromDate
    {
        get => fromDate;
        set => SetProperty(ref fromDate, new DateTime(value.Year, value.Month, 1));
    }
    [Required, StringLength(50)]
    public required string SubSystemEntityId { get; set; }
    [Required]
    public int ElectricitySupplierType { get; set; }
    [Required]
    public bool ImportOnlySpotPrice { get; set; }

    [Required, StringLength(500)]
    public string ApiKey { get; set; } = string.Empty;
    [Required]
    public int HomeId { get; set; }
}

