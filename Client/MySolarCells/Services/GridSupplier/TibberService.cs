using MySolarCells.Services.GridSupplier.Models;
using NetTopologySuite.Index.HPRtree;

namespace MySolarCells.Services.GridSupplier;

public class TibberService : IGridSupplierInterface
{
    private readonly MscDbContext mscDbContext;
    private readonly IRestClient restClient;
    private readonly IHomeService homeService;
    private readonly ILogService logService;
    public string GuideText => AppResources.Tibber_Guide_Text;
    public string DefaultApiUrl => "";
    public string NavigationUrl => "https://developer.tibber.com/settings/access-token";
    public bool ShowUserName => false;

    public bool ShowPassword => false;

    public bool ShowApiUrl => false;

    public bool ShowApiKey => true;
    public bool ShowNavigateUrl => true;

    private GridSupplierLoginResponse? gridSupplierLoginResponse;

    public TibberService(IRestClient restClient, MscDbContext mscDbContext, IHomeService homeService, ILogService logService)
    {
        this.restClient = restClient;
        this.mscDbContext = mscDbContext;
        this.homeService = homeService;
        this.logService = logService;
        Dictionary<string, string> defaultRequestHeaders = new Dictionary<string, string>();

        this.restClient.ApiSettings = new ApiSettings
        { BaseUrl = "https://api.tibber.com/v1-beta/gql", DefaultRequestHeaders = defaultRequestHeaders };
        ReInitRest();
    }

    private void ReInitRest()
    {
        restClient.ReInit();

        Dictionary<string, string> tokens = new Dictionary<string, string>
        {
            {
                AppConstants.Authorization,
                $"Bearer {homeService.FirstElectricitySupplier().ApiKey.Decrypt(AppConstants.Secretkey)}"
            }
        };
        restClient.UpdateToken(tokens);
    }

    public async Task<Result<GridSupplierLoginResponse>> TestConnection(string userName, string password, string apiUrl,
        string apiKey)
    {
        Dictionary<string, string> tokens = new Dictionary<string, string>
        {
            { AppConstants.Authorization, $"Bearer {apiKey}" }
        };
        restClient.UpdateToken(tokens);
        GraphQlRequestTibber graphQlRequestTibber = new GraphQlRequestTibber
        {
            query = "{ viewer { accountType }}"
        };

        var result = await restClient.ExecutePostAsync<TibberResponse>(string.Empty, graphQlRequestTibber);
        if (!result.WasSuccessful || result.Model == null)
            return new Result<GridSupplierLoginResponse>(result.ErrorMessage.Length > 0
                ? result.ErrorMessage
                : AppResources.The_Login_Failed_Check_That_You_Entered_The_Correct_Information);
        gridSupplierLoginResponse = new GridSupplierLoginResponse
        { ApiKey = apiKey, ResponseText = result.Model.data.viewer.accountType.First() };
        return new Result<GridSupplierLoginResponse>(new GridSupplierLoginResponse
        { ApiKey = apiKey, ResponseText = result.Model.data.viewer.accountType.First() });
    }

    public async Task<Result<List<ElectricitySupplier>>> GetPickerOne()
    {
        GraphQlRequestTibber graphQlRequestTibber = new GraphQlRequestTibber
        {
            query = "{ viewer { homes { id address { address1 postalCode city country } }}}"
        };
        var result = await restClient.ExecutePostAsync<TibberResponse>(string.Empty, graphQlRequestTibber);
        if (!result.WasSuccessful || result.Model == null)
            return new Result<List<ElectricitySupplier>>(result.ErrorMessage);
        List<ElectricitySupplier> homes = new List<ElectricitySupplier>();
        foreach (var item in result.Model.data.viewer.homes)
        {
            homes.Add(new ElectricitySupplier
            {
                ApiKey = gridSupplierLoginResponse?.ApiKey ?? string.Empty,
                ElectricitySupplierType = (int)ElectricitySupplierEnum.Tibber,
                Name = item.address.address1,
                SubSystemEntityId = item.id
            });
        }

        return new Result<List<ElectricitySupplier>>(homes);
    }

