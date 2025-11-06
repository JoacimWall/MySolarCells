namespace MySolarCells.Services;
public interface IHomeService
{
    Home CurrentHome();
    void ResetCurrenHome();
    ElectricitySupplier FirstElectricitySupplier();

    ChartDataRequest CurrentChartDataRequest();
    void SetCurrentChartDataRequest(ChartDataRequest value);
}
public class HomeService : IHomeService
{

    private readonly MscDbContext mscDbContext;
    private Home? home;
    private ChartDataRequest chartDataRequest = new();
    public HomeService(MscDbContext mscDbContext)
    {
        this.mscDbContext = mscDbContext;
    }
    public Home CurrentHome()
    {
        //Create if not exist
        if (home == null)
        {
            //Get first home
            home ??= mscDbContext.Home
                .Include(j => j.Inverters)
                .Include(j => j.ElectricitySuppliers).FirstOrDefault();
            if (home != null) return home;
            home = new Home { Name = "My home", CurrencyUnit = "Sek", EnergyUnit = "kWh" };
            mscDbContext.Home.Add(home);
            mscDbContext.SaveChanges();
            return home;
        }
        else
        {
            return home;
        }

    }

    public void ResetCurrenHome()
    {
        home = null;
    }

    public ElectricitySupplier FirstElectricitySupplier()
    {
        return CurrentHome().ElectricitySuppliers.Count == 0 ? new ElectricitySupplier { Name = "Missing", SubSystemEntityId = "Missing" } : CurrentHome().ElectricitySuppliers.First();
    }

    public ChartDataRequest CurrentChartDataRequest()
    {
        return chartDataRequest;
    }

    public void SetCurrentChartDataRequest(ChartDataRequest value)
    {

        chartDataRequest = value;
    }

}