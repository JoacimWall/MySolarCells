using Microsoft.EntityFrameworkCore;
using MySolarCells.Services.Sqlite.Models;

namespace MySolarCells.Services.Sqlite;

public class MscDbContext : DbContext
{
    public MscDbContext()
    {
        SQLitePCL.Batteries_V2.Init();
        try
        {
            this.Database.EnsureCreated();
            
            //this.Database.Migrate();
        }
        catch
        {

        }

    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //använd i app mode

        var dbPath = Path.Combine(FileSystem.AppDataDirectory,"Db_v_1.db3");
        try
        {
            optionsBuilder.UseSqlite($"Filename={dbPath}");
        }
        catch
        {

        }
        //används för migrations bygga med console app
        //optionsBuilder.UseSqlite("Data Source=MyDb.db");
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //UniqueIndex
        modelBuilder.Entity<Energy>().HasIndex(u => u.Timestamp).IsUnique();
        //modelBuilder.Entity<TemplateType>().HasIndex(u => u.Type).IsUnique();
        modelBuilder.Entity<InvestmentAndLon>()
            .HasMany(c => c.Interest);
    }

    public DbSet<Models.Home> Home { get; set; }
    public DbSet<Models.Inverter> Inverter { get; set; }
    public DbSet<Models.Energy> Energy { get; set; }
    public DbSet<Models.EnergyCalculationParameter> EnergyCalculationParameter { get; set; }
    public DbSet<Models.InvestmentAndLon> InvestmentAndLon { get; set; }
    public DbSet<Models.InvestmentAndLonInterest> InvestmentAndLonInterest { get; set; }
}

