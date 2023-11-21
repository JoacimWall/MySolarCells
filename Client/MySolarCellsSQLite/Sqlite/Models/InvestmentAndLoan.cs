namespace MySolarCells.SQLite.Sqlite.Models;

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
        set { SetProperty(ref description, value); }
    }

    private DateTime fromDate = DateTime.Today;
    [Required]
    public DateTime FromDate
    {
        get => fromDate;
        set {  SetProperty(ref fromDate, new DateTime(value.Year, value.Month, 1)); }
    }

    private int investment = 0;
    [Required]
    public int Investment
    {
        get => investment;
        set { SetProperty(ref investment, value); }
    }

    private int loan = 0;
    [Required]
    public int Loan
    {
        get => loan;
        set { SetProperty(ref loan, value); }
    }

    private ObservableCollection<InvestmentAndLoanInterest> interest;
    public ObservableCollection<InvestmentAndLoanInterest> Interest
    {
        get => interest;
        set { SetProperty(ref interest, value); }
    }

    //FK's
    [Required]
    public int HomeId { get; set; }

    [NotMapped] //used for calulation of Amortization
    public int? LoanLeft { get; set; }

}

