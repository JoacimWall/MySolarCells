namespace MySolarCells.Services.Inverter;

public interface IInverterServiceInterface
{
    Task<Result<InverterLoginResponse>> LoginInUser(string userName, string password);
    Task<Result<List<PickerItem>>> GetSites();
    Task<Result<PickerItem>> GetInverter(string siteNodeId);
    Task<bool> SyncProductionOwnUse(DateTime start, IProgress<int> progress, int progressStartNr);
}
public class InverterLoginResponse
{

    public string token { get; set; }
    public string tokenType { get; set; }
    public int expiresIn { get; set; }


}
