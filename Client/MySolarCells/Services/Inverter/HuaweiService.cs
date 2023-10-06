namespace MySolarCells.Services.Inverter;


public class HuaweiService : IInverterServiceInterface
{
    private IRestClient restClient;
    private InverterLoginResponse inverterLoginResponse;
    private HuaweiUserRoleSitesResponse sitesResponse;
    private HuaweiUserRoleDeviceResponse deviceResponse;
    
    public string InverterGuideText => "Guide text for the Huawei Inverter";
    public string DefaultApiUrl => "";
    public bool ShowUserName => true;

    public bool ShowPassword => true;

    public bool ShowApiUrl => false;

    public bool ShowApiKey => false;

    public HuaweiService(IRestClient restClient)
    {
        this.restClient = restClient;
        Dictionary<string, string> defaultRequestHeaders = new Dictionary<string, string>();
        defaultRequestHeaders.Add("White-Label-Access-Key", "bf8c1e86-b23f-49f6-9156-a1e6a08895d7");

        this.restClient.ApiSettings = new ApiSettings { BaseUrl = "https://Huawei-api.solytic.com/api/graphql", defaultRequestHeaders = defaultRequestHeaders };
        this.restClient.ReInit();
    }
    public async Task<Result<InverterLoginResponse>> TestConnection(string userName, string password, string apiUrl, string apiKey)
    {
        //LOGIN 
        string query = "mutation LOGIN_USER_MUTATION($emailAddress: String!, $password: String!, $termsOfUseAccepted: Boolean, $marketingOptIn: Boolean) {  loginUser(user: {emailAddress: $emailAddress, password: $password, termsOfUseAccepted: $termsOfUseAccepted, marketingOptIn: $marketingOptIn}) {    accessToken {      token      tokenType      expiresIn      __typename    }    refreshToken    user {      ...userMenuFragment      __typename    }    __typename  }}fragment userMenuFragment on User {  id  emailAddress  customer {    id    __typename  }  contact {    id    firstName    lastName    address {      id      country      __typename    }    __typename  }  userRoles {    id    name    __typename  }  locale  __typename}";
        var variables = new HuaweiUserRoleLoginVariables { emailAddress = userName, password = password };
        HuaweiUserRoleGraphQlRequest graphQlRequest = new HuaweiUserRoleGraphQlRequest { operationName = "LOGIN_USER_MUTATION", variables = variables, query = query };
        var loginResult = await this.restClient.ExecutePostAsync<HuaweiLoginResponse>(string.Empty, graphQlRequest);
        if (!loginResult.WasSuccessful)
            return new Result<InverterLoginResponse>(loginResult.ErrorMessage);
        else
        {
             this.inverterLoginResponse = new InverterLoginResponse
            {
                token = loginResult.Model.Data.loginUser.accessToken.token,
                expiresIn = loginResult.Model.Data.loginUser.accessToken.expiresIn,
                tokenType = loginResult.Model.Data.loginUser.accessToken.tokenType,
            };

        }

        Dictionary<string, string> tokens = new Dictionary<string, string>
        {
            { AppConstants.Authorization, string.Format("Bearer {0}", this.inverterLoginResponse.token) }
        };
        this.restClient.UpdateToken(tokens);

        return new Result<InverterLoginResponse>(inverterLoginResponse);
    }
    public async Task<Result<List<InverterSite>>> GetPickerOne()
    {
        //SITES
        string query = "query SITES_LIST_FIRST { sites(first: 1) { nodes { id name      inclination __typename    } __typename  } }";
        HuaweiUserRoleGraphQlRequest graphQlRequest = new HuaweiUserRoleGraphQlRequest { operationName = "SITES_LIST_FIRST", query = query, variables = new HuaweiUserRoleEmptyVariables() };
        var resultSites = await this.restClient.ExecutePostAsync<HuaweiUserRoleSitesResponse>(string.Empty, graphQlRequest);
        sitesResponse = resultSites.Model;
        var returnlist = new List<InverterSite>();
        foreach (var item in sitesResponse.data.sites.nodes)
        {
            returnlist.Add(new InverterSite { Id = item.id.ToString(), Name = item.name.ToString() });
        }
        return new Result<List<InverterSite>>(returnlist);
    }
    public async Task<Result<GetInverterResponse>> GetInverter(InverterSite inverterSite)
    {
        //FETCH_LIST_DEVICE_IDS_OF_SITE
        var query = "query FETCH_LIST_DEVICE_IDS_OF_SITE($id: Long!) {  site(id: $id) {    id    name    currency    purchaseCompensation    feedInCompensation    dataSources {      id      name      status      metadata {        id        key        value        __typename      }      devices {        id        name        type        uniqueIdentifier        metadata {          id          key          value          __typename        }        __typename      }      mostRecentDataSourceLogEntry {        timestamp        __typename      }      __typename    }    __typename  }}";
        var graphQlRequest = new HuaweiUserRoleGraphQlRequest { operationName = "FETCH_LIST_DEVICE_IDS_OF_SITE", query = query, variables = new HuaweiUserRoleListDeviceVariables { id = Convert.ToInt32(inverterSite.Id) } };
        var resultDevice = await this.restClient.ExecutePostAsync<HuaweiUserRoleDeviceResponse>(string.Empty, graphQlRequest);
        deviceResponse = resultDevice.Model;
        var device = deviceResponse.data.site.dataSources.First().devices.FirstOrDefault(x => x.type == "INVERTER");
        return new Result<GetInverterResponse>( new GetInverterResponse { InverterId= device.id.ToString(), Name = device.name });
    }

