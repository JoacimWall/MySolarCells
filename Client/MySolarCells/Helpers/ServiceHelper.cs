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
            case (int)ElectricitySupplierEnum.Unknown:
                return new TibberService(GetService<IRestClient>(), GetService<MscDbContext>(), GetService<IHomeService>(), GetService<ILogService>());
            case (int)ElectricitySupplierEnum.Tibber:
                return new TibberService(GetService<IRestClient>(), GetService<MscDbContext>(), GetService<IHomeService>(), GetService<ILogService>());


            default:
                return null;
        }

    }

    public static IInverterServiceInterface GetInverterService(int inverterModel)
    {

        switch (inverterModel)
        {
            case (int)InverterTypeEnum.Huawei:
                return new HuaweiService(GetService<IRestClient>(), GetService<MscDbContext>(), GetService<IHomeService>(), GetService<ILogService>());
            case (int)InverterTypeEnum.HomeAssistent:
                return new HomeAssistentInverterService(GetService<MscDbContext>(), GetService<IHomeService>(), GetService<ILogService>());
            case (int)InverterTypeEnum.SolarEdge:
                return new SolarEdgeService(GetService<IRestClient>(), GetService<MscDbContext>(), GetService<IHomeService>());

            default:
                return null;
        }

    }
}

