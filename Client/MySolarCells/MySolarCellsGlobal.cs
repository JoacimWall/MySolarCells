namespace MySolarCells;

public class MySolarCellsGlobals
{
    private static Home selectedHome; 
    public static Home SelectedHome
    {
        get => selectedHome;
        set
        {
            selectedHome= value;
            ServiceHelper.GetService<ISettingsService>().SelectedHomeId = value == null ? 0 : value.HomeId;
        }
    }
    //public static TietoAppSettings AppSettings { get; set; }
    //public static BackendStatus BackendStatus { get; set; }
    public static DateTime LastOnSleep { get; set; }
    public static DateTime LastOnStart { get; set; }
    public static DateTime LastOnResume { get; set; }
    public static Application App { get; set; }
    public static ChartDataRequest ChartDataRequest = new ChartDataRequest();
    public static ApplicationState ApplicationState { get; set; }


    //public static SynchronizationContextAwaiter GetAwaiter(this SynchronizationContext context)
    //{
    //    return new SynchronizationContextAwaiter(context);
    //}
    #region Messages
    //public static void SendTmActivitySpinnerMessage(bool isRunning)
    //{
    //    WeakReferenceMessenger.Default.Send<TmActivitySpinnerMessage>(new TmActivitySpinnerMessage(isRunning));

    //}
    #endregion

    #region AppCenter Crashes
    public static void ReportErrorToAppCenter(Exception ex, Dictionary<string, string> info)
    {

#if DEBUG
        ConsoleWriteLineDebug("HANDLED ERROR " + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);

#endif
        //#if RELEASE
        Dictionary<string, string> logInfo = new Dictionary<string, string>();
        logInfo.Add("AppPackageName", AppInfo.PackageName);
        logInfo.Add("LastOnStart", LastOnStart.ToString());
        logInfo.Add("LastOnSleep", LastOnSleep.ToString());
        logInfo.Add("LastOnResume", LastOnResume.ToString());
        foreach (var item in info)
        {
            logInfo.Add(item.Key, item.Value);
        }

        //Crashes.TrackError(ex, logInfo);
        //#endif
    }
    public static void ReportErrorToAppCenter(Exception ex, string info)
    {

#if DEBUG
        ConsoleWriteLineDebug("HANDLED ERROR " + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);

#endif
        //#if RELEASE
        Dictionary<string, string> logInfo = new Dictionary<string, string>();
        logInfo.Add("AppPackageName", AppInfo.PackageName);
        logInfo.Add("LastOnStart", LastOnStart.ToString());
        logInfo.Add("LastOnSleep", LastOnSleep.ToString());
        logInfo.Add("LastOnResume", LastOnResume.ToString());
        if (!string.IsNullOrEmpty(info))
            logInfo.Add("Info", info);


        //Crashes.TrackError(ex, logInfo);
        //#endif
    }
    #endregion
    public static void ConsoleWriteLineDebug(Exception ex)
    {
#if DEBUG
        MainThread.BeginInvokeOnMainThread(() => { Console.WriteLine(ex); });
#endif
    }
    public static void ConsoleWriteLineDebug(string stringInfo)
    {
#if DEBUG
        MainThread.BeginInvokeOnMainThread(() => { Console.WriteLine(stringInfo); });
#endif

    }
    public static string PrivMediaFolder()
    {
        var fodlderPath = FileSystem.AppDataDirectory + "/priv_media";
        if (!Directory.Exists(fodlderPath))
        {
            Directory.SetCurrentDirectory(FileSystem.AppDataDirectory);
            Directory.CreateDirectory("priv_media");
        }
        return (FileSystem.AppDataDirectory + "/priv_media");
    }

}
public enum ApplicationState
{
    NotSet,
    Active,
    InActive,
}

