using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EFCore.BulkExtensions;

namespace MySolarCells.Services;

public interface IKostalService
{
    Task<Result<KostalLoginResponse>> LoginInUser(string userName, string password);
    Task<Result<SitesResponse>> GetSites();
    Task<Result<Device>> GetInverter(int siteNodeId);
    Task<bool> SyncProductionOwnUseFirstTime(bool firstTime, IProgress<int> progress, int progressStartNr);
}
public class KostalService : IKostalService
{
    private IRestClient restClient;
    private KostalLoginResponse kostalLoginResponse;
    private SitesResponse sitesResponse;
    private DeviceResponse deviceResponse;
    public KostalService(IRestClient restClient)
    {
        this.restClient = restClient;
        Dictionary<string, string> defaultRequestHeaders = new Dictionary<string, string>();
        defaultRequestHeaders.Add("White-Label-Access-Key", "bf8c1e86-b23f-49f6-9156-a1e6a08895d7");

        this.restClient.ApiSettings = new ApiSettings { BaseUrl = "https://kostal-api.solytic.com/api/graphql", defaultRequestHeaders = defaultRequestHeaders };
        this.restClient.ReInit();
    }
    public async Task<Result<KostalLoginResponse>> LoginInUser(string userName, string password)
    {
        //LOGIN 
        string query = "mutation LOGIN_USER_MUTATION($emailAddress: String!, $password: String!, $termsOfUseAccepted: Boolean, $marketingOptIn: Boolean) {  loginUser(user: {emailAddress: $emailAddress, password: $password, termsOfUseAccepted: $termsOfUseAccepted, marketingOptIn: $marketingOptIn}) {    accessToken {      token      tokenType      expiresIn      __typename    }    refreshToken    user {      ...userMenuFragment      __typename    }    __typename  }}fragment userMenuFragment on User {  id  emailAddress  customer {    id    __typename  }  contact {    id    firstName    lastName    address {      id      country      __typename    }    __typename  }  userRoles {    id    name    __typename  }  locale  __typename}";
        var variables = new LoginVariables { emailAddress = userName, password = password };
        GraphQlRequest graphQlRequest = new GraphQlRequest { operationName = "LOGIN_USER_MUTATION", variables = variables, query = query };
        var loginResult = await this.restClient.ExecutePostAsync<KostalLoginResponse>(string.Empty, graphQlRequest);
        if (!loginResult.WasSuccessful)
            return new Result<KostalLoginResponse>(loginResult.ErrorMessage);

        kostalLoginResponse = loginResult.Model;

        Dictionary<string, string> tokens = new Dictionary<string, string>
        {
            { AppConstants.Authorization, string.Format("Bearer {0}", kostalLoginResponse.Data.loginUser.accessToken.token) }
        };
        this.restClient.UpdateToken(tokens);

        return new Result<KostalLoginResponse>(kostalLoginResponse);
    }
    public async Task<Result<SitesResponse>> GetSites()
    {
        //SITES
        string query = "query SITES_LIST_FIRST { sites(first: 1) { nodes { id name      inclination __typename    } __typename  } }";
        GraphQlRequest graphQlRequest = new GraphQlRequest { operationName = "SITES_LIST_FIRST", query = query, variables = new EmptyVariables() };
        var resultSites = await this.restClient.ExecutePostAsync<SitesResponse>(string.Empty, graphQlRequest);
        sitesResponse = resultSites.Model;
        return new Result<SitesResponse>(sitesResponse);
    }
    public async Task<Result<Device>> GetInverter(int siteNodeId)
    {
        //FETCH_LIST_DEVICE_IDS_OF_SITE
        var query = "query FETCH_LIST_DEVICE_IDS_OF_SITE($id: Long!) {  site(id: $id) {    id    name    currency    purchaseCompensation    feedInCompensation    dataSources {      id      name      status      metadata {        id        key        value        __typename      }      devices {        id        name        type        uniqueIdentifier        metadata {          id          key          value          __typename        }        __typename      }      mostRecentDataSourceLogEntry {        timestamp        __typename      }      __typename    }    __typename  }}";
        var graphQlRequest = new GraphQlRequest { operationName = "FETCH_LIST_DEVICE_IDS_OF_SITE", query = query, variables = new ListDeviceVariables { id = siteNodeId } };
        var resultDevice = await this.restClient.ExecutePostAsync<DeviceResponse>(string.Empty, graphQlRequest);
        deviceResponse = resultDevice.Model;
        var device = deviceResponse.data.site.dataSources.First().devices.FirstOrDefault(x => x.type == "INVERTER");
        return new Result<Device>(device);
    }

