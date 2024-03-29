using MySolarCellsSQLite.Sqlite;
using MySolarCellsSQLite.Sqlite.Models;

namespace MySolarCells.Services.GridSupplier;

public class TibberService : IGridSupplierInterface
{
    private readonly MscDbContext mscDbContext;
    private readonly IRestClient restClient;
    public string GuideText => AppResources.Tibber_Guide_Text;
    public string DefaultApiUrl => "";
    public string NavigationUrl => "https://developer.tibber.com/settings/access-token";
    public bool ShowUserName => false;

    public bool ShowPassword => false;

    public bool ShowApiUrl => false;

    public bool ShowApiKey => true;
    public bool ShowNavigateUrl => true;

    private GridSupplierLoginResponse? gridSupplierLoginResponse;

    public TibberService(IRestClient restClient, MscDbContext mscDbContext)
    {
        this.restClient = restClient;
        this.mscDbContext = mscDbContext;
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
                string.Format("Bearer {0}",
                    StringHelper.Decrypt(MySolarCellsGlobals.SelectedHome.ApiKey, AppConstants.Secretkey))
            }
        };
        restClient.UpdateToken(tokens);
    }

    public async Task<Result<GridSupplierLoginResponse>> TestConnection(string userName, string password, string apiUrl,
        string apiKey)
    {
        Dictionary<string, string> tokens = new Dictionary<string, string>
        {
            { AppConstants.Authorization, string.Format("Bearer {0}", apiKey) }
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
        else
        {
            gridSupplierLoginResponse = new GridSupplierLoginResponse
                { ApiKey = apiKey, ResponseText = result.Model.data.viewer.accountType.First() };
            return new Result<GridSupplierLoginResponse>(new GridSupplierLoginResponse
                { ApiKey = apiKey, ResponseText = result.Model.data.viewer.accountType.First() });
        }
    }

    public async Task<Result<List<Home>>> GetPickerOne()
    {
        GraphQlRequestTibber graphQlRequestTibber = new GraphQlRequestTibber
        {
            query = "{ viewer { homes { id address { address1 postalCode city country } }}}"
        };
        var result = await restClient.ExecutePostAsync<TibberResponse>(string.Empty, graphQlRequestTibber);
        if (!result.WasSuccessful || result.Model == null)
            return new Result<List<Home>>(result.ErrorMessage);
        else
        {
            List<Home> homes = new List<Home>();
            foreach (var item in result.Model.data.viewer.homes)
            {
                homes.Add(new Home
                {
                    ApiKey = gridSupplierLoginResponse?.ApiKey ?? string.Empty,
                    ElectricitySupplier = (int)ElectricitySupplier.Tibber,
                    Name = item.address.address1,
                    SubSystemEntityId = item.id
                });
            }

            return new Result<List<Home>>(homes);
        }
    }

    public async Task<Result<DataSyncResponse>> Sync(DateTime start, IProgress<int> progress, int progressStartNr)
    {
        try
        {
            ReInitRest();
            int batch100 = 0;
            int homeId = MySolarCellsGlobals.SelectedHome.HomeId;
            bool importOnlySpotPrice = MySolarCellsGlobals.SelectedHome.ImportOnlySpotPrice;
            List<Energy> energyInsertList = new List<Energy>();
            List<Energy> energyUpdateList = new List<Energy>();

            var dates = DateHelper.GetRelatedDates(DateTime.Today);
            DateTime end = dates.BaseDate.AddDays(1);
            //We want to sync Spot Price prices hole day
            //other data only until last hour  
            while (start < end)
            {
                TimeSpan difference;
                if (start.AddMonths(3) < end)
                {
                    difference = start.AddMonths(3) - start;
                }
                else
                {
                    difference = end - start;
                }

                var days = difference.Days;
                var hours = difference.Hours;
                var hoursTot = (days * 24) + hours;

                GraphQlRequestTibber graphQlRequestTibber = new GraphQlRequestTibber
                {
                    variables = new TibberConsumptionProductionRequest
                    {
                        homeid = MySolarCellsGlobals.SelectedHome.SubSystemEntityId,
                        from = StringHelper.EncodeTo64(String.Format("{0:s}", start)), //start.ToString() "yyyy-MM-dd
                        first = hoursTot
                    }
                };
                graphQlRequestTibber.query =
                    "query getdata($homeid: ID!, $from: String, $first: Int) { viewer { home(id: $homeid) { consumption(resolution: HOURLY, after: $from, first: $first) { nodes { from to cost unitPrice unitPriceVAT consumption consumptionUnit currency } } production(resolution: HOURLY, after: $from, first: $first) { nodes { from to profit unitPrice unitPriceVAT production productionUnit currency}}}}}";
                var resultSites =
                    await restClient.ExecutePostAsync<TibberResponse>(string.Empty, graphQlRequestTibber);
                if (resultSites.Model == null)
                    return new Result<DataSyncResponse>("No values");
                var cunsumI = resultSites.Model.data.viewer.home.consumption.nodes.Count;


                //used to check så the backend not give the same value twice
                bool existCheckDb = true;
                bool updateEntity = true;
                for (int i = 0; i < cunsumI; i++)
                {
                    progressStartNr++;
                    batch100++;

                    //Save Consumption
                    var energy = resultSites.Model.data.viewer.home.consumption.nodes[i];

                    //hoppar ifall det är nuvarande timma eller mer
                    //if (energy.from.Value > DateTime.Now.AddHours(-1))
                    //    continue;

                    //check if exist in db
                    Energy? energyExist = null;
                    if (existCheckDb && energy.from != null)
                    {
                        energyExist = await mscDbContext.Energy.FirstOrDefaultAsync(x =>
                            x.Timestamp == energy.from.Value && x.HomeId == MySolarCellsGlobals.SelectedHome.HomeId);
                        if (energyExist == null)
                        {
                            existCheckDb = false;
                            updateEntity = false;
                        }
                        else
                        {
                            updateEntity = true;
                        }
                    }

                    if (energyExist == null)
                    {
                        //if already added
                        if (energyInsertList.Any(x => energy.from != null && x.Timestamp == energy.from.Value))
                            continue;

                        if (!updateEntity && energy.from != null)
                        {
                            energyExist = new Energy
                            {
                                HomeId = homeId,
                                Unit = energy.consumptionUnit,
                                Currency = energy.currency,
                                Timestamp = energy.from.Value
                            };


                            energyExist.ElectricitySupplierPurchased = (int)ElectricitySupplier.Tibber;


                            if (importOnlySpotPrice)
                            {
                                energyExist.Purchased = 0;
                                energyExist.PurchasedCost = 0;
                            }
                            else
                            {
                                energyExist.Purchased = energy.consumption.HasValue
                                    ? Convert.ToDouble(energy.consumption.Value)
                                    : 0;
                                energyExist.PurchasedCost =
                                    energy.cost.HasValue ? Convert.ToDouble(energy.cost.Value) : 0;
                            }

                            energyExist.UnitPriceBuy =
                                energy.unitPrice.HasValue ? Convert.ToDouble(energy.unitPrice.Value) : 0;
                            energyExist.UnitPriceVatBuy = energy.unitPriceVAT.HasValue
                                ? Convert.ToDouble(energy.unitPriceVAT.Value)
                                : 0;
                            //only flag row as synced if time has passed we import spotprice for future hours 
                            if (energyExist.Timestamp < DateTime.Now.AddHours(-2))
                                energyExist.PurchasedSynced = true;
                            //production
                           
                                var energyProd =
                                    resultSites.Model.data.viewer.home.production.nodes.FirstOrDefault(x =>
                                        x.from == energy.from.Value);
                                if (energyProd != null)
                                {
                                    energyExist.ElectricitySupplierProductionSold = (int)ElectricitySupplier.Tibber;
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
                                    //only flag row as synced if time has passed we import spotprice for future hours 
                                    if (energyExist.Timestamp < DateTime.Now.AddHours(-2))
                                        energyExist.ProductionSoldSynced = true;
                                }
                           

                            if (!updateEntity)
                                energyInsertList.Add(energyExist);
                            else
                                energyUpdateList.Add(energyExist);


                            if (batch100 == 100)
                            {
                                await Task.Delay(100); //Så att GUI hinner uppdatera
                                progress.Report(progressStartNr);

                                batch100 = 0;
                                await mscDbContext.BulkInsertAsync(energyInsertList);
                                await mscDbContext.BulkUpdateAsync(energyUpdateList);
                                energyInsertList = new List<Energy>();
                                energyUpdateList = new List<Energy>();
                            }
                        }
                    }
                }

                if (energyInsertList.Count > 0)
                {
                    await Task.Delay(100); //Så att GUI hinner uppdatera
                    progress.Report(progressStartNr);

                    batch100 = 0;
                    await mscDbContext.BulkInsertAsync(energyInsertList);
                    await mscDbContext.BulkUpdateAsync(energyUpdateList);
                    energyInsertList = new();
                    energyUpdateList = new();
                }

                //Fill all null productions so that we don't have to loop throw then on more time.
                var emptyEnergyExist = mscDbContext.Energy.Where(x =>
                    x.ElectricitySupplierProductionSold == (int)InverterTyp.Unknown && x.HomeId == homeId);
                foreach (var energy in emptyEnergyExist)
                {
                    batch100++;
                    energy.ElectricitySupplierProductionSold = (int)ElectricitySupplier.Tibber;
                    energy.ProductionSold = 0;
                    energy.ProductionSoldProfit = 0;
                    if (energy.Timestamp < DateTime.Now)
                        energy.ProductionSoldSynced = true;

                    energyInsertList.Add(energy);
                    if (batch100 == 100)
                    {
                        batch100 = 0;
                        await mscDbContext.BulkUpdateAsync(energyInsertList);
                        energyInsertList = new List<Energy>();
                    }
                }

                if (energyInsertList.Count > 0)
                {
                    await mscDbContext.BulkUpdateAsync(energyInsertList);
                }

                //-------- Sync future prices spot ---------------------
                var graphQlRequestTibberPrice = new GraphQlRequestTibber
                {
                    variables = new TibberConsumptionProductionRequest
                    {
                        homeid = MySolarCellsGlobals.SelectedHome.SubSystemEntityId
                    },
                    query =
                        "query getdata($homeid: ID!) { viewer { home(id: $homeid) { currentSubscription { priceInfo { today { total energy tax level startsAt currency } tomorrow { total energy tax level startsAt currency }}}}}}"
                };
                var result =
                    await restClient.ExecutePostAsync<TibberResponse>(string.Empty, graphQlRequestTibberPrice);
                if (!result.WasSuccessful || result.Model == null)
                    return new Result<DataSyncResponse>(result.ErrorMessage);
                else
                {
                    List<TibberPrice> listPrice = new List<TibberPrice>();
                    if (result.Model.data.viewer.home.currentSubscription.priceInfo.today.Count > 0)
                        listPrice.AddRange(result.Model.data.viewer.home.currentSubscription.priceInfo.today);
                    if (result.Model.data.viewer.home.currentSubscription.priceInfo.tomorrow.Count > 0)
                        listPrice.AddRange(result.Model.data.viewer.home.currentSubscription.priceInfo.tomorrow);

                    foreach (var priceItem in listPrice)
                    {
                        var existPrice = await mscDbContext.Energy.FirstOrDefaultAsync(x =>
                            x.Timestamp == priceItem.startsAt && x.HomeId == MySolarCellsGlobals.SelectedHome.HomeId);
                        if (existPrice == null)
                        {
                            existPrice = new Energy
                            {
                                HomeId = homeId,
                                Unit = priceItem.currency,
                                Currency = priceItem.currency,
                                Timestamp = priceItem.startsAt
                            };
                            mscDbContext.Energy.Add(existPrice);
                        }

                        //only update when vat is = 0. else pick prices from consumtions import
                        if (existPrice.UnitPriceVatBuy == 0)
                        {
                            existPrice.UnitPriceBuy = priceItem.total;
                            existPrice.UnitPriceSold = priceItem.energy;
                        }

                        existPrice.PriceLevel = priceItem.level;
                        await mscDbContext.SaveChangesAsync();
                    }
                }

                return new Result<DataSyncResponse>(new DataSyncResponse
                {
                    Message = AppResources.Import_Data_From_Electricity_Supplier_Done,
                    SyncState = DataSyncState.ElectricySync
                });
            }

            return new Result<DataSyncResponse>("Error in import");
        }
        catch (Exception ex)
        {
            return new Result<DataSyncResponse>(ex.Message);
        }
    }
}

