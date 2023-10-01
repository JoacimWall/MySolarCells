namespace MySolarCells.Services.Sqlite.Models;

public class InvestmentAndLon : ObservableObject
{
    public InvestmentAndLon()
    {
        Interest = new ObservableCollection<InvestmentAndLonInterest>();

    }
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int InvestmentAndLonId { get; set; }
    [Required]
    public string Description { get; set; }
    [Required]
    public DateTime FromDate { get; set; } = DateTime.Today;
    [Required]
    public int Investment { get; set; } = 0;
    [Required]
    public int Lon { get; set; } = 0;
    
    public ObservableCollection<InvestmentAndLonInterest> Interest { get; set; }

    //FK's
    [Required]
        public int HomeId { get; set; }

}

