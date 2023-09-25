namespace MySolarCells.Helpers;

public static class ServiceHelper
{
    public static TService GetService<TService>() => Current.GetService<TService>();

    public static IServiceProvider Current =>
#if ANDROID
        MauiApplication.Current.Services;
#elif IOS
        MauiUIApplicationDelegate.Current.Services;
#else
        null;
#endif

}
