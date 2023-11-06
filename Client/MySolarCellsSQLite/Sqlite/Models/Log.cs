namespace MySolarCells.Services.Sqlite.Models;

public class Log : ObservableObject
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int LogId { get; set; }
    [Required]
    public DateTime CreateDate { get; set; } = DateTime.Now;
    [Required]
    public string LogTitle { get; set; } = string.Empty;
    [Required]
    public string LogDetails { get; set; } = string.Empty;
    [Required]
    public int LogTyp { get; set; } = 0;

}