    public async Task<bool> SyncProductionOwnUse(DateTime start, IProgress<int> progress, int progressStartNr)
    {

        using var dbContext = new MscDbContext();
        var inverter = await dbContext.Inverter.FirstOrDefaultAsync(x => x.HomeId == MySolarCellsGlobals.SelectedHome.HomeId);
        //LOGIN
        var loginResult = await TestConnection(inverter.UserName, StringHelper.Decrypt(inverter.Password, AppConstants.Secretkey),"","");
         inverterLoginResponse = loginResult.Model;


       
        try
        {

            
            int batch100 = 0;
            string processingDateFrom;
            string processingDateTo;
            int homeId = MySolarCellsGlobals.SelectedHome.HomeId;
            List<Sqlite.Models.Energy> eneryList = new List<Sqlite.Models.Energy>();
           
            DateTime end = DateTime.Now;
            DateTime nextStart = new DateTime();

            var deviceIds = new List<int>();
            deviceIds.Add(Convert.ToInt32(inverter.SubSystemEntityId));
            var dataTypes = new List<string>();
            dataTypes.Add("ac_hourly_yield");
            HuaweiUserRoleDeviceProductionResponse dayProduction = new HuaweiUserRoleDeviceProductionResponse();

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

                
                var dataSelector = new HuaweiUserRoleDateSelector { from = processingDateFrom, to = processingDateTo, fromTimeOfDay = "00:00:00", toTimeOfDay = "23:59:59" };
                var query = "query FETCH_DEVICE_DATA($deviceIds: [Long!]!, $dataTypes: [String!]!, $dateSelector: TimeSpanSelectorInput!, $padWithNull: Boolean = true, $byTimeInterval: AggregationInterval!, $byDevice: Boolean = true, $statistic: AggregationStatistic = SUM) {  deviceData(deviceIds: $deviceIds, dataTypes: $dataTypes, dateSelector: $dateSelector, padWithNull: $padWithNull) {    aggregate(byDevice: $byDevice, byTimeInterval: $byTimeInterval, statistic: $statistic) {      timeSeries {        points {          timestamp          value          __typename        }        __typename      }      __typename    }    __typename  }}";
                 var graphQlRequest = new HuaweiUserRoleGraphQlRequest { operationName = "FETCH_DEVICE_DATA", query = query, variables = new HuaweiUserRoleProductionHourVariables { padWithNull = true, byDevice = true, statistic = "SUM", deviceIds = deviceIds, dataTypes = dataTypes, dateSelector = dataSelector, byTimeInterval = "HOUR" } };
                var resultDay = await this.restClient.ExecutePostAsync<HuaweiUserRoleDeviceProductionResponse>(string.Empty, graphQlRequest);
                dayProduction = resultDay.Model;
                List<HuaweiUserRolePoint> listPoints = new List<HuaweiUserRolePoint>();
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
                            //Only add if price over zero
                            if (energyExist.UnitPriceBuy > 0)
                                energyExist.ProductionOwnUseProfit = energyExist.ProductionOwnUse * energyExist.UnitPriceBuy;
                            else
                                energyExist.ProductionOwnUseProfit = 0;
                        }
                        energyExist.InverterTypProductionOwnUse = (int)InverterTyp.Huawei;
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
        //        //get values from Huawei 
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
        //    hourValue.InverterTypProductionOwnUse = (int)InverterTyp.Huawei;
        //    await dbContext.SaveChangesAsync();

        //}


      
    }
}
//Response
public class HuaweiLoginResponse
{
    public HuaweiLoginData Data { get; set; }
}
public class HuaweiAccessToken
{
    public string token { get; set; }
    public string tokenType { get; set; }
    public int expiresIn { get; set; }
    public string __typename { get; set; }
}

public class HuaweiContact
{
    public int id { get; set; }
    public string firstName { get; set; }
    public string lastName { get; set; }
    public object address { get; set; }
    public string __typename { get; set; }
}

public class HuaweiCustomer
{
    public int id { get; set; }
    public string __typename { get; set; }
}

public class HuaweiLoginData
{
    public HuaweiLoginUser loginUser { get; set; }
}

public class HuaweiLoginUser
{
    public HuaweiAccessToken accessToken { get; set; }
    public string refreshToken { get; set; }
    public HuaweiUser user { get; set; }
    public string __typename { get; set; }
}

