
using System.Net.Http.Headers;

namespace MySolarCells.Services.GridSupplier;



public class TibberService : IGridSupplierInterface
{
    
    private IRestClient restClient;
    public string GuideText => "To use Tibber you ned your private API key. You can get that by click on the button below and login.";
    public string DefaultApiUrl => "";
    public bool ShowUserName => false;

    public bool ShowPassword => false;

    public bool ShowApiUrl => false;

    public bool ShowApiKey => true;
    public bool ShowNavigateUrl => true;

    private GridSupplierLoginResponse gridSupplierLoginResponse;
    public TibberService(IRestClient restClient)
    {
        this.restClient = restClient;
        Dictionary<string, string> defaultRequestHeaders = new Dictionary<string, string>();
       
        this.restClient.ApiSettings = new ApiSettings { BaseUrl = "https://api.tibber.com/v1-beta/gql", defaultRequestHeaders = defaultRequestHeaders };
        ReInitRest();


    }
    private void ReInitRest()
    {
        this.restClient.ReInit();
        if (MySolarCellsGlobals.SelectedHome != null)
        {
            Dictionary<string, string> tokens = new Dictionary<string, string>
            {
                { AppConstants.Authorization, string.Format("Bearer {0}", StringHelper.Decrypt(MySolarCellsGlobals.SelectedHome.ApiKey, AppConstants.Secretkey)) }
            };
            this.restClient.UpdateToken(tokens);
        }
    }
   
