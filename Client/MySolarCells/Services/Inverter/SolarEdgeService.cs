using System.Web;

namespace MySolarCells.Services.Inverter;

public class SolarEdgeService : IInverterServiceInterface
{
    private readonly IRestClient restClient;
    private readonly MscDbContext mscDbContext;
    private InverterLoginResponse inverterLoginResponse;


    public string InverterGuideText => AppResources.SolarEdge_Intro_Guide_Text;
    public string DefaultApiUrl => "";
    public bool ShowUserName => false;

    public bool ShowPassword => false;

    public bool ShowApiUrl => false;

    public bool ShowApiKey => true;

    public SolarEdgeService(IRestClient restClient, MscDbContext mscDbContext)
    {
        this.restClient = restClient;
        this.mscDbContext = mscDbContext;
        Dictionary<string, string> defaultRequestHeaders = new Dictionary<string, string>();
        this.restClient.ApiSettings = new ApiSettings { BaseUrl = "https://monitoringapi.solaredge.com", defaultRequestHeaders = defaultRequestHeaders };
        this.restClient.ReInit();
    }
    public async Task<Result<InverterLoginResponse>> TestConnection(string userName, string password, string apiUrl, string apiKey)
    {
        this.inverterLoginResponse = new InverterLoginResponse
        {
            token = apiKey,
            expiresIn = 0,
            tokenType = "",
        };

        return new Result<InverterLoginResponse>(this.inverterLoginResponse);
    }
    public async Task<Result<List<InverterSite>>> GetPickerOne()
    {
        string query = string.Format("/sites/list?api_key={0}", this.inverterLoginResponse.token);
        var resultSites = await this.restClient.ExecuteGetAsync<SolarEdgeSiteListResponse>(query);
        if (!resultSites.WasSuccessful)
        {
            return new Result<List<InverterSite>>(resultSites.ErrorMessage);
        }
        var sitesResponse = resultSites.Model;
        var returnlist = new List<InverterSite>();
        foreach (var item in sitesResponse.sites.site)
        {
            returnlist.Add(new InverterSite { Id = item.id.ToString(), Name = item.name.ToString(), InverterName = item.primaryModule.modelName, InstallationDate = Convert.ToDateTime(item.installationDate) });
        }
        return new Result<List<InverterSite>>(returnlist);
    }
    public async Task<Result<GetInverterResponse>> GetInverter(InverterSite inverterSite)
    {

        return new Result<GetInverterResponse>(new GetInverterResponse { InverterId = inverterSite.Id, Name = inverterSite.InverterName });
    }

    
    public async Task<Result<DataSyncResponse>> Sync(DateTime start, IProgress<int> progress, int progressStartNr)
    {
         var inverter = await this.mscDbContext.Inverter.OrderByDescending(s => s.FromDate).FirstOrDefaultAsync(x => x.HomeId == MySolarCellsGlobals.SelectedHome.HomeId);
        ////LOGIN
        var loginResult = await TestConnection("", "", "", StringHelper.Decrypt(inverter.ApiKey, AppConstants.Secretkey));


        try
        {
            int batch100 = 0;
            string processingDateFrom;
            string processingDateTo;
            int homeId = MySolarCellsGlobals.SelectedHome.HomeId;
            List<SQLite.Sqlite.Models.Energy> eneryList = new List<SQLite.Sqlite.Models.Energy>();
            DateTime end = DateTime.Now;
            DateTime nextStart = new DateTime();
            int maxWeek = 0;

            while (start < end)
            {
                //Get 1 mounts per request max
                if (start.AddMonths(1) < end)
                {
                    processingDateFrom = new DateTime(start.Year, start.Month, start.Day).ToString("yyyy-MM-dd") + " 00:00:00";
                    processingDateTo = new DateTime(start.AddMonths(1).Year, start.AddMonths(1).Month, start.AddMonths(1).Day).ToString("yyyy-MM-dd") + " 00:00:00";
                    nextStart = start.AddMonths(1);
                }
                else
                {
                    processingDateFrom = new DateTime(start.Year, start.Month, start.Day).ToString("yyyy-MM-dd") + " 00:00:00";
                    processingDateTo = new DateTime(end.Year, end.Month, end.Day).ToString("yyyy-MM-dd") + string.Format(" {0}:00:00", end.ToString("HH"));
                    nextStart = end;
                }

                List<SolarEdgeSumHour> sumes = new List<SolarEdgeSumHour>();
                //Add Production and feed in and selfconsumption
                var query = string.Format("/site/{0}/energyDetails?timeUnit=HOUR&api_key={1}&startTime={2}&endTime={3}", inverter.SubSystemEntityId, loginResult.Model.token, processingDateFrom, processingDateTo);
                var resultEnergy = await this.restClient.ExecuteGetAsync<SolarEdgeEnegyDetialsResponse>(query);

                foreach (var item in resultEnergy.Model.energyDetails.meters)
                {
                    foreach (var value in item.values)
                    {
                        DateTime timestamp = Convert.ToDateTime(value.date);
                        DateTime searchTime = new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, timestamp.Hour, 0, 0);
                        //if (timestamp.Minute == 0)
                        //{   //So the last 15 minutes land on prev hour. 13:00 belong to 12:45-13:00 production then timestamp 12:00    
                        //    searchTime = searchTime.AddHours(-1);
                        //}
                        var exist = sumes.FirstOrDefault(x => x.TimeStamp == searchTime);
                        if (exist == null)
                        {
                            exist = new SolarEdgeSumHour { TimeStamp = searchTime };
                            sumes.Add(exist);
                        }
                        switch (item.type.ToLower())
                        {
                            case "selfconsumption":
                            //case "production":
                                exist.SelfConsumption = exist.SelfConsumption + (value.value.HasValue ? value.value.Value : 0);
                                break;
                            case "feedin":
                                exist.FeedIn = exist.FeedIn + (value.value.HasValue ? value.value.Value : 0);
                                break;
                            case "purchased":
                                exist.Purchased = exist.Purchased + (value.value.HasValue ? value.value.Value : 0);
                                break;
                            //case "production":
                            //    exist.batteryCharge = exist.batteryCharge + (value.value.HasValue ? value.value.Value : 0);
                            //    break;
                            //case "consumption":
                            //    exist.batteryOutput = exist.batteryOutput + (value.value.HasValue ? value.value.Value : 0);
                            //break;

                            default:
                                break;
                        }
                        //exist.SelfConsumption = exist.SelfConsumption - exist.FeedIn;
                    }
                }
                //Add Battery max 7 days
                var nextStartDays = new DateTime();
                var startDays = start;
                while (startDays < nextStart)
                {
                    maxWeek++;
                    //Get 7 mounts per request max
                    if (startDays.AddDays(7) < nextStart)
                    {
                        processingDateFrom = new DateTime(startDays.Year, startDays.Month, startDays.Day).ToString("yyyy-MM-dd") + " 00:00:00";
                        processingDateTo = new DateTime(startDays.AddDays(7).Year, startDays.AddDays(7).Month, startDays.AddDays(7).Day).ToString("yyyy-MM-dd") + " 00:00:00";
                        nextStartDays = startDays.AddDays(7);
                    }
                    else
                    {
                        processingDateFrom = new DateTime(startDays.Year, startDays.Month, startDays.Day).ToString("yyyy-MM-dd") + " 00:00:00";
                        processingDateTo = new DateTime(startDays.Year, startDays.Month, startDays.Day).ToString("yyyy-MM-dd") + string.Format(" {0}:00:00", end.ToString("HH"));
                        nextStartDays = nextStart;
                    }

                    query = string.Format("/site/{0}/storageData?api_key={1}&startTime={2}&endTime={3}", inverter.SubSystemEntityId, loginResult.Model.token, processingDateFrom, processingDateTo);
                    var resultBattery = await this.restClient.ExecuteGetAsync<SolarEdgeStorageDataResponse>(query);
                    if (resultBattery.WasSuccessful && resultBattery.Model.storageData != null)
                    {
                        foreach (var item in resultBattery.Model.storageData.batteries[0].telemetries)
                        {
                            DateTime timestamp = Convert.ToDateTime(item.timeStamp);
                            DateTime searchTime = new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, timestamp.Hour, 0, 0);
                            var exist = sumes.FirstOrDefault(x => x.TimeStamp == searchTime);
                            if (exist == null)
                            {
                                exist = new SolarEdgeSumHour { TimeStamp = searchTime };
                                sumes.Add(exist);
                            }
                            //Charging
                            if (item.batteryState == 3)
                            {
                                exist.batteryCharge = exist.batteryCharge + (item.power.HasValue ? item.power.Value : 0);
                            }//Using 
                            else if (item.batteryState == 4)
                            {
                                exist.batteryOutput = exist.batteryOutput + (item.power.HasValue ? Math.Abs(item.power.Value) : 0);
                            }


                        }
                    }
                    startDays = nextStartDays;
                }
                var producedI = sumes.Count;

                for (int i = 0; i < producedI; i++)
                {
                    progressStartNr++;
                    batch100++;
                    var energyExist = this.mscDbContext.Energy.FirstOrDefault(x => x.Timestamp == sumes[i].TimeStamp);
                    if (energyExist != null)
                    {

                        energyExist.InverterTypProductionOwnUse = (int)InverterTyp.SolarEdge;
                        energyExist.ElectricitySupplierPurchased = (int)InverterTyp.SolarEdge;
                        energyExist.ElectricitySupplierProductionSold = (int)InverterTyp.SolarEdge;
                        if (energyExist.Timestamp < end.AddHours(-2))
                        {
                            energyExist.ProductionSoldSynced = true;
                            energyExist.ProductionOwnUseSynced = true;
                            energyExist.PurchasedSynced = true;
                            energyExist.BatterySynced = true;
                        }
                        energyExist.Purchased = sumes[i].Purchased > 0 ? Convert.ToDouble(sumes[i].Purchased / 1000)  : 0;
                        energyExist.ProductionOwnUse = sumes[i].SelfConsumption > 0 ? Convert.ToDouble(sumes[i].SelfConsumption / 1000) : 0;
                        energyExist.ProductionSold = sumes[i].FeedIn > 0 ? Convert.ToDouble(sumes[i].FeedIn / 1000) : 0;
                        energyExist.BatteryCharge = sumes[i].batteryCharge > 0 ? Convert.ToDouble((sumes[i].batteryCharge/12)/1000) : 0;
                        energyExist.BatteryUsed = sumes[i].batteryOutput > 0 ? Convert.ToDouble((sumes[i].batteryOutput/12) / 1000) : 0;
                        //Fixar till så att det blir rätt då ProductionOwnUse är både använt och laddat batteriet
                        energyExist.ProductionOwnUse = energyExist.ProductionOwnUse - energyExist.BatteryUsed;
                        if (energyExist.ProductionOwnUse < 0)
                            energyExist.ProductionOwnUse = 0;
                        //Calc spot costs
                        energyExist.PurchasedCost = energyExist.Purchased * energyExist.UnitPriceBuy;
                        energyExist.ProductionSoldProfit = energyExist.ProductionSold * energyExist.UnitPriceSold;
                        energyExist.ProductionOwnUseProfit = energyExist.ProductionOwnUse * energyExist.UnitPriceBuy;
                        //energyExist.BatteryChargeProfitFake = energyExist.BatteryCharge * energyExist.UnitPriceSold;
                        energyExist.BatteryUsedProfit = energyExist.BatteryUsed * energyExist.UnitPriceBuy;
                        
                        eneryList.Add(energyExist);
                    }

                    if (batch100 == 100)
                    {
                        await Task.Delay(100); //Så att GUI hinner uppdatera
                        progress.Report(progressStartNr);

                        batch100 = 0;
                        await this.mscDbContext.BulkUpdateAsync(eneryList);
                        eneryList = new List<SQLite.Sqlite.Models.Energy>();
                    }

                }

                start = nextStart;
            }