    public async Task<bool> SyncProductionOwnUseFirstTime(bool firstTime, IProgress<int> progress, int progressStartNr)
    {

        using var dbContext = new MscDbContext();
        var inverter = await dbContext.Inverter.FirstOrDefaultAsync(x => x.HomeId == MySolarCellsGlobals.SelectedHome.HomeId);
        //LOGIN 
        string query = "mutation LOGIN_USER_MUTATION($emailAddress: String!, $password: String!, $termsOfUseAccepted: Boolean, $marketingOptIn: Boolean) {  loginUser(user: {emailAddress: $emailAddress, password: $password, termsOfUseAccepted: $termsOfUseAccepted, marketingOptIn: $marketingOptIn}) {    accessToken {      token      tokenType      expiresIn      __typename    }    refreshToken    user {      ...userMenuFragment      __typename    }    __typename  }}fragment userMenuFragment on User {  id  emailAddress  customer {    id    __typename  }  contact {    id    firstName    lastName    address {      id      country      __typename    }    __typename  }  userRoles {    id    name    __typename  }  locale  __typename}";
        var variables = new LoginVariables { emailAddress = inverter.UserName, password = StringHelper.Decrypt(inverter.Password, AppConstants.Secretkey) };
        GraphQlRequest graphQlRequest = new GraphQlRequest { operationName = "LOGIN_USER_MUTATION", variables = variables, query = query };
        var login = await this.restClient.ExecutePostAsync<KostalLoginResponse>(string.Empty, graphQlRequest);
        kostalLoginResponse = login.Model;

        Dictionary<string, string> tokens = new Dictionary<string, string>
        {
            { AppConstants.Authorization, string.Format("Bearer {0}", kostalLoginResponse.Data.loginUser.accessToken.token) }
        };
        this.restClient.UpdateToken(tokens);

       
        try
        {

            
            int batch100 = 0;
            string processingDateFrom;
            string processingDateTo;
            int homeId = MySolarCellsGlobals.SelectedHome.HomeId;
            List<Sqlite.Models.Energy> eneryList = new List<Sqlite.Models.Energy>();
            DateTime start = inverter.FromDate;
            DateTime end = DateTime.Now;
            DateTime nextStart = new DateTime();

            var deviceIds = new List<int>();
            deviceIds.Add(Convert.ToInt32(inverter.SubSystemEntityId));
            var dataTypes = new List<string>();
            dataTypes.Add("ac_hourly_yield");
            DeviceProductionResponse dayProduction = new DeviceProductionResponse();

            while (start < end)
            {
                //Get 3 mounts per request
                if (start.AddMonths(3) < end)
                {
                    processingDateFrom = new DateTime(start.Year, start.Month, start.Day).ToString("yyyy-MM-dd");
                    processingDateTo = new DateTime(start.AddMonths(3).Year, start.AddMonths(3).Month, start.AddMonths(3).Day).ToString("yyyy-MM-dd");
                    nextStart = start.AddMonths(3);
                }
                else
                {
                    processingDateFrom = new DateTime(start.Year, start.Month, start.Day).ToString("yyyy-MM-dd");
                    processingDateTo = new DateTime(end.Year, end.Month, end.Day).ToString("yyyy-MM-dd");
                    nextStart = end;
                }

                
                var dataSelector = new DateSelector { from = processingDateFrom, to = processingDateTo, fromTimeOfDay = "00:00:00", toTimeOfDay = "23:59:59" };
                query = "query FETCH_DEVICE_DATA($deviceIds: [Long!]!, $dataTypes: [String!]!, $dateSelector: TimeSpanSelectorInput!, $padWithNull: Boolean = true, $byTimeInterval: AggregationInterval!, $byDevice: Boolean = true, $statistic: AggregationStatistic = SUM) {  deviceData(deviceIds: $deviceIds, dataTypes: $dataTypes, dateSelector: $dateSelector, padWithNull: $padWithNull) {    aggregate(byDevice: $byDevice, byTimeInterval: $byTimeInterval, statistic: $statistic) {      timeSeries {        points {          timestamp          value          __typename        }        __typename      }      __typename    }    __typename  }}";
                graphQlRequest = new GraphQlRequest { operationName = "FETCH_DEVICE_DATA", query = query, variables = new ProductionHourVariables { padWithNull = true, byDevice = true, statistic = "SUM", deviceIds = deviceIds, dataTypes = dataTypes, dateSelector = dataSelector, byTimeInterval = "HOUR" } };
                var resultDay = await this.restClient.ExecutePostAsync<DeviceProductionResponse>(string.Empty, graphQlRequest);
                dayProduction = resultDay.Model;
                List<Point> listPoints = new List<Point>();
                if (resultDay.WasSuccessful && resultDay.Model != null)
                {
                    listPoints = dayProduction.data.deviceData.aggregate.timeSeries.First().points;

                }
                else
                {
                    //TODO:Show error
                }
                var producedI = listPoints.Count;

                for (int i = 0; i < producedI; i++)
                {
                    progressStartNr++;
                    batch100++;
                    var energyExist = dbContext.Energy.FirstOrDefault(x => x.Timestamp == listPoints[i].timestamp);
                    if (energyExist != null)
                    {
                        if (listPoints[i].value.HasValue && listPoints[i].value.Value > 0)
                        { 
                            energyExist.ProductionOwnUse = (Convert.ToDouble(listPoints[i].value.Value) / 1000) - Convert.ToDouble(energyExist.ProductionSold);
                            energyExist.ProductionOwnUseProfit = energyExist.ProductionOwnUse * energyExist.UnitPriceBuy;
                        }
                        energyExist.InverterTypProductionOwnUse = (int)InverterTyp.Kostal;
                        energyExist.ProductionOwnUseSynced = true;
                        eneryList.Add(energyExist);

                    }

                    if (batch100 == 100)
                    {
                        await Task.Delay(100); //Så att GUI hinner uppdatera
                        progress.Report(progressStartNr);

                        batch100 = 0;
                        await dbContext.BulkUpdateAsync(eneryList);
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
                await dbContext.BulkUpdateAsync(eneryList);
                eneryList = new List<Sqlite.Models.Energy>();
            }
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }


        // old code

        //var dateMissingValue = dbContext.Energy.Where(x => x.InverterTypProductionOwnUse == (int)InverterTyp.Unknown);
        //string processingDateFrom;
        //string processingDateTo;
        //string currentProcessDate = string.Empty;
        //DeviceProductionResponse dayProduction = new DeviceProductionResponse();
        //foreach (var hourValue in dateMissingValue)
        //{
        //    //hämtar ut intervall
        //    processingDateFrom = new DateTime(hourValue.Timestamp.Year, hourValue.Timestamp.Month, hourValue.Timestamp.Day).ToString("yyyy-MM-dd");
        //    processingDateTo = new DateTime(hourValue.Timestamp.Year, hourValue.Timestamp.Month, hourValue.Timestamp.Day).AddDays(1).ToString("yyyy-MM-dd");
        //    //Kollar ifall vi ska hämta 24 timmar till
        //    if (currentProcessDate != processingDateFrom)
        //    {
        //        currentProcessDate = processingDateFrom;
        //        //get values from Kostal 
        //        //FETCH_DEVICE_DATA
        //        var deviceIds = new List<int>();
        //        deviceIds.Add(Convert.ToInt32(inverter.SubSystemEntityId));
        //        var dataTypes = new List<string>();
        //        dataTypes.Add("ac_hourly_yield");
        //        var dataSelector = new DateSelector { from = processingDateFrom, to = processingDateTo, fromTimeOfDay = "00:00:00", toTimeOfDay = "23:59:59" };
        //        query = "query FETCH_DEVICE_DATA($deviceIds: [Long!]!, $dataTypes: [String!]!, $dateSelector: TimeSpanSelectorInput!, $padWithNull: Boolean = true, $byTimeInterval: AggregationInterval!, $byDevice: Boolean = true, $statistic: AggregationStatistic = SUM) {  deviceData(deviceIds: $deviceIds, dataTypes: $dataTypes, dateSelector: $dateSelector, padWithNull: $padWithNull) {    aggregate(byDevice: $byDevice, byTimeInterval: $byTimeInterval, statistic: $statistic) {      timeSeries {        points {          timestamp          value          __typename        }        __typename      }      __typename    }    __typename  }}";
        //        graphQlRequest = new GraphQlRequest { operationName = "FETCH_DEVICE_DATA", query = query, variables = new ProductionHourVariables { padWithNull = true, byDevice = true, statistic = "SUM", deviceIds = deviceIds, dataTypes = dataTypes, dateSelector = dataSelector, byTimeInterval = "HOUR" } };
        //        var resultDay = await this.restClient.ExecutePostAsync<DeviceProductionResponse>(string.Empty, graphQlRequest);
        //        dayProduction = resultDay.Model;
        //    }
        //    var prod = dayProduction.data.deviceData.aggregate.timeSeries.First().points.FirstOrDefault(x => x.timestamp == hourValue.Timestamp);
        //    if (prod.value.HasValue && prod.value.Value > 0)
        //    {
        //        hourValue.ProductionOwnUse = (prod.value.Value / 1000) - hourValue.ProductionSold;
        //        hourValue.ProductionOwnUseProfit = hourValue.ProductionOwnUse * hourValue.UnitPriceBuy;

        //    }
        //    hourValue.InverterTypProductionOwnUse = (int)InverterTyp.Kostal;
        //    await dbContext.SaveChangesAsync();

        //}


      
    }
}
//Response
public class KostalLoginResponse
{
    public LoginData Data { get; set; }
}
public class AccessToken
{
    public string token { get; set; }
    public string tokenType { get; set; }
    public int expiresIn { get; set; }
    public string __typename { get; set; }
}

public class Contact
{
    public int id { get; set; }
    public string firstName { get; set; }
    public string lastName { get; set; }
    public object address { get; set; }
    public string __typename { get; set; }
}

public class Customer
{
    public int id { get; set; }
    public string __typename { get; set; }
}

public class LoginData
{
    public LoginUser loginUser { get; set; }
}

public class LoginUser
{
    public AccessToken accessToken { get; set; }
    public string refreshToken { get; set; }
    public User user { get; set; }
    public string __typename { get; set; }
}

public class User
{
    public int id { get; set; }
    public string emailAddress { get; set; }
    public Customer customer { get; set; }
    public Contact contact { get; set; }
    public List<UserRole> userRoles { get; set; }
    public string locale { get; set; }
    public string __typename { get; set; }
}

public class UserRole
{
    public int id { get; set; }
    public string name { get; set; }
    public string __typename { get; set; }
}
//SITES_LIST_FIRST
public class SitesResponse
{
    public SitesData data { get; set; }
}
public class SitesData
{
    public Sites sites { get; set; }
}

public class Node
{
    public int id { get; set; }
    public string name { get; set; }
    public object inclination { get; set; }
    public string __typename { get; set; }
}

public class Sites
{
    public List<Node> nodes { get; set; }
    public string __typename { get; set; }
}

//FETCH_LIST_DEVICE_IDS_OF_SITE
public class DeviceResponse
{
    public DeviceData data { get; set; }
}
public class DeviceData
{
    public DeviceSite site { get; set; }
}

public class DataSource
{
    public int id { get; set; }
    public string name { get; set; }
    public string status { get; set; }
    public List<Metadata> metadata { get; set; }
    public List<Device> devices { get; set; }
    public MostRecentDataSourceLogEntry mostRecentDataSourceLogEntry { get; set; }
    public string __typename { get; set; }
}

public class Device
{
    public int id { get; set; }
    public string name { get; set; }
    public string type { get; set; }
    public string uniqueIdentifier { get; set; }
    public List<Metadata> metadata { get; set; }
    public string __typename { get; set; }
}

public class Metadata
{
    public int id { get; set; }
    public string key { get; set; }
    public string value { get; set; }
    public string __typename { get; set; }
}

public class MostRecentDataSourceLogEntry
{
    public DateTime timestamp { get; set; }
    public string __typename { get; set; }
}



public class DeviceSite
{
    public int id { get; set; }
    public string name { get; set; }
    public object currency { get; set; }
    public object purchaseCompensation { get; set; }
    public object feedInCompensation { get; set; }
    public List<DataSource> dataSources { get; set; }
    public string __typename { get; set; }
}


//FETCH_DEVICE_DATA
public class DeviceProductionResponse
{
    public DeviceProductionData data { get; set; }
}
public class Aggregate
{
    public List<TimeSeries> timeSeries { get; set; }
    public string __typename { get; set; }
}

public class DeviceProductionData
{
    public DeviceProductionDeviceData deviceData { get; set; }
}

public class DeviceProductionDeviceData
{
    public Aggregate aggregate { get; set; }
    public string __typename { get; set; }
}

public class Point
{
    public DateTime timestamp { get; set; }
    public decimal? value { get; set; }
    public string __typename { get; set; }
}



public class TimeSeries
{
    public List<Point> points { get; set; }
    public string __typename { get; set; }
}






//Request
public class GraphQlRequest
{
    public string operationName { get; set; }
    public Object variables { get; set; }
    public string query { get; set; }
}

public class LoginVariables
{
    public string emailAddress { get; set; }
    public string password { get; set; }
    public object termsOfUseAccepted { get; set; }
    public object marketingOptIn { get; set; }
}
public class EmptyVariables
{

}
public class ListDeviceVariables
{
    public int id { get; set; }
}


public class ProductionHourVariables
{
    public bool padWithNull { get; set; }
    public bool byDevice { get; set; }
    public string statistic { get; set; }
    public List<int> deviceIds { get; set; }
    public List<string> dataTypes { get; set; }
    public DateSelector dateSelector { get; set; }
    public string byTimeInterval { get; set; }
}
public class DateSelector
{
    public string from { get; set; }
    public string to { get; set; }
    public string fromTimeOfDay { get; set; }
    public string toTimeOfDay { get; set; }
}






