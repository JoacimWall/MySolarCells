namespace MySolarCellsSQLite.Sqlite.Models;

public class Preferences
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PreferencesId { get; set; }
    [Required, StringLength(50)]
    public string Name { get; set; } = "";
    public DateTime DateValue { get; set; }
    [StringLength(100)]
    public string? StringValue { get; set; }
    public int IntValue { get; set; }
    public double DoubleValue { get; set; }
}

