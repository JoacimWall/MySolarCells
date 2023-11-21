namespace MySolarCells.SQLite.Sqlite.Models;

public class Preferences
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PreferencesId { get; set; }
    [Required]
    public string Name { get; set; } = "";
    public DateTime DateValue { get; set; }
    public string? StringValue { get; set; } 
    public int IntValue { get; set; }
    public double doubleValue { get; set; } 
}

