namespace MySolarCells.Services.Sqlite.Models;

public class InvestmentAndLonInterest
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int InvestmentAndLonInterestId { get; set; }
    [Required]
    public string Description { get; set; }
    [Required]
    public float Interest { get; set; } = 0;
    [Required]
    public int Amortization { get; set; } = 0;
    
    [Required]
    public DateTime FromDate { get; set; } = DateTime.Today;
    //FK's
    [Required]
    public int InvestmentAndLonId { get; set; }
}

