using MySolarCellsSQLite.Sqlite;

namespace MySolarCells.Helpers;
public static class ServiceHelper
{
#nullable disable
    private static IServiceProvider Services { get; set; }

    public static void Initialize(IServiceProvider serviceProvider)
    {
        Services = serviceProvider;
    }

    public static T GetService<T>()
    {
        return Services.GetService<T>();
    }
    public static IGridSupplierInterface GetGridSupplierService(int electricitySupplier)
    {

        switch (electricitySupplier)
        {
            case (int)ElectricitySupplier.Unknown:
                return new TibberService(GetService<IRestClient>(), GetService<MscDbContext>());
            case (int)ElectricitySupplier.Tibber:
                return new TibberService(GetService<IRestClient>(), GetService<MscDbContext>());
           

            default:
                return null;
        }

    }

    public static IInverterServiceInterface GetInverterService(int inverterModel)
    {

        switch (inverterModel)
        {
            case (int)InverterTyp.Huawei:
                return new HuaweiService(GetService<IRestClient>(), GetService<MscDbContext>());
            case (int)InverterTyp.HomeAssistent:
                return new HomeAssistentInverterService( GetService<MscDbContext>());
            case (int)InverterTyp.SolarEdge:
                return new SolarEdgeService(GetService<IRestClient>(), GetService<MscDbContext>());

            default:
                return null;
        }

    }
}

