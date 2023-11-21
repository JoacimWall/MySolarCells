namespace MySolarCells.SQLite.Sqlite.Models;

public class InvestmentAndLoanInterest : ObservableObject
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int InvestmentAndLoanInterestId { get; set; }
    [Required]
    public string Description { get; set; } = "";
    [Required]
    public float Interest { get; set; } = 0;
    private DateTime fromDate = DateTime.Today;
    [Required]
    public DateTime FromDate
    {
        get =>  fromDate;
        set
        {
            SetProperty(ref fromDate, new DateTime(value.Year,value.Month,1));
            
        }
    }
    [Required]
    public int Amortization { get; set; } = 0;
    //FK's
    [Required]
    public int InvestmentAndLoanId { get; set; }

    [NotMapped]
    public string Name
    {
        get { return FromDate.ToString("yyyy-MM"); }
    }
}