            if (eneryList.Count > 0)
            {
                await Task.Delay(100); //Så att GUI hinner uppdatera
                progress.Report(progressStartNr);

                batch100 = 0;
                await this.mscDbContext.BulkUpdateAsync(eneryList);
                eneryList = new List<SQLite.Sqlite.Models.Energy>();
            }
            
        }
        catch (Exception ex)
        {
            return new Result<DataSyncResponse>(ex.Message);
        }


        return new Result<DataSyncResponse>(new DataSyncResponse
        {
            SyncState = DataSyncState.ProductionSync,
            Message = AppResources.Import_Of_Production_Done
        }, true);



    }
}

public class SolarEdgeEnergyDetails
{
    public string timeUnit { get; set; }
    public string unit { get; set; }
    public List<SolarEdgeEnergyMeter> meters { get; set; }
}

public class SolarEdgeEnergyMeter
{
    public string type { get; set; }
    public List<SolarEdgeEnergyValue> values { get; set; }
}

public class SolarEdgeEnegyDetialsResponse
{
    public SolarEdgeEnergyDetails energyDetails { get; set; }
}

public class SolarEdgeEnergyValue
{
    public string date { get; set; }
    public double? value { get; set; }
}

public class SolarEdgeBattery
{
    public double nameplate { get; set; }
    public string serialNumber { get; set; }
    public string modelNumber { get; set; }
    public int telemetryCount { get; set; }
    public List<SolarEdgeTelemetry> telemetries { get; set; }
}

