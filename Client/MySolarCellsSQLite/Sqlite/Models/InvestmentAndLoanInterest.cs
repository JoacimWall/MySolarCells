namespace MySolarCells.SQLite.Sqlite.Models;

public class InvestmentAndLoanInterest : ObservableObject
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int InvestmentAndLoanInterestId { get; set; }

    private string description="";
    [Required]
    public string Description
    {
        get => description;
        set { SetProperty(ref description, value); }
    }

    private float interest = 0;
    [Required]  //1.05;
    public float Interest
    {
        get => interest;
        set { SetProperty(ref interest, value); }
    }

    private DateTime fromDate = DateTime.Today;
    [Required]
    public DateTime FromDate
    {
        get =>  fromDate;
        set  {  SetProperty(ref fromDate, new DateTime(value.Year,value.Month,1)); }
    }

    private int amortization = 0;
    [Required]
    public int Amortization
    {
        get => amortization;
        set { SetProperty(ref amortization, value); }
    }
    //FK's
    [Required]
    public int InvestmentAndLoanId { get; set; }

    [NotMapped]
    public string Name
    {
        get { return FromDate.ToString("yyyy-MM"); }
    }
}