//Requests
public class GraphQlRequestTibber
{
    //public string operationName { get; set; }
    public Object variables { get; set; } = new();
    public string query { get; set; } = "";
}

public class TibberConsumptionProductionRequest
{
    public string homeid { get; set; } = "";
    public string from { get; set; } = "";
    public int first { get; set; }
}

//Responses

public class TibberResponse
{
    public TibberData data { get; set; } = new TibberData();
}

public class TibberData
{
    public TibberViewer viewer { get; set; } = new TibberViewer();
}

public class TibberViewer
{
    public TibberHome home { get; set; } = new();
    public List<string> accountType { get; set; } = new();
    public List<TibberHome> homes { get; set; } = new();
}

public class TibberAddress
{
    public string address1 { get; set; } = "";
    public object address2 { get; set; } = new();
    public object address3 { get; set; } = new();
    public string postalCode { get; set; } = "";
    public string city { get; set; } = "";
    public string country { get; set; } = "";
    public string latitude { get; set; } = "";
    public string longitude { get; set; } = "";
}

public class TibberHome
{
    public string id { get; set; } = "";
    public TibberConsumption consumption { get; set; } = new();
    public TibberProduction production { get; set; } = new();
    public TibberAddress address { get; set; } = new();
    public TibberCurrentSubscription currentSubscription { get; set; } = new();
}