    public async Task<Result<GridSupplierLoginResponse>> TestConnection(string userName, string password, string apiUrl, string apiKey)
    {
       
        Dictionary<string, string> tokens = new Dictionary<string, string>
            {
                { AppConstants.Authorization, string.Format("Bearer {0}", apiKey) }
            };
        this.restClient.UpdateToken(tokens);
        GraphQlRequestTibber graphQlRequestTibber = new GraphQlRequestTibber
        {
            query = "{ viewer { accountType }}"
        };
        
        var result = await this.restClient.ExecutePostAsync<TibberResponse>(string.Empty, graphQlRequestTibber);
        if (!result.WasSuccessful)
            return new Result<GridSupplierLoginResponse>(result.ErrorMessage);
        else
        {
            gridSupplierLoginResponse = new GridSupplierLoginResponse { ApiKey = apiKey, ResponseText = result.Model.data.viewer.accountType.First() };
            return new Result<GridSupplierLoginResponse>(new GridSupplierLoginResponse { ApiKey = apiKey, ResponseText = result.Model.data.viewer.accountType.First() });

        }



        

    }
    public async Task<Result<List<Sqlite.Models.Home>>> GetPickerOne()
    {
        GraphQlRequestTibber graphQlRequestTibber = new GraphQlRequestTibber
        {
            query = "{ viewer { homes { id address { address1 postalCode city country } }}}"
        };
        var result = await this.restClient.ExecutePostAsync<TibberResponse>(string.Empty, graphQlRequestTibber);
        if (!result.WasSuccessful)
            return new Result<List<Sqlite.Models.Home>>(result.ErrorMessage);
        else
        {
            List<Sqlite.Models.Home> homes = new List<Sqlite.Models.Home>();
            foreach (var item in result.Model.data.viewer.homes)
            {
                homes.Add(new Sqlite.Models.Home
                {
                     ApiKey = gridSupplierLoginResponse.ApiKey,
                      ElectricitySupplier = (int)ElectricitySupplier.Tibber,
                      Name = item.address.address1,
                      SubSystemEntityId = item.id

                });
            }
            return new Result<List<Sqlite.Models.Home>>(homes);

        }
        //if (basicData.Errors != null)
        //{
        //    return new Result<List<Sqlite.Models.Home>>(basicData.Errors.First().Message);
        //}
        //else
        //{
        //    var returlist = new List<Sqlite.Models.Home>();
        //    foreach (var item in basicData.Data.Viewer.Homes.ToList())
        //    {
        //        returlist.Add(new Sqlite.Models.Home
        //        {
        //            ApiKey = StringHelper.Decrypt(MySolarCellsGlobals.SelectedHome.ApiKey, AppConstants.Secretkey),
        //            SubSystemEntityId = item.Id.ToString(),
        //            Name = item.AppNickname
        //        });
        //    }
        //    return new Result<List<Sqlite.Models.Home>>(returlist);

        //}
    }
    public async Task<bool> Sync(DateTime start, IProgress<int> progress, int progressStartNr)
    {
        try
        {
            ReInitRest();
            int batch100 = 0;
            int homeId = MySolarCellsGlobals.SelectedHome.HomeId;
            bool importOnlySpotPrice = MySolarCellsGlobals.SelectedHome.ImportOnlySpotPrice;
            List<Sqlite.Models.Energy> eneryList = new List<Sqlite.Models.Energy>();
            using var dbContext = new MscDbContext();
            //DateTime start = MySolarCellsGlobals.SelectedHome.FromDate;
            DateTime end = DateTime.Now;
            DateTime nextStart = new DateTime();
            TimeSpan difference = new TimeSpan();
            int days = 0;
            int hours = 0;
            int hoursTot = 0;
            //TODO:Remove just for test
            // await dbContext.Energy.BatchDeleteAsync();
            while (start < end)
            {
                //Get 3 mounts per request
                if (start.AddMonths(3) < end)
                {
                    difference = start.AddMonths(3) - start;
                    nextStart = start.AddMonths(3).AddHours(1);
                }
                else
                {
                    difference = end - start;
                    nextStart = end.AddHours(1);
                }

                days = difference.Days;
                hours = difference.Hours;
                hoursTot = (days * 24) + hours;

                GraphQlRequestTibber graphQlRequestTibber = new GraphQlRequestTibber
                {
                    variables = new TibberConsumptionProductionRequest
                    {
                        homeid = MySolarCellsGlobals.SelectedHome.SubSystemEntityId,
                        from = StringHelper.EncodeTo64(String.Format("{0:s}", start)), //start.ToString() "yyyy-MM-dd
                        first = hoursTot
                    }
                };
                graphQlRequestTibber.query = "query getdata($homeid: ID!, $from: String, $first: Int) { viewer { home(id: $homeid) { consumption(resolution: HOURLY, after: $from, first: $first) { nodes { from to cost unitPrice unitPriceVAT consumption consumptionUnit currency } } production(resolution: HOURLY, after: $from, first: $first) { nodes { from to profit unitPrice unitPriceVAT production productionUnit currency}}}}}";
                var resultSites = await this.restClient.ExecutePostAsync<TibberResponse>(string.Empty, graphQlRequestTibber);

                var cunsumI = resultSites.Model.data.viewer.home.consumption.nodes.Count;


                //used to check så the backend not give the same value twice
                bool existCheckDb = true;
                for (int i = 0; i < cunsumI; i++)
                {

                    progressStartNr++;
                    batch100++;

                    //Save Consumtion
                    var energy = resultSites.Model.data.viewer.home.consumption.nodes[i];
                    //check if exist in db
                    if (existCheckDb)
                    {
                        var energyExistAllready = await dbContext.Energy.FirstOrDefaultAsync(x => x.Timestamp == energy.from.Value && x.HomeId == MySolarCellsGlobals.SelectedHome.HomeId);
                        if (energyExistAllready == null)
                        {
                            existCheckDb = false;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    //if allready added
                    if (eneryList.Any(x => x.Timestamp == energy.from.Value))
                        continue;

                    var energyExist = new Sqlite.Models.Energy
                    {
                        HomeId = homeId,
                        Unit = energy.consumptionUnit,
                        Currency = energy.currency,
                        Timestamp = energy.from.Value
                    };
                    // await dbContext.Energy.AddAsync(energyExist);

                    energyExist.ElectricitySupplierPurchased = (int)ElectricitySupplier.Tibber;
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
                    energyExist.PurchasedSynced = true;
                    //production
                    if (resultSites.Model.data.viewer.home.production != null && resultSites.Model.data.viewer.home.production.nodes != null)
                    {
                        var energyProd = resultSites.Model.data.viewer.home.production.nodes.FirstOrDefault(x => x.from == energy.from.Value);
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
                                energyExist.ProductionSold = energyProd.production.HasValue ? Convert.ToDouble(energyProd.production.Value) : 0;
                                energyExist.ProductionSoldProfit = energyProd.profit.HasValue ? Convert.ToDouble(energyProd.profit.Value) : 0;
                            }
                            energyExist.UnitPriceSold = energyProd.unitPrice.HasValue ? Convert.ToDouble(energyProd.unitPrice.Value) : 0;
                            energyExist.UnitPriceVatSold = energyProd.unitPriceVAT.HasValue ? Convert.ToDouble(energyProd.unitPriceVAT.Value) : 0;
                            energyExist.ProductionSoldSynced = true;

                        }
                    }
                    eneryList.Add(energyExist);

                    if (batch100 == 100)
                    {
                        await Task.Delay(100); //Så att GUI hinner uppdatera
                        progress.Report(progressStartNr);

                        batch100 = 0;
                        await dbContext.BulkInsertAsync(eneryList);
                        eneryList = new List<Sqlite.Models.Energy>();
                    }



                }


                start = nextStart;
            }
            if (eneryList.Count > 0)
            {
                await Task.Delay(100); //Så att GUI hinner uppdatera
                progress.Report(progressStartNr);

                batch100 = 0;
                await dbContext.BulkInsertAsync(eneryList);
                eneryList = new List<Sqlite.Models.Energy>();

            }

            //Fill all null productions so that we don't have to loop throw then on more time.
            var emptyEnergyExist = dbContext.Energy.Where(x => x.ElectricitySupplierProductionSold == (int)InverterTyp.Unknown && x.HomeId == homeId);
            foreach (var enery in emptyEnergyExist)
            {
                batch100++;
                enery.ElectricitySupplierProductionSold = (int)ElectricitySupplier.Tibber;
                enery.ProductionSold = 0;
                enery.ProductionSoldProfit = 0;
                enery.ProductionSoldSynced = true;
                eneryList.Add(enery);
                if (batch100 == 100)
                {
                    //await Task.Delay(100); //Så att GUI hinner uppdatera
                    //progress.Report(progressStartNr);

                    batch100 = 0;
                    await dbContext.BulkUpdateAsync(eneryList);
                    eneryList = new List<Sqlite.Models.Energy>();
                }
            }

            if (eneryList.Count > 0)
            {
                //await Task.Delay(100); //Så att GUI hinner uppdatera
                //progress.Report(progressStartNr);

                batch100 = 0;
                await dbContext.BulkUpdateAsync(eneryList);
                eneryList = new List<Sqlite.Models.Energy>();

            }



            return true;

        }
        catch (Exception ex)
        {
            return false;
        }
    }

    
}

