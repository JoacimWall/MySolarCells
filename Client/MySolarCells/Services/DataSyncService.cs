
namespace MySolarCells.Services;
public interface IDataSyncService
{
    Task<Result<DataSyncResponse>> Sync();


}

public class DataSyncService : IDataSyncService
{
    //private readonly IGridSupplierInterface gridSupplierInterface;
    //private readonly IInverterServiceInterface inverterService;
    private readonly ISettingsService settingsService;
    private readonly MscDbContext mscDbContext;
    public DataSyncService(ISettingsService settingsService,MscDbContext mscDbContext)
    {
        this.settingsService = settingsService;
        this.mscDbContext= mscDbContext;
    }

    public async Task<Result<DataSyncResponse>> Sync()
    {
        var gridSupplierInterface = ServiceHelper.GetGridSupplierService(this.mscDbContext.Home.FirstOrDefault().ElectricitySupplier);
        var inverterService = ServiceHelper.GetInverterService(this.mscDbContext.Inverter.FirstOrDefault().InverterTyp);

        var currentHour = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Hour);
        if (this.settingsService.LastDataSync > currentHour)
        {
            mscDbContext.Log.Add(new Services.Sqlite.Models.Log
            {
                LogTitle = "Sync not started allready has all data for prev hour",
                CreateDate = DateTime.Now,
                LogDetails = "",
                LogTyp = (int)LogTyp.Info
            });
            return new Result<DataSyncResponse>(new DataSyncResponse { Message = "Sync not started allready has all data for prev hour" }, true);
        }

        mscDbContext.Log.Add(new Services.Sqlite.Models.Log
        {
            LogTitle = "Sync started",
            CreateDate = DateTime.Now,
            LogDetails = "",
            LogTyp = (int)LogTyp.Info
        });
        //Get last Sync Time
        var lastSyncTime = this.mscDbContext.Energy.Where(x => x.PurchasedSynced == true).OrderByDescending(o => o.Timestamp).First();
        var difference = DateTime.Now - lastSyncTime.Timestamp;

        var days = difference.Days;
        var hours = difference.Hours;
        var totalhours = (days * 24) + hours;

        var progress = new Progress<int>(currentDay =>
        {
            CalculateProgress(currentDay, totalhours);
        });

        //await Task.Delay(200);
        //keepUploading = true;

        //ShowProgressStatus = true;
        //ProgressStatus = "Import consumation and sold production.";
        //ProgressSubStatus = "saved rows 0";
        await Task.Delay(200);
        var result = await gridSupplierInterface.Sync(lastSyncTime.Timestamp, progress, 0);
        if (!result.WasSuccessful)
            return result;


        //ShowProgressStatus = true;
        //ProgressStatus = "Import solar own use and calculate profit.";
        //ProgressSubStatus = "saved rows 0";
        await Task.Delay(200);

        // var inverter = await dbContext.Inverter.FirstOrDefaultAsync(x => x.HomeId == MySolarCellsGlobals.SelectedHome.HomeId);
        lastSyncTime = this.mscDbContext.Energy.Where(x => x.ProductionOwnUseSynced == true).OrderByDescending(o => o.Timestamp).First();
        var differenceInverter = DateTime.Now - lastSyncTime.Timestamp;

        var daysInv = differenceInverter.Days;
        var hoursInv = differenceInverter.Hours;
        var totalhoursInv = (daysInv * 24) + hoursInv;
        progress = new Progress<int>(currentDay =>
        {
            CalculateProgress(currentDay, totalhoursInv);
        });
        var resultInverter = await inverterService.Sync(lastSyncTime.Timestamp, progress, 0);
       

        mscDbContext.Log.Add(new Services.Sqlite.Models.Log
        {
            LogTitle = "Sync done",
            CreateDate = DateTime.Now,
            LogDetails = "",
            LogTyp = result.WasSuccessful ? (int)LogTyp.Info : (int)LogTyp.Error
        });
        if (resultInverter.WasSuccessful)
            this.settingsService.LastDataSync = DateTime.Now;
        return resultInverter;

    }
    private void CalculateProgress(long completed, long total)
    {
        var comp = Convert.ToDouble(completed);
        var tot = Convert.ToDouble(total);
        var percentage = comp / tot;
        //UploadProgress.ProgressTo(percentage, 100, Easing.Linear);
        //ProgressProcent = (float)percentage * 100;
        //ProgressSubStatus = "saved rows " + completed.ToString();
    }

   
}

    



