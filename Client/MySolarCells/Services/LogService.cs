
#if RELEASE
using Microsoft.AppCenter.Crashes;
#endif
namespace MySolarCells.Services;
public interface ILogService
{
    void ReportErrorToAppCenter(Exception ex, Dictionary<string, string> info);
    void ReportErrorToAppCenter(Exception ex, string info);
    void ConsoleWriteLineDebug(Exception ex);
    void ConsoleWriteLineDebug(string stringInfo);
}


public class TmLogService : ILogService
{
    // ReSharper disable once NotAccessedField.Local
    private readonly IInternetConnectionHelper internetConnectionHelper;

    public TmLogService(IInternetConnectionHelper internetConnectionHelper)
    {
        this.internetConnectionHelper = internetConnectionHelper;
    }

    public void ConsoleWriteLineDebug(Exception ex)
    {
#if DEBUG
        MainThread.BeginInvokeOnMainThread(() => { Console.WriteLine(ex); });
#endif
    }

    public void ConsoleWriteLineDebug(string stringInfo)
    {
#if DEBUG
        MainThread.BeginInvokeOnMainThread(() => { Console.WriteLine(stringInfo); });
#endif
    }

    #region AppCenter Crashes

    public void ReportErrorToAppCenter(Exception ex, Dictionary<string, string> info)
    {
#if DEBUG
        ConsoleWriteLineDebug("HANDLED ERROR " + Environment.NewLine + ex.Message + Environment.NewLine +
                              ex.StackTrace);
#endif
#if RELEASE
        System.Collections.Generic.Dictionary<string, string> logInfo = new Dictionary<string, string>();
        logInfo.Add("AppPackageName", AppInfo.PackageName);
        logInfo.Add("LastOnStart", TietoGlobals.LastOnStart.ToString());
        logInfo.Add("LastOnSleep", TietoGlobals.LastOnSleep.ToString());
        logInfo.Add("LastOnResume", TietoGlobals.LastOnResume.ToString());
        logInfo.Add("HasInternetConnection", this.internetConnectionHelper.InternetAccess().ToString());
        logInfo.Add("HasReceivedMemoryWarningInLastSession",Crashes.HasReceivedMemoryWarningInLastSessionAsync().Result.ToString());

        foreach (var item in info)
        {
            logInfo.Add(item.Key, item.Value);
        }
        
        Crashes.TrackError(ex, logInfo);
#endif
    }

    public void ReportErrorToAppCenter(Exception ex, string info)
    {
#if DEBUG
        ConsoleWriteLineDebug("HANDLED ERROR " + Environment.NewLine + ex.Message + Environment.NewLine +
                              ex.StackTrace);
#endif
#if RELEASE
        Dictionary<string, string> logInfo = new Dictionary<string, string>();
        logInfo.Add("AppPackageName", AppInfo.PackageName);
        logInfo.Add("LastOnStart", TietoGlobals.LastOnStart.ToString());
        logInfo.Add("LastOnSleep", TietoGlobals.LastOnSleep.ToString());
        logInfo.Add("LastOnResume", TietoGlobals.LastOnResume.ToString());
        logInfo.Add("HasInternetConnection", this.internetConnectionHelper.InternetAccess().ToString());
        logInfo.Add("HasReceivedMemoryWarningInLastSession", Crashes.HasReceivedMemoryWarningInLastSessionAsync().Result.ToString());
        if (!string.IsNullOrEmpty(info))
            logInfo.Add("Info", info);


        Crashes.TrackError(ex, logInfo);
#endif
    }

    #endregion
}