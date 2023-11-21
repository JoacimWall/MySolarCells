namespace MySolarCells.SQLite.Sqlite.Models;

public class SavingEssitmateParameters : ObservableObject
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SavingEssitmateParametersId { get; set; }
    private DateTime fromDate = DateTime.Today;
    [Required]
    public DateTime FromDate
    {
        get => fromDate;
        set {   SetProperty(ref fromDate, new DateTime(value.Year, value.Month, 1));  }
    }

    private double realDevelopmentElectricityPrice = 1.05;
    [Required]  //1.05;
    public double RealDevelopmentElectricityPrice
    {
        get => realDevelopmentElectricityPrice;
        set { SetProperty(ref realDevelopmentElectricityPrice, value); }
    }

    private double panelDegradationPerYear = 0.25;
    [Required]
    public double PanelDegradationPerYear
    {
        get => panelDegradationPerYear;
        set { SetProperty(ref panelDegradationPerYear, value); }
    }

    //FK's
    public int HomeId { get; set; } = 1;
}

