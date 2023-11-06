namespace MySolarCells.Services.Sqlite.Models;

public class Inverter
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int InverterId { get; set; }
    [Required]
    public string Name { get; set; } =string.Empty;
    [Required]
    public DateTime FromDate { get; set; }
    [Required]
    public string SubSystemEntityId { get; set; } = string.Empty;
    [Required]
    public int InverterTyp { get; set; }
    public string? UserName { get; set; } 
    public string? Password { get; set; } 
    public string? ApiUrl { get; set; } 
    public string? ApiKey { get; set; } 
    //FK's
    [Required]
    public int HomeId { get; set; }
}

