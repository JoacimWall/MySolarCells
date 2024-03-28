namespace MySolarCellsSQLite.Sqlite.Models;

public class Log : ObservableObject
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int LogId { get; set; }
    [Required]
    public DateTime CreateDate { get; set; } = DateTime.Now;
    [Required,StringLength(100)]
    public string LogTitle { get; set; } = string.Empty;
    [Required,StringLength(200)]
    public string LogDetails { get; set; } = string.Empty;
    [Required]
    public int LogTyp { get; set; } 
}

