namespace MySolarCells.Services;
public interface IInternetConnectionService
{
    bool InternetAccess();
    bool InternetAccess(IDialogService dialogService, bool showError = true);
    bool OnWifi();
}
public class InternetConnectionService : IInternetConnectionService
{
    private static DateTime lastMessage;

    public bool InternetAccess()
    {
        var current = Connectivity.NetworkAccess;

        if (current == NetworkAccess.Internet) return true;

        return false;
    }

    public bool InternetAccess(IDialogService dialogService, bool showError = true)
    {
        var current = Connectivity.NetworkAccess;

        if (current == NetworkAccess.Internet) return true;
        if (showError)
        {
            //Check if last mess was recently, so we don't show to many mess
            if (lastMessage > DateTime.Now)
                return false;

            dialogService.ShowAlertAsync(AppResources.Internet_Connection_Missing, "", AppResources.Ok);
            lastMessage = DateTime.Now.AddSeconds(5);
        }

        return false;
    }

    public bool OnWifi()
    {
        var profiles = Connectivity.ConnectionProfiles;
        if (profiles.Contains(ConnectionProfile.WiFi)) return true;

        return false;
    }
}