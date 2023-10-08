
namespace MySolarCells.Services;
public interface IDataSyncService
{
    Task<Result<BoolModel>> Sync();


}

public class DataSyncService : IDataSyncService
{
    private readonly ITibberService tibberService;
    private readonly IInverterServiceInterface inverterService;
    public DataSyncService(ITibberService tibberService)
    {
        this.tibberService = tibberService;
        using var dbContext = new MscDbContext();
        this.inverterService = ServiceHelper.GetInverterService(dbContext.Inverter.First().InverterTyp);
    }

    public async Task<Result<BoolModel>> Sync()
    {
       
        using var dbContext = new MscDbContext();
        //Get last Sync Time
        var lastSyncTime = dbContext.Energy.Where(x => x.PurchasedSynced == true).OrderByDescending(o => o.Timestamp).First();
        var difference = DateTime.Now - lastSyncTime.Timestamp;

        var days = difference.Days;
        var hours = difference.Hours;
        var totalhours = (days * 24) + hours;

        var progress = new Progress<int>(currentDay =>
        {
            CalculateProgress(currentDay, totalhours);
        });

        await Task.Delay(200);
        //keepUploading = true;

        //ShowProgressStatus = true;
        //ProgressStatus = "Import consumation and sold production.";
        //ProgressSubStatus = "saved rows 0";
        await Task.Delay(200);
        var result = await this.tibberService.SyncConsumptionAndProductionFirstTime(lastSyncTime.Timestamp, progress, 0);
        if (!result)
        {
            return new Result<BoolModel>("Error import consumation and sold production.");
        }


        //ShowProgressStatus = true;
        //ProgressStatus = "Import solar own use and calculate profit.";
        //ProgressSubStatus = "saved rows 0";
        await Task.Delay(200);

        // var inverter = await dbContext.Inverter.FirstOrDefaultAsync(x => x.HomeId == MySolarCellsGlobals.SelectedHome.HomeId);
        lastSyncTime = dbContext.Energy.Where(x => x.ProductionOwnUseSynced == true).OrderByDescending(o => o.Timestamp).First();
        var differenceInverter = DateTime.Now - lastSyncTime.Timestamp;

        var daysInv = differenceInverter.Days;
        var hoursInv = differenceInverter.Hours;
        var totalhoursInv = (daysInv * 24) + hoursInv;
        progress = new Progress<int>(currentDay =>
        {
            CalculateProgress(currentDay, totalhoursInv);
        });
        var resultInverter = await this.inverterService.SyncProductionOwnUse(lastSyncTime.Timestamp, progress, 0);
        if (!resultInverter)
        {
            return new Result<BoolModel>("Error import solar own use and calculate profit");
         
        }

        return new Result<BoolModel>(new BoolModel { });


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

    



