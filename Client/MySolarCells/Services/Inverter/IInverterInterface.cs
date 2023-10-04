namespace MySolarCells.Services.Inverter;

public interface IInverterServiceInterface
{
    Task<Result<InverterLoginResponse>> TestConnection(string userName, string password,string apiUrl,string apiKey);
    Task<Result<List<PickerItem>>> GetPickerOne();
    Task<Result<GetInverterResponse>> GetInverter(PickerItem pickerItem);
    Task<bool> SyncProductionOwnUse(DateTime start, IProgress<int> progress, int progressStartNr);
    
    string InverterGuideText { get; }
    string DefaultApiUrl { get; }
    bool ShowUserName { get;  }
    bool ShowPassword { get;  }
    bool ShowApiUrl { get;  }
    
    bool ShowApiKey { get; }
    
}
public class InverterLoginResponse
{

    public string token { get; set; }
    public string tokenType { get; set; }
    public int expiresIn { get; set; }
}
public class GetInverterResponse
{

    public string InverterId { get; set; }
    public string Name { get; set; }

}