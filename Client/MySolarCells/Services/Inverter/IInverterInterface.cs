namespace MySolarCells.Services.Inverter;

public interface IInverterServiceInterface
{
    Task<Result<InverterLoginResponse>> TestConnection(string userName, string password, string apiUrl, string apiKey);
    Task<Result<List<InverterSite>>> GetPickerOne();
    Task<Result<GetInverterResponse>> GetInverter(InverterSite inverterSite);
    Task<Result<DataSyncResponse>> Sync(DateTime start, IProgress<int> progress, int progressStartNr);

    string InverterGuideText { get; }
    string DefaultApiUrl { get; }
    bool ShowUserName { get; }
    bool ShowPassword { get; }
    bool ShowApiUrl { get; }

    bool ShowApiKey { get; }

}
public class InverterLoginResponse
{

    public string token { get; set; } = "";
    public string tokenType { get; set; } = "";
    public int expiresIn { get; set; }
}
public class SiteListResponse
{

    public string token { get; set; } = "";
    public string tokenType { get; set; } = "";
    public int expiresIn { get; set; }
}
public class InverterSite
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string InverterName { get; set; } = "";
    public DateTime InstallationDate { get; set; } = DateTime.Today;
}
public class GetInverterResponse
{

    public string InverterId { get; set; } = "";
    public string Name { get; set; } = "";

}
public class DataSyncResponse
{
    public DataSyncState SyncState { get; set; }
    public string Message { get; set; } = "";

}
public enum DataSyncState
{
    ElectricySync = 1,
    ProductionSync = 2
}