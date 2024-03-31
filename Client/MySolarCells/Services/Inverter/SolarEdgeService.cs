using MySolarCells.Services.Inverter.Models;

namespace MySolarCells.Services.Inverter;

public class SolarEdgeService : IInverterServiceInterface
{
    private readonly IRestClient restClient;
    private readonly MscDbContext mscDbContext;
    private readonly IHomeService homeService;
    private InverterLoginResponse? inverterLoginResponse;


    public string InverterGuideText => AppResources.SolarEdge_Intro_Guide_Text;
    public string DefaultApiUrl => "";
    public bool ShowUserName => false;

    public bool ShowPassword => false;

    public bool ShowApiUrl => false;

    public bool ShowApiKey => true;

    public SolarEdgeService(IRestClient restClient, MscDbContext mscDbContext, IHomeService homeService)
    {
        this.restClient = restClient;
        this.mscDbContext = mscDbContext;
        this.homeService = homeService;
        Dictionary<string, string> defaultRequestHeaders = new Dictionary<string, string>();
        this.restClient.ApiSettings = new ApiSettings { BaseUrl = "https://monitoringapi.solaredge.com", DefaultRequestHeaders = defaultRequestHeaders };
        this.restClient.ReInit();
    }
    public Task<Result<InverterLoginResponse>> TestConnection(string userName, string password, string apiUrl, string apiKey)
    {
        inverterLoginResponse = new InverterLoginResponse
        {
            token = apiKey,
            expiresIn = 0,
            tokenType = "",
        };

        return Task.FromResult(new Result<InverterLoginResponse>(inverterLoginResponse));
    }
    public async Task<Result<List<InverterSite>>> GetPickerOne()
    {
        if (inverterLoginResponse == null)
            return new Result<List<InverterSite>>("Inverter login response missing");
        
        string query = $"/sites/list?api_key={inverterLoginResponse.token}";
        var resultSites = await restClient.ExecuteGetAsync<SolarEdgeSiteListResponse>(query);
        if (!resultSites.WasSuccessful || resultSites.Model == null)
        {
            return new Result<List<InverterSite>>(resultSites.ErrorMessage);
        }
        var sitesResponse = resultSites.Model;
        var returnlist = new List<InverterSite>();
        foreach (var item in sitesResponse.sites.site)
        {
            returnlist.Add(new InverterSite { Id = item.id.ToString(), Name = item.name, InverterName = item.primaryModule.modelName, InstallationDate = Convert.ToDateTime(item.installationDate) });
        }
        return new Result<List<InverterSite>>(returnlist);
    }
    public Task<Result<GetInverterResponse>> GetInverter(InverterSite inverterSite)
    {

        return Task.FromResult(new Result<GetInverterResponse>(new GetInverterResponse { InverterId = inverterSite.Id, Name = inverterSite.InverterName }));
    }

    
    public async Task<Result<DataSyncResponse>> Sync(DateTime start, IProgress<int> progress, int progressStartNr)
    {
         var inverter = await mscDbContext.Inverter.OrderByDescending(s => s.FromDate).FirstAsync(x => x.HomeId == homeService.CurrentHome().HomeId);

         string apiKey="";
        if (inverter.ApiKey != null)
        {
             apiKey = inverter.ApiKey.Decrypt(AppConstants.Secretkey);
        }

        var loginResult = await TestConnection("", "", "", apiKey);

        if (!loginResult.WasSuccessful || loginResult.Model == null)
            return new Result<DataSyncResponse>("login result null");
        
        try
        {
            int batch100 = 0;
            var energyList = new List<Energy>();
            var end = DateTime.Now;

            while (start < end)
            {
                //Get 1 mounts per request max
                DateTime nextStart;
                string processingDateTo;
                string processingDateFrom;
                if (start.AddMonths(1) < end)
                {
                    processingDateFrom = new DateTime(start.Year, start.Month, start.Day).ToString("yyyy-MM-dd") + " 00:00:00";
                    processingDateTo = new DateTime(start.AddMonths(1).Year, start.AddMonths(1).Month, start.AddMonths(1).Day).ToString("yyyy-MM-dd") + " 00:00:00";
                    nextStart = start.AddMonths(1);
                }
                else
                {
                    processingDateFrom = new DateTime(start.Year, start.Month, start.Day).ToString("yyyy-MM-dd") + " 00:00:00";
                    processingDateTo = new DateTime(end.Year, end.Month, end.Day).ToString("yyyy-MM-dd") +
                                       $" {end:HH}:00:00";
                    nextStart = end;
                }

                List<SolarEdgeSumHour> sumes = new List<SolarEdgeSumHour>();
                //Add Production and feed in and self consumption
                var query =
                    $"/site/{inverter.SubSystemEntityId}/energyDetails?timeUnit=HOUR&api_key={loginResult.Model.token}&startTime={processingDateFrom}&endTime={processingDateTo}";
                var resultEnergy = await restClient.ExecuteGetAsync<SolarEdgeEnegyDetialsResponse>(query);
                if (!resultEnergy.WasSuccessful || resultEnergy.Model == null)
                    return new Result<DataSyncResponse>("ResultEnergy is null");
                
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
                        }
                        //exist.SelfConsumption = exist.SelfConsumption - exist.FeedIn;
                    }
                }
                //Add Battery max 7 days
                var startDays = start;
                while (startDays < nextStart)
                {
                    //Get 7 mounts per request max
                    DateTime nextStartDays;
                    if (startDays.AddDays(7) < nextStart)
                    {
                        processingDateFrom = new DateTime(startDays.Year, startDays.Month, startDays.Day).ToString("yyyy-MM-dd") + " 00:00:00";
                        processingDateTo = new DateTime(startDays.AddDays(7).Year, startDays.AddDays(7).Month, startDays.AddDays(7).Day).ToString("yyyy-MM-dd") + " 00:00:00";
                        nextStartDays = startDays.AddDays(7);
                    }
                    else
                    {
                        processingDateFrom = new DateTime(startDays.Year, startDays.Month, startDays.Day).ToString("yyyy-MM-dd") + " 00:00:00";
                        processingDateTo = new DateTime(startDays.Year, startDays.Month, startDays.Day).ToString("yyyy-MM-dd") +
                                           $" {end:HH}:00:00";
                        nextStartDays = nextStart;
                    }

                    query =
                        $"/site/{inverter.SubSystemEntityId}/storageData?api_key={loginResult.Model.token}&startTime={processingDateFrom}&endTime={processingDateTo}";
                    var resultBattery = await restClient.ExecuteGetAsync<SolarEdgeStorageDataResponse>(query);
                    if (resultBattery is { Model.storageData: not null, WasSuccessful: true })
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
                                exist.batteryCharge += (item.power.HasValue ? item.power.Value : 0);
                            }//Using 
                            else if (item.batteryState == 4)
                            {
                                exist.batteryOutput += (item.power.HasValue ? Math.Abs(item.power.Value) : 0);
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
                    var energyExist = mscDbContext.Energy.FirstOrDefault(x => x.Timestamp == sumes[i].TimeStamp);
                    if (energyExist != null)
                    {

                        energyExist.InverterTypeProductionOwnUse = (int)InverterTypeEnum.SolarEdge;
                        energyExist.ElectricitySupplierPurchased = (int)InverterTypeEnum.SolarEdge;
                        energyExist.ElectricitySupplierProductionSold = (int)InverterTypeEnum.SolarEdge;
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
                        
                        energyList.Add(energyExist);
                    }

                    if (batch100 == 100)
                    {
                        await Task.Delay(100); //Så att GUI hinner uppdatera
                        progress.Report(progressStartNr);

                        batch100 = 0;
                        await mscDbContext.BulkUpdateAsync(energyList);
                        energyList = new List<Energy>();
                    }

                }

                start = nextStart;
            }

            if (energyList.Count > 0)
            {
                await Task.Delay(100); //So that the GUI has time to update
                progress.Report(progressStartNr);
                await mscDbContext.BulkUpdateAsync(energyList);
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
        });



    }
}





