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
    public DbSet<Home> Home { get; set; }
    public DbSet<Inverter> Inverter { get; set; }
    public DbSet<Energy> Energy { get; set; }
    public DbSet<EnergyCalculationParameter> EnergyCalculationParameter { get; set; }
}

