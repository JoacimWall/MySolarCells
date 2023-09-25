using System.Net.Http.Headers;
using System.Text.Json;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Tibber.Sdk;

namespace MySolarCells.Services;
public interface ITibberService
{
    bool Init(string apiKey);
    Task<Result<List<Home>>> GetBasicData();
    Task<bool> SyncConsumptionAndProductionFirstTime(bool firstTime, IProgress<int> progress, int progressStartNr);
    //Task<bool> GetHomeProduction(string SubSystemEntityId, int hours, int homeId);
}
public class TibberService : ITibberService
{
    private ProductInfoHeaderValue userAgent = new ProductInfoHeaderValue("my-solar-cells", "0.1");
    private TibberApiClient client;
    private IRestClient restClient;
    public TibberService(IRestClient restClient)
    {
        this.restClient = restClient;
        Dictionary<string, string> defaultRequestHeaders = new Dictionary<string, string>();
        //defaultRequestHeaders.Add("White-Label-Access-Key", "bf8c1e86-b23f-49f6-9156-a1e6a08895d7");

        this.restClient.ApiSettings = new ApiSettings { BaseUrl = "https://api.tibber.com/v1-beta/gql", defaultRequestHeaders = defaultRequestHeaders };
        this.restClient.ReInit();
        Dictionary<string, string> tokens = new Dictionary<string, string>
        {
            { AppConstants.Authorization, string.Format("Bearer {0}", "XiKN2tjMlXqX_iQ_VHUkScawRgIH6571DouqqZVHN8k") }
        };
        this.restClient.UpdateToken(tokens);



    }
    public bool Init(string apiKey)
    {
        this.client = new TibberApiClient(apiKey, this.userAgent);

        return true;
    }
    public async Task<Result<List<Home>>> GetBasicData()
    {
        var basicData = await client.GetBasicData();
        using var fmContext = new MscDbContext();
        //TODO:Handel errors
        if (basicData.Errors != null)
        {
            return new Result<List<Home>>(basicData.Errors.First().Message);
        }
        else
        {
            return new Result<List<Home>>(basicData.Data.Viewer.Homes.ToList());

        }

    }
    public async Task<bool> SyncConsumptionAndProductionFirstTime(bool firstTime, IProgress<int> progress, int progressStartNr)
    {
        try
        {
            int batch100 = 0;
            int homeId = MySolarCellsGlobals.SelectedHome.HomeId;
            List<Sqlite.Models.Energy> eneryList = new List<Sqlite.Models.Energy>();
            using var dbContext = new MscDbContext();
            DateTime start = MySolarCellsGlobals.SelectedHome.FromDate;
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
                    nextStart = start.AddMonths(3);
                }
                else
                {
                    difference = end - start;
                    nextStart = end;
                }

                days = difference.Days;
                hours = difference.Hours;
                hoursTot = (days * 24) + hours;

                GraphQlRequestTibber graphQlRequestTibber = new GraphQlRequestTibber
                {
                    variables = new TibberConsumptionProductionRequest
                    {
                        homeid = MySolarCellsGlobals.SelectedHome.SubSystemEntityId,
                        from = StringHelper.EncodeTo64(start.ToString("yyyy-MM-dd")),
                        first = hoursTot
                    }
                };
                graphQlRequestTibber.query = "query getdata($homeid: ID!, $from: String, $first: Int) { viewer { home(id: $homeid) { consumption(resolution: HOURLY, after: $from, first: $first) { nodes { from to cost unitPrice unitPriceVAT consumption consumptionUnit currency } } production(resolution: HOURLY, after: $from, first: $first) { nodes { from to profit unitPrice unitPriceVAT production productionUnit currency}}}}}";
                var resultSites = await this.restClient.ExecutePostAsync<TibberConsumptionResponse>(string.Empty, graphQlRequestTibber);

                var cunsumI = resultSites.Model.data.viewer.home.consumption.nodes.Count;



                
                for (int i = 0; i < cunsumI; i++)
                {
                    
                    progressStartNr++;
                    batch100++;
                    
                    //Save Consumtion
                    var energy = resultSites.Model.data.viewer.home.consumption.nodes[i];
                    //check if exist in db
                    //var energyExist = await dbContext.Energy.FirstOrDefaultAsync(x => x.Timestamp == energy.from.Value && x.HomeId == MySolarCellsGlobals.SelectedHome.HomeId);
                    var energyExist = new Sqlite.Models.Energy
                    {
                        HomeId = homeId,
                        Unit = energy.consumptionUnit,
                        Currency = energy.currency,
                        Timestamp = energy.from.Value
                    };
                    await dbContext.Energy.AddAsync(energyExist);

                    energyExist.ElectricitySupplierPurchased = (int)ElectricitySupplier.Tibber;
                    energyExist.Purchased = Convert.ToDouble(energy.consumption.Value);
                    energyExist.PurchasedCost = Convert.ToDouble(energy.cost.Value);
                    energyExist.UnitPriceBuy = Convert.ToDouble(energy.unitPrice.Value);
                    energyExist.UnitPriceVatBuy = Convert.ToDouble(energy.unitPriceVAT.Value);
                    energyExist.PurchasedSynced = true;
                    //production
                    if (resultSites.Model.data.viewer.home.production != null && resultSites.Model.data.viewer.home.production.nodes != null)
                    {
                        var energyProd = resultSites.Model.data.viewer.home.production.nodes.FirstOrDefault(x => x.from == energy.from.Value);
                        if (energyProd != null)
                        {
                            energyExist.ElectricitySupplierProductionSold = (int)ElectricitySupplier.Tibber;
                            energyExist.ProductionSold = energyProd.production.HasValue ? Convert.ToDouble(energyProd.production.Value) : 0;
                            energyExist.ProductionSoldProfit = energyProd.profit.HasValue ? Convert.ToDouble(energyProd.profit.Value) : 0;
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
    //public async Task<bool> GetHomeProduction(string SubSystemEntityId, int hours, int homeId)
    //{
    //    var production = await client.GetHomeProduction(Guid.Parse(SubSystemEntityId), EnergyResolution.Hourly, hours);
    //    using var fmContext = new MscDbContext();
    //    //TODO:Handel errors

    //    foreach (var energy in production)
    //    {
    //        //check if Home exist in db
    //        var energyExist = await fmContext.Energy.FirstOrDefaultAsync(x => x.Timestamp == energy.From.Value.DateTime && x.HomeId == homeId);
    //        if (energyExist == null)
    //        {
    //            energyExist = new Sqlite.Models.Energy
    //            {
    //                HomeId = homeId,
    //                Unit = energy.ProductionUnit,
    //                Currency = energy.Currency,
    //                Timestamp = energy.From.Value.DateTime
    //            };
    //            await fmContext.Energy.AddAsync(energyExist);
    //        }
    //        energyExist.ElectricitySupplierProductionSold = (int)ElectricitySupplier.Tibber;
    //        energyExist.ProductionSold = energy.Production.HasValue ? energy.Production.Value : 0;
    //        energyExist.ProductionSoldProfit = energy.Profit.HasValue ? energy.Profit.Value : 0;
    //        energyExist.UnitPriceSold = energy.UnitPrice.HasValue ? energy.UnitPrice.Value : 0;
    //        energyExist.UnitPriceVatSold = energy.UnitPriceVat.HasValue ? energy.UnitPriceVat.Value : 0;

    //        await fmContext.SaveChangesAsync();


    //    }
    //    return true;
    //}
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
public class TibberConsumptionResponse
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
}


public class TibberHome
{
    public TibberConsumption consumption { get; set; }
    public TibberProduction production { get; set; }
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





