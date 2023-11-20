using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;
using MySolarCells.Services.Sqlite.Models;
using MySolarCellsSQLite.Sqlite.Models;

namespace MySolarCells.Services.Sqlite;

public class MscDbContext : DbContext
{
    //---------- THIS CODE SHOULD BE ENABLE WEHEN WE CREATE MIGRATION FROM TERMINAL PROMPT -------------------------------
    // Constructor with no argument/empty is required and it is used when adding/removing migrations from class library
    //public MscDbContext()
    //{
    //}
    //protected override void OnConfiguring(DbContextOptionsBuilder options)
    //        => options.UseSqlite();


    //---------- THIS CODE SHOULD BE ENABLE WEHEN WE RUN THE APP -------------------------------
    public MscDbContext()
    {
        SQLitePCL.Batteries_V2.Init();
        try
        {
            //this.Database.EnsureCreated();
            this.Database.Migrate();
        }
        catch (Exception e)
        {

        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //använd i app mode
        string fodlderPath = FileSystem.AppDataDirectory;
        string filename = "Db_v_1.db3";
        if (DeviceInfo.Platform == DevicePlatform.MacCatalyst)
        {
            //detta som mac catalyst local
            fodlderPath = Path.Combine(fodlderPath, "MySolarCells");
            if (!Directory.Exists(fodlderPath))
            {
                Directory.SetCurrentDirectory(FileSystem.AppDataDirectory);
                Directory.CreateDirectory("MySolarCells");
            }
        }
        try
        {
            optionsBuilder.UseSqlite($"Filename={Path.Combine(fodlderPath, filename)}");
        }
        catch
        {

        }



    }

    // ----------------------- ALLWAYS ENABLED --------------------------------

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //UniqueIndex
        modelBuilder.Entity<Energy>().HasIndex(u => u.Timestamp).IsUnique();
        modelBuilder.Entity<Models.Preferences>().HasIndex(u => u.Name).IsUnique();
        modelBuilder.Entity<EnergyCalculationParameter>().HasIndex(u => u.FromDate).IsUnique();
        modelBuilder.Entity<InvestmentAndLoan>().HasIndex(u => u.FromDate).IsUnique();
        modelBuilder.Entity<InvestmentAndLoan>().HasMany(c => c.Interest);
        modelBuilder.Entity<InvestmentAndLoanInterest>().HasIndex(u => new { u.FromDate, u.InvestmentAndLoanId }).IsUnique();
    }

    public DbSet<Home> Home { get; set; }
    public DbSet<Inverter> Inverter { get; set; }
    public DbSet<Energy> Energy { get; set; }
    public DbSet<EnergyCalculationParameter> EnergyCalculationParameter { get; set; }
    public DbSet<InvestmentAndLoan> InvestmentAndLon { get; set; }
    public DbSet<InvestmentAndLoanInterest> InvestmentAndLonInterest { get; set; }
    public DbSet<Models.Preferences> Preferences { get; set; }
    public DbSet<Log> Log { get; set; }
    public DbSet<SavingEssitmateParameters> SavingEssitmateParameters { get; set; }
    
}

