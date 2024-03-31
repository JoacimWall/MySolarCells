namespace MySolarCellsSQLite.Sqlite.Models;

public  class Home : ObservableObject
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int HomeId { get; set; }
    [Required,StringLength(50)]
    public required string Name { get; set; }

    [Required,StringLength(5)]
    public required string EnergyUnit { get; set; } = "kWh";
   

    [Required, StringLength(5)] public required string CurrencyUnit { get; set; } = "Sek";
    private ObservableCollection<ElectricitySupplier> electricitySuppliers = new ObservableCollection<ElectricitySupplier>();
    public ObservableCollection<ElectricitySupplier> ElectricitySuppliers
    {
        get => electricitySuppliers;
        set => SetProperty(ref electricitySuppliers, value);
    }
    private ObservableCollection<Inverter> inverters = new ObservableCollection<Inverter>();
    public ObservableCollection<Inverter> Inverters
    {
        get => inverters;
        set => SetProperty(ref inverters, value);
    }
}