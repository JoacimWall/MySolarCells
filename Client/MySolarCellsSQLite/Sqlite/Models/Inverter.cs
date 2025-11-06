namespace MySolarCellsSQLite.Sqlite.Models;

public class Inverter
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int InverterId { get; set; }
    [Required, StringLength(50)]
    public required string Name { get; set; } = string.Empty;
    [Required]
    public DateTime FromDate { get; set; }
    [Required, StringLength(50)]
    public string SubSystemEntityId { get; set; } = string.Empty;
    [Required]
    public int InverterTyp { get; set; }
    [StringLength(50)]
    public string? UserName { get; set; }
    [StringLength(50)]
    public string? Password { get; set; }
    [StringLength(300)]
    public string? ApiUrl { get; set; }
    [StringLength(100)]
    public string? ApiKey { get; set; }

    [Required]
    public int HomeId { get; set; }
}

