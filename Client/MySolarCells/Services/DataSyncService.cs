
using MySolarCellsSQLite.Sqlite;
using MySolarCellsSQLite.Sqlite.Models;

namespace MySolarCells.Services;
public interface IDataSyncService
{
    Task<Result<DataSyncResponse>> Sync();


}

public class DataSyncService : IDataSyncService
{
    private readonly MscDbContext mscDbContext;
    public DataSyncService(MscDbContext mscDbContext)
    {
        this.mscDbContext= mscDbContext;
    }

    public async Task<Result<DataSyncResponse>> Sync()
    {
        var gridSupplierInterface = ServiceHelper.GetGridSupplierService(mscDbContext.Home.First().ElectricitySupplier);
        var inverterService = ServiceHelper.GetInverterService(mscDbContext.Inverter.OrderByDescending(s => s.FromDate).First().InverterTyp);
      
       

        mscDbContext.Log.Add(new Log
        {
            LogTitle = "Sync started",
            CreateDate = DateTime.Now,
            LogDetails = "",
            LogTyp = (int)LogTyp.Info
        });
        //Get last Sync Time for grid supplier
        var lastSyncTime = await mscDbContext.Energy.Where(x => x.PurchasedSynced == true).OrderByDescending(o => o.Timestamp).FirstOrDefaultAsync();
        var lastSyncInverterTime = await mscDbContext.Energy.Where(x => x.ProductionOwnUseSynced == true && x.PurchasedSynced == true).OrderByDescending(o => o.Timestamp).FirstOrDefaultAsync();

        if (lastSyncTime == null || lastSyncInverterTime == null)
            return new Result<DataSyncResponse>("No data in Energy table"); 
            
        var previousHourDatetime = DateTime.Now.AddHours(-1);   
        var prevHoleHour = new DateTime(previousHourDatetime.Year, previousHourDatetime.Month, previousHourDatetime.Day, previousHourDatetime.Hour,0,0);
        if (lastSyncTime.Timestamp >= prevHoleHour && lastSyncInverterTime.Timestamp >= prevHoleHour)
        {
            mscDbContext.Log.Add(new Log
            {
                LogTitle = AppResources.Synchronization_Not_Started_As_We_Have_All_Data_And_More,
                CreateDate = DateTime.Now,
                LogDetails = "",
                LogTyp = (int)LogTyp.Info
            });
            return new Result<DataSyncResponse>(new DataSyncResponse { Message = AppResources.Synchronization_Not_Started_As_We_Have_All_Data_And_More });
        }

        var difference = DateTime.Now - lastSyncTime.Timestamp;

        var days = difference.Days;
        var hours = difference.Hours;
        var totalHours = (days * 24) + hours;

        var progress = new Progress<int>(currentDay =>
        {
            CalculateProgress(currentDay, totalHours);
        });

        //await Task.Delay(200);
        //keepUploading = true;

        //ShowProgressStatus = true;
        //ProgressStatus = "Import consumption and sold production.";
        //ProgressSubStatus = "saved rows 0";
        await Task.Delay(200);
        var result = await gridSupplierInterface.Sync(lastSyncTime.Timestamp, progress, 0);
        if (!result.WasSuccessful)
            return result;


        //ShowProgressStatus = true;
        //ProgressStatus = "Import solar own use and calculate profit.";
        //ProgressSubStatus = "saved rows 0";
        await Task.Delay(200);

        //It needs to be updated after grid sync to be correct.
         lastSyncInverterTime = mscDbContext.Energy.Where(x => x.ProductionOwnUseSynced == true && x.PurchasedSynced == true).OrderByDescending(o => o.Timestamp).First();

        var differenceInverter = DateTime.Now - lastSyncInverterTime.Timestamp;

        var daysInv = differenceInverter.Days;
        var hoursInv = differenceInverter.Hours;
        var totalHoursInverter = (daysInv * 24) + hoursInv;
        progress = new Progress<int>(currentDay =>
        {
            CalculateProgress(currentDay, totalHoursInverter);
        });
        var resultInverter = await inverterService.Sync(lastSyncInverterTime.Timestamp, progress, 0);
       

        mscDbContext.Log.Add(new Log
        {
            LogTitle = "Sync done",
            CreateDate = DateTime.Now,
            LogDetails = "",
            LogTyp = result.WasSuccessful ? (int)LogTyp.Info : (int)LogTyp.Error
        });
       
        return resultInverter;

    }
    private void CalculateProgress(long completed, long total)
    {
        var comp = Convert.ToDouble(completed);
        var tot = Convert.ToDouble(total);
        var percentage = comp / tot;
    }

   
}

    



