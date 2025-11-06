namespace MySolarCells.Services;
public interface IDataSyncService
{
    Task<Result<DataSyncResponse>> Sync(bool syncGaps = false);
}

public class DataSyncService : IDataSyncService
{
    private readonly MscDbContext mscDbContext;
    public DataSyncService(MscDbContext mscDbContext)
    {
        this.mscDbContext = mscDbContext;
    }

    public async Task<Result<DataSyncResponse>> Sync(bool syncGaps = false)
    {
        var gridSupplierInterface = ServiceHelper.GetGridSupplierService(mscDbContext.ElectricitySupplier.First().ElectricitySupplierType);



        mscDbContext.Log.Add(new Log
        {
            LogTitle = "Sync started",
            CreateDate = DateTime.Now,
            LogDetails = "",
            LogTyp = (int)LogTypeEnum.Info
        });
        //Get last Sync Time for grid supplier
        Energy? lastSyncTime = null;
        if (syncGaps)
        {   //so that we continue ro chekc if evry row exist in db
            MySolarCellsGlobals.ImportErrorValidateEvrryRow = true;
            lastSyncTime = await mscDbContext.Energy.OrderBy(o => o.Timestamp).FirstOrDefaultAsync();
            if (lastSyncTime == null)
                return new Result<DataSyncResponse>("No data in Energy table");
            bool steppNext = true;
            DateTime existTime = lastSyncTime.Timestamp;
            while (steppNext)
            {
                var existLastSyncTime = await mscDbContext.Energy.FirstOrDefaultAsync(x => x.Timestamp == lastSyncTime.Timestamp.AddHours(1));
                if (existLastSyncTime == null)
                    steppNext = false;
                else
                    lastSyncTime = existLastSyncTime;
            }
        }
        else
        {
            lastSyncTime = await mscDbContext.Energy.Where(x => x.PurchasedSynced == true).OrderByDescending(o => o.Timestamp).FirstOrDefaultAsync();
        }

        if (lastSyncTime == null)
            return new Result<DataSyncResponse>("No data in Energy table");

        var previousHourDatetime = DateTime.Now.AddHours(-1);
        var prevHoleHour = new DateTime(previousHourDatetime.Year, previousHourDatetime.Month, previousHourDatetime.Day, previousHourDatetime.Hour, 0, 0);
        if (lastSyncTime.Timestamp < prevHoleHour)
        {
            var difference = DateTime.Now - lastSyncTime.Timestamp;
            var days = difference.Days;
            var hours = difference.Hours;
            var totalHours = (days * 24) + hours;

            var progress = new Progress<int>(currentDay =>
            {
                CalculateProgress(currentDay, totalHours);
            });


            await Task.Delay(200);
            var result = await gridSupplierInterface.Sync(lastSyncTime.Timestamp, progress, 0);
            if (!result.WasSuccessful)
                return result;

        }

        await Task.Delay(200);

        //It needs to be updated after grid sync to be correct.
        var lastSyncInverterTime = mscDbContext.Energy.Where(x => x.ProductionOwnUseSynced == true && x.PurchasedSynced == true).OrderByDescending(o => o.Timestamp).First();
        if (lastSyncInverterTime.Timestamp < prevHoleHour)
        {
            var differenceInverter = DateTime.Now - lastSyncInverterTime.Timestamp;

            var daysInv = differenceInverter.Days;
            var hoursInv = differenceInverter.Hours;
            var totalHoursInverter = (daysInv * 24) + hoursInv;
            var progress = new Progress<int>(currentDay =>
            {
                CalculateProgress(currentDay, totalHoursInverter);
            });
            //INVERTER
            var inverterService = ServiceHelper.GetInverterService(mscDbContext.Inverter.OrderByDescending(s => s.FromDate).First().InverterTyp);

            var resultInverter = await inverterService.Sync(lastSyncInverterTime.Timestamp, progress, 0);
            if (!resultInverter.WasSuccessful)
                return resultInverter;
        }

        //mscDbContext.Log.Add(new Log
        //{
        //    LogTitle = "Sync done",
        //    CreateDate = DateTime.Now,
        //    LogDetails = "",
        //    LogTyp = result.WasSuccessful ? (int)LogTypeEnum.Info : (int)LogTypeEnum.Error
        //});

        return new Result<DataSyncResponse>(new DataSyncResponse
        {
            SyncState = DataSyncState.ProductionSync,
            Message = AppResources.Import_Of_Production_Done
        });

    }
    private void CalculateProgress(long completed, long total)
    {
        // var comp = Convert.ToDouble(completed);
        // var tot = Convert.ToDouble(total);
        // var percentage = comp / tot;
    }


}