//Requests
public class GraphQlRequestTibber
{
    //public string operationName { get; set; }
    public Object variables { get; set; }
    public string query { get; set; }
}

public class TibberConsumptionProductionRequest
{
    public string homeid { get; set; }
    public string from { get; set; }
    public int first { get; set; }

}


//Responses

public class TibberResponse
{
    public TibberData data { get; set; }
}
public class TibberData
{
    public TibberViewer viewer { get; set; }
}

public class TibberViewer
{
    public TibberHome home { get; set; }
    public List<string> accountType { get; set; }
    public List<TibberHome> homes { get; set; }
}

public class TibberAddress
{
    public string address1 { get; set; }
    public object address2 { get; set; }
    public object address3 { get; set; }
    public string postalCode { get; set; }
    public string city { get; set; }
    public string country { get; set; }
    public string latitude { get; set; }
    public string longitude { get; set; }
}
public class TibberHome
{
    public string id { get; set; }
    public TibberConsumption consumption { get; set; }
    public TibberProduction production { get; set; }
    public TibberAddress address { get; set; }

}
// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
public class TibberConsumption
{
    public List<TibberConsumptionNode> nodes { get; set; }
}
public class TibberProduction
{
    public List<TibberProductionNode> nodes { get; set; }
}
public class TibberConsumptionNode
{
    public DateTime? from { get; set; }
    public DateTime? to { get; set; }
    public decimal? cost { get; set; }
    public decimal? unitPrice { get; set; }
    public decimal? unitPriceVAT { get; set; }
    public decimal? consumption { get; set; }
    public string consumptionUnit { get; set; }
    public string currency { get; set; }

}
public class TibberProductionNode
{
    public DateTime? from { get; set; }
    public DateTime? to { get; set; }
    public decimal? profit { get; set; }
    public decimal? unitPrice { get; set; }
    public decimal? unitPriceVAT { get; set; }
    public decimal? production { get; set; }
    public string productionUnit { get; set; }
    public string currency { get; set; }
}