public class TibberCurrentSubscription
{
    public TibberPriceInfo priceInfo { get; set; } = new();
}

public class TibberPriceInfo
{
    public List<TibberPrice> today { get; set; } = new();
    public List<TibberPrice> tomorrow { get; set; } = new();
}

public class TibberPrice
{
    public double total { get; set; }
    public double energy { get; set; }
    public double tax { get; set; }
    public string level { get; set; } = "";
    public DateTime startsAt { get; set; }
    public string currency { get; set; } = "";
}

public class TibberConsumption
{
    public List<TibberConsumptionNode> nodes { get; set; } = new();
}

public class TibberProduction
{
    public List<TibberProductionNode> nodes { get; set; } = new();
}

public class TibberConsumptionNode
{
    public DateTime? from { get; set; }
    public DateTime? to { get; set; }
    public decimal? cost { get; set; }
    public decimal? unitPrice { get; set; }
    public decimal? unitPriceVAT { get; set; }
    public decimal? consumption { get; set; }
    public string consumptionUnit { get; set; } = "";
    public string currency { get; set; } = "";
}

public class TibberProductionNode
{
    public DateTime? from { get; set; }
    public DateTime? to { get; set; }
    public decimal? profit { get; set; }
    public decimal? unitPrice { get; set; }
    public decimal? unitPriceVAT { get; set; }
    public decimal? production { get; set; }
    public string productionUnit { get; set; } = "";
    public string currency { get; set; } = "";
}