public class HuaweiUser
{
    public int id { get; set; }
    public string emailAddress { get; set; }
    public HuaweiCustomer customer { get; set; }
    public HuaweiContact contact { get; set; }
    public List<HuaweiUserRole> userRoles { get; set; }
    public string locale { get; set; }
    public string __typename { get; set; }
}

public class HuaweiUserRole
{
    public int id { get; set; }
    public string name { get; set; }
    public string __typename { get; set; }
}
//SITES_LIST_FIRST
public class HuaweiUserRoleSitesResponse
{
    public HuaweiUserRoleSitesData data { get; set; }
}
public class HuaweiUserRoleSitesData
{
    public HuaweiUserRoleSites sites { get; set; }
}

public class HuaweiUserRoleNode
{
    public int id { get; set; }
    public string name { get; set; }
    public object inclination { get; set; }
    public string __typename { get; set; }
}

public class HuaweiUserRoleSites
{
    public List<HuaweiUserRoleNode> nodes { get; set; }
    public string __typename { get; set; }
}

//FETCH_LIST_DEVICE_IDS_OF_SITE
public class HuaweiUserRoleDeviceResponse
{
    public HuaweiUserRoleDeviceData data { get; set; }
}
public class HuaweiUserRoleDeviceData
{
    public HuaweiUserRoleDeviceSite site { get; set; }
}

public class HuaweiUserRoleDataSource
{
    public int id { get; set; }
    public string name { get; set; }
    public string status { get; set; }
    public List<HuaweiUserRoleMetadata> metadata { get; set; }
    public List<HuaweiUserRoleDevice> devices { get; set; }
    public HuaweiUserRoleMostRecentDataSourceLogEntry mostRecentDataSourceLogEntry { get; set; }
    public string __typename { get; set; }
}

public class HuaweiUserRoleDevice
{
    public int id { get; set; }
    public string name { get; set; }
    public string type { get; set; }
    public string uniqueIdentifier { get; set; }
    public List<HuaweiUserRoleMetadata> metadata { get; set; }
    public string __typename { get; set; }
}

public class HuaweiUserRoleMetadata
{
    public int id { get; set; }
    public string key { get; set; }
    public string value { get; set; }
    public string __typename { get; set; }
}

public class HuaweiUserRoleMostRecentDataSourceLogEntry
{
    public DateTime timestamp { get; set; }
    public string __typename { get; set; }
}



public class HuaweiUserRoleDeviceSite
{
    public int id { get; set; }
    public string name { get; set; }
    public object currency { get; set; }
    public object purchaseCompensation { get; set; }
    public object feedInCompensation { get; set; }
    public List<HuaweiUserRoleDataSource> dataSources { get; set; }
    public string __typename { get; set; }
}


//FETCH_DEVICE_DATA
public class HuaweiUserRoleDeviceProductionResponse
{
    public HuaweiUserRoleDeviceProductionData data { get; set; }
}
public class HuaweiUserRoleAggregate
{
    public List<HuaweiUserRoleTimeSeries> timeSeries { get; set; }
    public string __typename { get; set; }
}

public class HuaweiUserRoleDeviceProductionData
{
    public HuaweiUserRoleDeviceProductionDeviceData deviceData { get; set; }
}

public class HuaweiUserRoleDeviceProductionDeviceData
{
    public HuaweiUserRoleAggregate aggregate { get; set; }
    public string __typename { get; set; }
}

public class HuaweiUserRolePoint
{
    public DateTime timestamp { get; set; }
    public decimal? value { get; set; }
    public string __typename { get; set; }
}



public class HuaweiUserRoleTimeSeries
{
    public List<HuaweiUserRolePoint> points { get; set; }
    public string __typename { get; set; }
}






//Request
public class HuaweiUserRoleGraphQlRequest
{
    public string operationName { get; set; }
    public Object variables { get; set; }
    public string query { get; set; }
}

public class HuaweiUserRoleLoginVariables
{
    public string emailAddress { get; set; }
    public string password { get; set; }
    public object termsOfUseAccepted { get; set; }
    public object marketingOptIn { get; set; }
}
public class HuaweiUserRoleEmptyVariables
{

}
public class HuaweiUserRoleListDeviceVariables
{
    public int id { get; set; }
}


public class HuaweiUserRoleProductionHourVariables
{
    public bool padWithNull { get; set; }
    public bool byDevice { get; set; }
    public string statistic { get; set; }
    public List<int> deviceIds { get; set; }
    public List<string> dataTypes { get; set; }
    public HuaweiUserRoleDateSelector dateSelector { get; set; }
    public string byTimeInterval { get; set; }
}
public class HuaweiUserRoleDateSelector
{
    public string from { get; set; }
    public string to { get; set; }
    public string fromTimeOfDay { get; set; }
    public string toTimeOfDay { get; set; }
}