public class SolarEdgeStorageDataResponse
{
    public SolarEdgeStorageData storageData { get; set; }
}

public class SolarEdgeStorageData
{
    public int batteryCount { get; set; }
    public List<SolarEdgeBattery> batteries { get; set; }
}

public class SolarEdgeTelemetry
{
    public string timeStamp { get; set; }
    public double? power { get; set; }
    public int batteryState { get; set; }
    public int lifeTimeEnergyDischarged { get; set; }
    public int lifeTimeEnergyCharged { get; set; }
    public double batteryPercentageState { get; set; }
    public double fullPackEnergyAvailable { get; set; }
    public double internalTemp { get; set; }
    public double? ACGridCharging { get; set; }
}

public class SolarEdgeSumHour
{
    public DateTime TimeStamp { get; set; }
    public double SelfConsumption { get; set; }
    public double FeedIn { get; set; }
    public double Purchased { get; set; }
    public double batteryCharge { get; set; }
    public double batteryOutput { get; set; }

}

//Response
public class SolarEdgeSiteListResponse
{
    public SolarEdgeSites sites { get; set; }

}

public class SolarEdgeSite
{
    public int id { get; set; }
    public string name { get; set; }
    public int accountId { get; set; }
    public string status { get; set; }
    public double peakPower { get; set; }
    public string lastUpdateTime { get; set; }
    public string installationDate { get; set; }
    public object ptoDate { get; set; }
    public string notes { get; set; }
    public string type { get; set; }
    public SolarEdgePrimaryModule primaryModule { get; set; }

}

public class SolarEdgeSites
{
    public int count { get; set; }
    public List<SolarEdgeSite> site { get; set; }
}

public class SolarEdgePrimaryModule
{
    public string manufacturerName { get; set; }
    public string modelName { get; set; }
    public double maximumPower { get; set; }
    public double temperatureCoef { get; set; }
}

public class SolarEdgeMeter
{
    public string type { get; set; }
    public List<SolarEdgeValue> values { get; set; }
}

public class SolarEdgePowerDetails
{
    public string timeUnit { get; set; }
    public string unit { get; set; }
    public List<SolarEdgeMeter> meters { get; set; }
}

public class SolarEdgePowerDetailsResults
{
    public SolarEdgePowerDetails powerDetails { get; set; }
}

public class SolarEdgeValue
{
    public string date { get; set; }
    public double value { get; set; }
}




