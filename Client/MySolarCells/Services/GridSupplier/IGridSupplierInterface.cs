

using MySolarCellsSQLite.Sqlite.Models;

namespace MySolarCells.Services.GridSupplier
{
	public interface IGridSupplierInterface
	{
        Task<Result<GridSupplierLoginResponse>> TestConnection(string userName, string password, string apiUrl, string apiKey);
        Task<Result<List<Home>>> GetPickerOne();
        Task<Result<DataSyncResponse>> Sync(DateTime start, IProgress<int> progress, int progressStartNr);

        string GuideText { get; }
        string DefaultApiUrl { get; }
        string NavigationUrl { get; }
        bool ShowUserName { get; }
        bool ShowPassword { get; }
        bool ShowApiUrl { get; }
        bool ShowApiKey { get; }
        bool ShowNavigateUrl { get; }
        
    }
    public class GridSupplierLoginResponse
    {

        public string ApiKey { get; set; } = "";
        public string ResponseText { get; set; } = "";

    }
}