    public async Task<Result<DataSyncResponse>> Sync(DateTime start, IProgress<int> progress, int progressStartNr)
    {
        try
        {
            ReInitRest();
            int batch100 = 0;
            int homeId = homeService.CurrentHome().HomeId;
            bool importOnlySpotPrice = homeService.FirstElectricitySupplier().ImportOnlySpotPrice;
            List<Energy> energyInsertList = new List<Energy>();
            List<Energy> energyUpdateList = new List<Energy>();

            var dates = DateHelper.GetRelatedDates(DateTime.Today);
            DateTime end = dates.BaseDate.AddDays(1);

            //We want to sync Spot Price prices hole day
            //other data only until last hour  
            while (start < end)
            {
                TimeSpan difference;
                DateTime startDateForImport;
                if (start.AddMonths(3) < end)
                {
                    startDateForImport = start;
                    difference = start.AddMonths(3) - start;
                    logService.ConsoleWriteLineDebug($"Import from {start.ToShortDateString()} to {start.AddDays(difference.Days).ToShortDateString()}");
                    start = start.AddMonths(3);
                }
                else
                {
                    startDateForImport = start;
                    difference = end - start;
                    logService.ConsoleWriteLineDebug($"Import from {start.ToShortDateString()} to {start.AddDays(difference.Days).ToShortDateString()}");
                    start = end;
                }

                var days = difference.Days;
                var hours = difference.Hours;
                var hoursTot = (days * 24) + hours;

                GraphQlRequestTibber graphQlRequestTibber = new GraphQlRequestTibber
                {
                    variables = new TibberConsumptionProductionRequest
                    {
                        homeid = homeService.FirstElectricitySupplier().SubSystemEntityId,
                        from = StringHelper.EncodeTo64($"{startDateForImport:s}"), //start.ToString() "yyyy-MM-dd
                        first = hoursTot
                    },
                    query = "query getdata($homeid: ID!, $from: String, $first: Int) { viewer { home(id: $homeid) { consumption(resolution: HOURLY, after: $from, first: $first) { nodes { from to cost unitPrice unitPriceVAT consumption consumptionUnit currency } } production(resolution: HOURLY, after: $from, first: $first) { nodes { from to profit unitPrice unitPriceVAT production productionUnit currency}}}}}"
                };
                var resultSites = await restClient.ExecutePostAsync<TibberResponse>(string.Empty, graphQlRequestTibber);
                if (resultSites.Model == null)
                    return new Result<DataSyncResponse>("No values");

                var consumptionI = resultSites.Model.data.viewer.home.consumption.nodes.Count;

                //used to check så the backend not give the same value twice
                var existCheckDb = true;
                var updateEntity = true;
                for (int i = 0; i < consumptionI; i++)
                {
                    progressStartNr++;
                    batch100++;

                    //Save Consumption
                    var energy = resultSites.Model.data.viewer.home.consumption.nodes[i];

                    //check if exist in db
                    Energy? energyExist = null;
                    if (existCheckDb && energy.from != null)
                    {
                        energyExist = await mscDbContext.Energy.FirstOrDefaultAsync(x => x.Timestamp == energy.from.Value && x.HomeId == homeService.CurrentHome().HomeId);
                        if (energyExist == null)
                        {
                            existCheckDb = false;
                            updateEntity = false;
                        }
                        else
                        {
                            energyExist.ElectricitySupplierPurchased = (int)ElectricitySupplierEnum.Tibber;
                            updateEntity = true;
                        }
                    }


                    if (energy.from != null)
                    {
                        if (energyExist == null)
                        {
                            energyExist = new Energy
                            {
                                HomeId = homeId,
                                // Unit = energy.consumptionUnit,
                                // Currency = energy.currency,
                                Timestamp = energy.from.Value,
                                ElectricitySupplierPurchased = (int)ElectricitySupplierEnum.Tibber
                            };
                        }

                        if (importOnlySpotPrice)
                        {
                            energyExist.Purchased = 0;
                            energyExist.PurchasedCost = 0;
                        }
                        else
                        {
                            energyExist.Purchased = energy.consumption.HasValue ? Convert.ToDouble(energy.consumption.Value) : 0;
                            energyExist.PurchasedCost = energy.cost.HasValue ? Convert.ToDouble(energy.cost.Value) : 0;
                        }

                        energyExist.UnitPriceBuy = energy.unitPrice.HasValue ? Convert.ToDouble(energy.unitPrice.Value) : 0;
                        energyExist.UnitPriceVatBuy = energy.unitPriceVAT.HasValue ? Convert.ToDouble(energy.unitPriceVAT.Value) : 0;

                        //production

                        var energyProd = resultSites.Model.data.viewer.home.production.nodes.FirstOrDefault(x => x.from == energy.from.Value);
                        if (energyProd != null)
                        {
                            energyExist.ElectricitySupplierProductionSold = (int)ElectricitySupplierEnum.Tibber;
                            if (importOnlySpotPrice)
                            {
                                energyExist.ProductionSold = 0;
                                energyExist.ProductionSoldProfit = 0;
                            }
                            else
                            {
                                energyExist.ProductionSold = energyProd.production.HasValue
                                    ? Convert.ToDouble(energyProd.production.Value)
                                    : 0;
                                energyExist.ProductionSoldProfit = energyProd.profit.HasValue
                                    ? Convert.ToDouble(energyProd.profit.Value)
                                    : 0;
                            }

                            energyExist.UnitPriceSold = energyProd.unitPrice.HasValue
                                ? Convert.ToDouble(energyProd.unitPrice.Value)
                                : 0;
                            energyExist.UnitPriceVatSold = energyProd.unitPriceVAT.HasValue
                                ? Convert.ToDouble(energyProd.unitPriceVAT.Value)
                                : 0;
                            //only flag row as synced if time has passed we import spot price for future hours
                            var dateTimeMinus15MinHeltimma = DateTime.Now.AddMinutes(-15);
                            if (energy!.to!.Value <= new DateTime(dateTimeMinus15MinHeltimma.Year, dateTimeMinus15MinHeltimma.Month, dateTimeMinus15MinHeltimma.Day, dateTimeMinus15MinHeltimma.Hour, 0, 0))
                            {
                                energyExist.ProductionSoldSynced = true;
                                energyExist.PurchasedSynced = true;
                            }

                            
                        }
                        //add rows 
                        if (!updateEntity)
                        {
                            var existInInsert = energyInsertList.FirstOrDefault(x => x.Timestamp == energyExist.Timestamp);
                            if (existInInsert != null)
                            {
                                for (int j = 0; j < energyInsertList.Count; j++)
                                {
                                    if (energyInsertList[j].Timestamp == energyExist.Timestamp)
                                    {
                                        energyInsertList[j] = energyExist;
                                        continue;
                                    }
                                }

                            }
                            else
                                energyInsertList.Add(energyExist);
                        }
                        else
                        {
                            energyUpdateList.Add(energyExist);
                        }
                        if (batch100 == 100)
                        {
                            await Task.Delay(100); //So that the GUI has time to update
                            progress.Report(progressStartNr);

                            batch100 = 0;
                            await mscDbContext.BulkInsertAsync(energyInsertList);
                            energyInsertList = new List<Energy>();
                            Console.WriteLine("Bulk insert OK");
                            await mscDbContext.BulkUpdateAsync(energyUpdateList);
                            energyUpdateList = new List<Energy>();
                            Console.WriteLine("Bulk update OK");


                        }
                    }
                }
                
            }
            //Last 100 or less
            if (energyInsertList.Count > 0 || energyUpdateList.Count > 0)
            {
                await Task.Delay(100); //So that the GUI has time to update
                progress.Report(progressStartNr);

                batch100 = 0;
                await mscDbContext.BulkInsertAsync(energyInsertList);
                energyInsertList = new List<Energy>();
                await mscDbContext.BulkUpdateAsync(energyUpdateList);
                energyUpdateList = new List<Energy>();
                
            }

            //Fill all null productions so that we don't have to loop throw then on more time.
            energyUpdateList = new List<Energy>();
            var emptyEnergyExist = mscDbContext.Energy.Where(x => x.ElectricitySupplierProductionSold == (int)InverterTypeEnum.Unknown && x.HomeId == homeId);
            foreach (var energy in emptyEnergyExist)
            {
                batch100++;
                energy.ElectricitySupplierProductionSold = (int)ElectricitySupplierEnum.Tibber;
                energy.ProductionSold = 0;
                energy.ProductionSoldProfit = 0;
                if (energy.Timestamp < DateTime.Now)
                    energy.ProductionSoldSynced = true;

                energyUpdateList.Add(energy);
                if (batch100 == 100)
                {
                    batch100 = 0;
                    await mscDbContext.BulkUpdateAsync(energyUpdateList);
                    energyUpdateList = new List<Energy>();
                }
            }

            if (energyUpdateList.Count > 0)
            {
                await mscDbContext.BulkUpdateAsync(energyUpdateList);
            }

            //-------- Sync future prices spot ---------------------
            var graphQlRequestTibberPrice = new GraphQlRequestTibber
            {
                variables = new TibberConsumptionProductionRequest
                {
                    homeid = homeService.FirstElectricitySupplier().SubSystemEntityId
                },
                query =
                    "query getdata($homeid: ID!) { viewer { home(id: $homeid) { currentSubscription { priceInfo { today { total energy tax level startsAt currency } tomorrow { total energy tax level startsAt currency }}}}}}"
            };
            var result = await restClient.ExecutePostAsync<TibberResponse>(string.Empty, graphQlRequestTibberPrice);
            if (!result.WasSuccessful || result.Model == null)
                return new Result<DataSyncResponse>(result.ErrorMessage);
            List<TibberPrice> listPrice = new List<TibberPrice>();
            if (result.Model.data.viewer.home.currentSubscription.priceInfo.today.Count > 0)
                listPrice.AddRange(result.Model.data.viewer.home.currentSubscription.priceInfo.today);
            if (result.Model.data.viewer.home.currentSubscription.priceInfo.tomorrow.Count > 0)
                listPrice.AddRange(result.Model.data.viewer.home.currentSubscription.priceInfo.tomorrow);

            foreach (var priceItem in listPrice)
            {
                var existPrice = await mscDbContext.Energy.FirstOrDefaultAsync(x =>
                    x.Timestamp == priceItem.startsAt && x.HomeId == homeService.CurrentHome().HomeId);
                if (existPrice == null)
                {
                    existPrice = new Energy
                    {
                        HomeId = homeId,
                        // Unit = priceItem.currency,
                        // Currency = priceItem.currency,
                        Timestamp = priceItem.startsAt
                    };
                    mscDbContext.Energy.Add(existPrice);
                }

                //only update when vat is = 0. else pick prices from consummations import
                if (existPrice.UnitPriceVatBuy == 0)
                {
                    existPrice.UnitPriceBuy = priceItem.total;
                    existPrice.UnitPriceSold = priceItem.energy;
                }

                existPrice.PriceLevel = priceItem.level;
                await mscDbContext.SaveChangesAsync();
            }
            return new Result<DataSyncResponse>(new DataSyncResponse
            {
                Message = AppResources.Import_Data_From_Electricity_Supplier_Done,
                SyncState = DataSyncState.ElectricySync
            });
        }
        catch (Exception ex)
        {
            return new Result<DataSyncResponse>(ex.Message);
        }
    }
}


