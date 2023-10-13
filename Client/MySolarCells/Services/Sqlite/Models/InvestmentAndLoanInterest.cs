namespace MySolarCells.Services.Sqlite.Models;

public class InvestmentAndLoanInterest
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int InvestmentAndLoanInterestId { get; set; }
    [Required]
    public string Description { get; set; }
    [Required]
    public float Interest { get; set; } = 0;
    [Required]
    public DateTime FromDate { get; set; } = DateTime.Today;
    //FK's
    [Required]
    public int InvestmentAndLoanId { get; set; }

    [NotMapped]
    public string Name
    {
        get { return FromDate.ToShortDateString(); }
    }
}
