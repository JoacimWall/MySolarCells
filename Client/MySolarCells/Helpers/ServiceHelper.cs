namespace MySolarCells.Helpers;

public static class ServiceHelper
{

    public static IServiceProvider Services { get; private set; }

    public static void Initialize(IServiceProvider serviceProvider) =>
        Services = serviceProvider;

    public static T GetService<T>() => Services.GetService<T>();
    //    public static TService GetService<TService>() => Current.GetService<TService>();

    //    public static IServiceProvider Current =>
    //#if ANDROID
    //        MauiApplication.Current.Services;
    //#elif IOS || MACCATALYST 
    //        MauiUIApplicationDelegate.Current.Services;
    //#else
    //        null;
    //#endif
    public static IGridSupplierInterface GetGridSupplierService(int electricitySupplier)
    {

        switch (electricitySupplier)
        {
            case (int)ElectricitySupplier.Unknown:
                return new TibberService(ServiceHelper.GetService<IRestClient>(), ServiceHelper.GetService<MscDbContext>());
            case (int)ElectricitySupplier.Tibber:
                return new TibberService(ServiceHelper.GetService<IRestClient>(), ServiceHelper.GetService<MscDbContext>());
           

            default:
                return null;
        }

    }

    public static IInverterServiceInterface GetInverterService(int inverterModel)
    {

        switch (inverterModel)
        {
            case (int)InverterTyp.Kostal:
                return new KostalService(ServiceHelper.GetService<IRestClient>() ,ServiceHelper.GetService<MscDbContext>());
            case (int)InverterTyp.Huawei:
                return new HuaweiService(ServiceHelper.GetService<IRestClient>(), ServiceHelper.GetService<MscDbContext>());
            case (int)InverterTyp.HomeAssistent:
                return new HomeAssistentInverterService( ServiceHelper.GetService<MscDbContext>());
            case (int)InverterTyp.SolarEdge:
                return new SolarEdgeService(ServiceHelper.GetService<IRestClient>(), ServiceHelper.GetService<MscDbContext>());

            default:
                return null;
        }

    }
}
