namespace MySolarCells.Services.Sqlite.Models;

public class Home
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int HomeId { get; set; }
    [Required]
    public string Name { get; set; }
    [Required]
    public DateTime FromDate { get; set; }
    [Required]
    public string SubSystemEntityId { get; set; }
    [Required]
    public int ElectricitySupplier { get; set; }
    [Required]
    public string ApiKey { get; set; } = string.Empty;
}

