namespace MySolarCells.Services.Sqlite.Models;

public class Home : ObservableObject
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int HomeId { get; set; }
    [Required]
    public string Name { get; set; }
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
    [Required]
    public string SubSystemEntityId { get; set; }
    [Required]
    public int ElectricitySupplier { get; set; }
    [Required]
    public bool ImportOnlySpotPrice { get; set; }
    
    [Required]
    public string ApiKey { get; set; } = string.Empty;
}

