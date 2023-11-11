

namespace MySolarCells.Services.Sqlite.Models;

public class InvestmentAndLoan : ObservableObject
{
    public InvestmentAndLoan()
    {
        Interest = new ObservableCollection<InvestmentAndLoanInterest>();

    }
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int InvestmentAndLoanId { get; set; }
    private string description;
    [Required]
    public string Description
    {
        get => description;
        set
        {
            SetProperty(ref description, value);
           
        }
    }
private DateTime fromDate = DateTime.Today;
    [Required]
    public DateTime FromDate
    {
        get => fromDate;
        set
        {
            SetProperty(ref fromDate, new DateTime(value.Year, value.Month, 1));
           
        }
    }
    [Required]
    public int Investment { get; set; } = 0;
    [Required]
    public int Loan { get; set; } = 0;
    
    public ObservableCollection<InvestmentAndLoanInterest> Interest { get; set; }

    //FK's
    [Required]
        public int HomeId { get; set; }

    [NotMapped] //used for calulation of Amortization
    public int? LoanLeft { get; set; }

}

