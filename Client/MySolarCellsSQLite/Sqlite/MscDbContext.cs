using Microsoft.EntityFrameworkCore;
using MySolarCells.Services.Sqlite.Models;

namespace MySolarCells.Services.Sqlite;

public class MscDbContext : DbContext
{
    // Constructor with no argument is required and it is used when adding/removing migrations from class library
    public MscDbContext()
    {

    }
    public MscDbContext(DbContextOptions<MscDbContext> options)  : base(options)
    {
        //Database.EnsureCreated();
        Database.Migrate();
    }
    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //{
    //    //använd i app mode
    //    string fodlderPath = FileSystem.AppDataDirectory;
    //    if (DeviceInfo.Platform == DevicePlatform.MacCatalyst)
    //        fodlderPath = Path.Combine(FileSystem.AppDataDirectory, "MySolarCells");

    //    if (!Directory.Exists(fodlderPath))
    //    {
    //        Directory.SetCurrentDirectory(FileSystem.AppDataDirectory);
    //        Directory.CreateDirectory("MySolarCells");
    //    }

    //    var dbPath = Path.Combine(fodlderPath, "Db_v_1.db3");
    //    try
    //    {
    //        optionsBuilder.UseSqlite($"Filename={dbPath}");
    //    }
    //    catch
    //    {

    //    }
    //    //används för migrations bygga med console app
    //    //optionsBuilder.UseSqlite("Data Source=MyDb.db");
    //}
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    => optionsBuilder.UseSqlite();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //UniqueIndex
        modelBuilder.Entity<Energy>().HasIndex(u => u.Timestamp).IsUnique();
        modelBuilder.Entity<Preferences>().HasIndex(u => u.Name).IsUnique();
        //modelBuilder.Entity<TemplateType>().HasIndex(u => u.Type).IsUnique();
        modelBuilder.Entity<InvestmentAndLoan>()
            .HasMany(c => c.Interest);
    }

    public DbSet<Home> Home { get; set; }
    public DbSet<Inverter> Inverter { get; set; }
    public DbSet<Energy> Energy { get; set; }
    public DbSet<EnergyCalculationParameter> EnergyCalculationParameter { get; set; }
    public DbSet<InvestmentAndLoan> InvestmentAndLon { get; set; }
    public DbSet<InvestmentAndLoanInterest> InvestmentAndLonInterest { get; set; }
    public DbSet<Preferences> Preferences { get; set; }
}

