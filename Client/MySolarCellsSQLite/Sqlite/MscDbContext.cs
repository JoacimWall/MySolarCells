using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;
using MySolarCellsSQLite.Sqlite.Models;
using Preferences = MySolarCellsSQLite.Sqlite.Models.Preferences;

namespace MySolarCellsSQLite.Sqlite;

public class MscDbContext : DbContext
{
    //---------- THIS CODE SHOULD BE ENABLED WHEN WE CREATE MIGRATION FROM TERMINAL PROMPT -------------------------------
    // Constructor with no argument/empty is required, and it is used when adding/removing migrations from class library
    // public MscDbContext()
    // {
    // }
    // protected override void OnConfiguring(DbContextOptionsBuilder options)
    //         => options.UseSqlite();


    //---------- THIS CODE SHOULD BE ENABLE WHEN WE RUN THE APP -------------------------------
    public MscDbContext()
    {
        SQLitePCL.Batteries_V2.Init();
        try
        {
            Database.Migrate();
        }
        catch (Exception)
        {
            // ignored
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var folderPath = FileSystem.AppDataDirectory;
        const string filename = "Db_v_2.db3";
        if (DeviceInfo.Platform == DevicePlatform.MacCatalyst)
        {
            //detta som mac catalyst local
            folderPath = Path.Combine(folderPath, "MySolarCells");
            if (!Directory.Exists(folderPath))
            {
                Directory.SetCurrentDirectory(FileSystem.AppDataDirectory);
                Directory.CreateDirectory("MySolarCells");
            }
        }
        try
        {
            optionsBuilder.UseSqlite($"Filename={Path.Combine(folderPath, filename)}");
        }
        catch
        {
            // ignored
        }
    }

    // ----------------------- ALWAYS ENABLED --------------------------------

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //UniqueIndex
        modelBuilder.Entity<Energy>().HasIndex(u => u.Timestamp).IsUnique();
        modelBuilder.Entity<Preferences>().HasIndex(u => u.Name).IsUnique();
        modelBuilder.Entity<EnergyCalculationParameter>().HasIndex(u => u.FromDate).IsUnique();
        modelBuilder.Entity<InvestmentAndLoan>().HasIndex(u => u.FromDate).IsUnique();
        modelBuilder.Entity<InvestmentAndLoan>().HasMany(c => c.Interest);
        modelBuilder.Entity<InvestmentAndLoanInterest>().HasIndex(u => new { u.FromDate, u.InvestmentAndLoanId }).IsUnique();
    }
    public DbSet<Home> Home { get; set; }
    public DbSet<ElectricitySupplier> ElectricitySupplier { get; set; }
    public DbSet<Inverter> Inverter { get; set; }
    public DbSet<Energy> Energy { get; set; }
    public DbSet<EnergyCalculationParameter> EnergyCalculationParameter { get; set; }
    public DbSet<InvestmentAndLoan> InvestmentAndLon { get; set; }
    public DbSet<InvestmentAndLoanInterest> InvestmentAndLonInterest { get; set; }
    public DbSet<Preferences> Preferences { get; set; }
    public DbSet<Log> Log { get; set; }
    public DbSet<SavingEstimateParameters> SavingEstimateParameters { get; set; }
    public DbSet<PowerTariffParameters> PowerTariffParameters { get; set; }


}

