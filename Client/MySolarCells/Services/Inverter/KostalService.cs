// namespace MySolarCells.Services.Inverter;
//
// public class KostalService : IInverterServiceInterface
// {
//     private readonly IRestClient restClient;
//     private readonly MscDbContext mscDbContext;
//     private InverterLoginResponse inverterLoginResponse;
//     private KostalUserRoleSitesResponse sitesResponse;
//     private KostalUserRoleDeviceResponse deviceResponse;
//
//     public string InverterGuideText => AppResources.Kostal_Intro_Guide_Text;
//     public string DefaultApiUrl => "";
//     public bool ShowUserName => true;
//
//     public bool ShowPassword => true;
//
//     public bool ShowApiUrl => false;
//
//     public bool ShowApiKey => false;
//
//     public KostalService(IRestClient restClient, MscDbContext mscDbContext)
//     {
//         this.restClient = restClient;
//         this.mscDbContext = mscDbContext;
//         Dictionary<string, string> defaultRequestHeaders = new Dictionary<string, string>();
//         defaultRequestHeaders.Add("White-Label-Access-Key", "bf8c1e86-b23f-49f6-9156-a1e6a08895d7");
//
//         this.restClient.ApiSettings = new ApiSettings { BaseUrl = "https://kostal-api.solytic.com/api/graphql", DefaultRequestHeaders = defaultRequestHeaders };
//         this.restClient.ReInit();
//     }
//     public async Task<Result<InverterLoginResponse>> TestConnection(string userName, string password, string apiUrl, string apiKey)
//     {
//         //LOGIN 
//         string query = "mutation LOGIN_USER_MUTATION($emailAddress: String!, $password: String!, $termsOfUseAccepted: Boolean, $marketingOptIn: Boolean) {  loginUser(user: {emailAddress: $emailAddress, password: $password, termsOfUseAccepted: $termsOfUseAccepted, marketingOptIn: $marketingOptIn}) {    accessToken {      token      tokenType      expiresIn      __typename    }    refreshToken    user {      ...userMenuFragment      __typename    }    __typename  }}fragment userMenuFragment on User {  id  emailAddress  customer {    id    __typename  }  contact {    id    firstName    lastName    address {      id      country      __typename    }    __typename  }  userRoles {    id    name    __typename  }  locale  __typename}";
//         var variables = new KostalUserRoleLoginVariables { emailAddress = userName, password = password };
//         KostalUserRoleGraphQlRequest graphQlRequest = new KostalUserRoleGraphQlRequest { operationName = "LOGIN_USER_MUTATION", variables = variables, query = query };
//         var loginResult = await this.restClient.ExecutePostAsync<KostalLoginResponse>(string.Empty, graphQlRequest);
//         if (!loginResult.WasSuccessful)
//             return new Result<InverterLoginResponse>(loginResult.ErrorMessage);
//         else if (loginResult.Model == null || loginResult.Model.Data == null || loginResult.Model.Data.loginUser == null)
//             return new Result<InverterLoginResponse>(AppResources.The_Login_Failed_Check_That_You_Entered_The_Correct_Information);
//         else
//         {
//             this.inverterLoginResponse = new InverterLoginResponse
//             {
//                 token = loginResult.Model.Data.loginUser.accessToken.token,
//                 expiresIn = loginResult.Model.Data.loginUser.accessToken.expiresIn,
//                 tokenType = loginResult.Model.Data.loginUser.accessToken.tokenType,
//             };
//
//         }
//
//         Dictionary<string, string> tokens = new Dictionary<string, string>
//         {
//             { AppConstants.Authorization, string.Format("Bearer {0}", this.inverterLoginResponse.token) }
//         };
//         this.restClient.UpdateToken(tokens);
//
//         return new Result<InverterLoginResponse>(inverterLoginResponse);
//     }
//     public async Task<Result<List<InverterSite>>> GetPickerOne()
//     {
//         //SITES
//         string query = "query SITES_LIST_FIRST { sites(first: 1) { nodes { id name   installationDate   inclination __typename    } __typename  } }";
//         KostalUserRoleGraphQlRequest graphQlRequest = new KostalUserRoleGraphQlRequest { operationName = "SITES_LIST_FIRST", query = query, variables = new KostalUserRoleEmptyVariables() };
//         var resultSites = await this.restClient.ExecutePostAsync<KostalUserRoleSitesResponse>(string.Empty, graphQlRequest);
//         sitesResponse = resultSites.Model;
//         var returnlist = new List<InverterSite>();
//         foreach (var item in sitesResponse.data.sites.nodes)
//         {
//             returnlist.Add(new InverterSite { Id = item.id.ToString(), Name = item.name.ToString(), InstallationDate = Convert.ToDateTime(item.installationDate) });
//         }
//         return new Result<List<InverterSite>>(returnlist);
//     }
//     public async Task<Result<GetInverterResponse>> GetInverter(InverterSite inverterSite)
//     {
//         //FETCH_LIST_DEVICE_IDS_OF_SITE
//         var query = "query FETCH_LIST_DEVICE_IDS_OF_SITE($id: Long!) {  site(id: $id) {    id    name  installationDate  currency    purchaseCompensation    feedInCompensation    dataSources {      id      name      status      metadata {        id        key        value        __typename      }      devices {        id        name        type        uniqueIdentifier        metadata {          id          key          value          __typename        }        __typename      }      mostRecentDataSourceLogEntry {        timestamp        __typename      }      __typename    }    __typename  }}";
//         var graphQlRequest = new KostalUserRoleGraphQlRequest { operationName = "FETCH_LIST_DEVICE_IDS_OF_SITE", query = query, variables = new KostalUserRoleListDeviceVariables { id = Convert.ToInt32(inverterSite.Id) } };
//         var resultDevice = await this.restClient.ExecutePostAsync<KostalUserRoleDeviceResponse>(string.Empty, graphQlRequest);
//         deviceResponse = resultDevice.Model;
//         var device = deviceResponse.data.site.dataSources.First().devices.FirstOrDefault(x => x.type == "INVERTER");
//         return new Result<GetInverterResponse>(new GetInverterResponse { InverterId = device.id.ToString(), Name = device.name });
//     }
//
//     public async Task<Result<DataSyncResponse>> Sync(DateTime start, IProgress<int> progress, int progressStartNr)
//     {
//
//        
//         var inverter = await this.mscDbContext.Inverter.OrderByDescending(s => s.FromDate).FirstOrDefaultAsync(x => x.HomeId == MySolarCellsGlobals.SelectedHome.HomeId);
//         //LOGIN
//         var loginResult = await TestConnection(inverter.UserName, StringHelper.Decrypt(inverter.Password, AppConstants.Secretkey), "", "");
//         if (!loginResult.WasSuccessful)
//             return new Result<DataSyncResponse>(loginResult.ErrorMessage);
//         inverterLoginResponse = loginResult.Model;
//
//         try
//         {
//
//
//             int batch100 = 0;
//             string processingDateFrom;
//             string processingDateTo;
//             int homeId = MySolarCellsGlobals.SelectedHome.HomeId;
//             List<SQLite.Sqlite.Models.Energy> eneryList = new List<SQLite.Sqlite.Models.Energy>();
//             DateTime end = DateTime.Now;
//             DateTime nextStart = new DateTime();
//
//             var deviceIds = new List<int>();
//             deviceIds.Add(Convert.ToInt32(inverter.SubSystemEntityId));
//             var dataTypes = new List<string>();
//             dataTypes.Add("ac_hourly_yield");
//             KostalUserRoleDeviceProductionResponse dayProduction = new KostalUserRoleDeviceProductionResponse();
//
//             while (start < end)
//             {
//                 //Get 3 mounts per request
//                 if (start.AddMonths(3) < end)
//                 {
//                     processingDateFrom = new DateTime(start.Year, start.Month, start.Day).ToString("yyyy-MM-dd");
//                     processingDateTo = new DateTime(start.AddMonths(3).Year, start.AddMonths(3).Month, start.AddMonths(3).Day).ToString("yyyy-MM-dd");
//                     nextStart = start.AddMonths(3);
//                 }
//                 else
//                 {
//                     processingDateFrom = new DateTime(start.Year, start.Month, start.Day).ToString("yyyy-MM-dd");
//                     processingDateTo = new DateTime(end.Year, end.Month, end.Day).ToString("yyyy-MM-dd");
//                     nextStart = end;
//                 }
//
//
//                 var dataSelector = new KostalUserRoleDateSelector { from = processingDateFrom, to = processingDateTo, fromTimeOfDay = "00:00:00", toTimeOfDay = "23:59:59" };
//                 var query = "query FETCH_DEVICE_DATA($deviceIds: [Long!]!, $dataTypes: [String!]!, $dateSelector: TimeSpanSelectorInput!, $padWithNull: Boolean = true, $byTimeInterval: AggregationInterval!, $byDevice: Boolean = true, $statistic: AggregationStatistic = SUM) {  deviceData(deviceIds: $deviceIds, dataTypes: $dataTypes, dateSelector: $dateSelector, padWithNull: $padWithNull) {    aggregate(byDevice: $byDevice, byTimeInterval: $byTimeInterval, statistic: $statistic) {      timeSeries {        points {          timestamp          value          __typename        }        __typename      }      __typename    }    __typename  }}";
//                 var graphQlRequest = new KostalUserRoleGraphQlRequest { operationName = "FETCH_DEVICE_DATA", query = query, variables = new KostalUserRoleProductionHourVariables { padWithNull = true, byDevice = true, statistic = "SUM", deviceIds = deviceIds, dataTypes = dataTypes, dateSelector = dataSelector, byTimeInterval = "HOUR" } };
//                 var resultDay = await this.restClient.ExecutePostAsync<KostalUserRoleDeviceProductionResponse>(string.Empty, graphQlRequest);
//                 dayProduction = resultDay.Model;
//                 List<KostalUserRolePoint> listPoints = new List<KostalUserRolePoint>();
//                 if (resultDay.WasSuccessful && resultDay.Model != null)
//                 {
//                     listPoints = dayProduction.data.deviceData.aggregate.timeSeries.First().points;
//
//                 }
//                 else
//                 {
//                     //TODO:Show error
//                 }
//                 var producedI = listPoints.Count;
//
//                 for (int i = 0; i < producedI; i++)
//                 {
//                     progressStartNr++;
//                     batch100++;
//                     var energyExist = this.mscDbContext.Energy.FirstOrDefault(x => x.Timestamp == listPoints[i].timestamp);
//                     if (energyExist != null)
//                     {
//                         if (listPoints[i].value.HasValue && listPoints[i].value.Value > 0)
//                         {
//                             energyExist.ProductionOwnUse = (Convert.ToDouble(listPoints[i].value.Value) / 1000) - Convert.ToDouble(energyExist.ProductionSold);
//                             //Only add if price over zero
//                             if (energyExist.UnitPriceBuy > 0)
//                                 energyExist.ProductionOwnUseProfit = energyExist.ProductionOwnUse * energyExist.UnitPriceBuy;
//                             else
//                                 energyExist.ProductionOwnUseProfit = 0;
//                         }
//                         energyExist.InverterTypProductionOwnUse = (int)InverterTyp.Kostal;
//                         if (energyExist.Timestamp < end.AddHours(-2))
//                             energyExist.ProductionOwnUseSynced = true;
//
//                         eneryList.Add(energyExist);
//
//                     }
//
//                     if (batch100 == 100)
//                     {
//                         await Task.Delay(100); //Så att GUI hinner uppdatera
//                         progress.Report(progressStartNr);
//
//                         batch100 = 0;
//                         await this.mscDbContext.BulkUpdateAsync(eneryList);
//                         eneryList = new List<SQLite.Sqlite.Models.Energy>();
//                     }
//
//                 }
//
//                 start = nextStart;
//             }
//
//             if (eneryList.Count > 0)
//             {
//                 await Task.Delay(100); //Så att GUI hinner uppdatera
//                 progress.Report(progressStartNr);
//
//                 batch100 = 0;
//                 await this.mscDbContext.BulkUpdateAsync(eneryList);
//                 eneryList = new List<SQLite.Sqlite.Models.Energy>();
//             }
//             return new Result<DataSyncResponse>(new DataSyncResponse
//             {
//                 SyncState = DataSyncState.ProductionSync,
//                 Message = AppResources.Import_Of_Production_Done
//             }, true);
//         }
//         catch (Exception ex)
//         {
//             return new Result<DataSyncResponse>(ex.Message);
//         }
//
//
//         
//     }
// }
// //Response
// public class KostalLoginResponse
// {
//     public KostalLoginData Data { get; set; }
// }
// public class KostalAccessToken
// {
//     public string token { get; set; }
//     public string tokenType { get; set; }
//     public int expiresIn { get; set; }
//     public string __typename { get; set; }
// }
//
// public class KostalContact
// {
//     public int id { get; set; }
//     public string firstName { get; set; }
//     public string lastName { get; set; }
//     public object address { get; set; }
//     public string __typename { get; set; }
// }
//
// public class KostalCustomer
// {
//     public int id { get; set; }
//     public string __typename { get; set; }
// }
//
// public class KostalLoginData
// {
//     public KostalLoginUser loginUser { get; set; }
// }
//
// public class KostalLoginUser
// {
//     public KostalAccessToken accessToken { get; set; }
//     public string refreshToken { get; set; }
//     public KostalUser user { get; set; }
//     public string __typename { get; set; }
// }
//
// public class KostalUser
// {
//     public int id { get; set; }
//     public string emailAddress { get; set; }
//     public KostalCustomer customer { get; set; }
//     public KostalContact contact { get; set; }
//     public List<KostalUserRole> userRoles { get; set; }
//     public string locale { get; set; }
//     public string __typename { get; set; }
// }
//
// public class KostalUserRole
// {
//     public int id { get; set; }
//     public string name { get; set; }
//     public string __typename { get; set; }
// }
// //SITES_LIST_FIRST
// public class KostalUserRoleSitesResponse
// {
//     public KostalUserRoleSitesData data { get; set; }
// }
// public class KostalUserRoleSitesData
// {
//     public KostalUserRoleSites sites { get; set; }
// }
//
// public class KostalUserRoleNode
// {
//     public int id { get; set; }
//     public string name { get; set; }
//     public object inclination { get; set; }
//     public string installationDate { get; set; }
//     public string __typename { get; set; }
// }
//
// public class KostalUserRoleSites
// {
//     public List<KostalUserRoleNode> nodes { get; set; }
//     public string __typename { get; set; }
// }
//
// //FETCH_LIST_DEVICE_IDS_OF_SITE
// public class KostalUserRoleDeviceResponse
// {
//     public KostalUserRoleDeviceData data { get; set; }
// }
// public class KostalUserRoleDeviceData
// {
//     public KostalUserRoleDeviceSite site { get; set; }
// }
//
// public class KostalUserRoleDataSource
// {
//     public int id { get; set; }
//     public string name { get; set; }
//     public string status { get; set; }
//     public List<KostalUserRoleMetadata> metadata { get; set; }
//     public List<KostalUserRoleDevice> devices { get; set; }
//     public KostalUserRoleMostRecentDataSourceLogEntry mostRecentDataSourceLogEntry { get; set; }
//     public string __typename { get; set; }
// }
//
// public class KostalUserRoleDevice
// {
//     public int id { get; set; }
//     public string name { get; set; }
//     public string type { get; set; }
//     public string uniqueIdentifier { get; set; }
//     public List<KostalUserRoleMetadata> metadata { get; set; }
//     public string __typename { get; set; }
// }
//
// public class KostalUserRoleMetadata
// {
//     public int id { get; set; }
//     public string key { get; set; }
//     public string value { get; set; }
//     public string __typename { get; set; }
// }
//
// public class KostalUserRoleMostRecentDataSourceLogEntry
// {
//     public DateTime timestamp { get; set; }
//     public string __typename { get; set; }
// }
//
//
//
// public class KostalUserRoleDeviceSite
// {
//     public int id { get; set; }
//     public string name { get; set; }
//     public string installationDate { get; set; }
//     public object currency { get; set; }
//     public object purchaseCompensation { get; set; }
//     public object feedInCompensation { get; set; }
//     public List<KostalUserRoleDataSource> dataSources { get; set; }
//     public string __typename { get; set; }
// }
//
//
// //FETCH_DEVICE_DATA
// public class KostalUserRoleDeviceProductionResponse
// {
//     public KostalUserRoleDeviceProductionData data { get; set; }
// }
// public class KostalUserRoleAggregate
// {
//     public List<KostalUserRoleTimeSeries> timeSeries { get; set; }
//     public string __typename { get; set; }
// }
//
// public class KostalUserRoleDeviceProductionData
// {
//     public KostalUserRoleDeviceProductionDeviceData deviceData { get; set; }
// }
//
// public class KostalUserRoleDeviceProductionDeviceData
// {
//     public KostalUserRoleAggregate aggregate { get; set; }
//     public string __typename { get; set; }
// }
//
// public class KostalUserRolePoint
// {
//     public DateTime timestamp { get; set; }
//     public decimal? value { get; set; }
//     public string __typename { get; set; }
// }
//
//
//
// public class KostalUserRoleTimeSeries
// {
//     public List<KostalUserRolePoint> points { get; set; }
//     public string __typename { get; set; }
// }
//
//
//
//
//
//
// //Request
// public class KostalUserRoleGraphQlRequest
// {
//     public string operationName { get; set; }
//     public Object variables { get; set; }
//     public string query { get; set; }
// }
//
// public class KostalUserRoleLoginVariables
// {
//     public string emailAddress { get; set; }
//     public string password { get; set; }
//     public object termsOfUseAccepted { get; set; }
//     public object marketingOptIn { get; set; }
// }
// public class KostalUserRoleEmptyVariables
// {
//
// }
// public class KostalUserRoleListDeviceVariables
// {
//     public int id { get; set; }
// }
//
//
// public class KostalUserRoleProductionHourVariables
// {
//     public bool padWithNull { get; set; }
//     public bool byDevice { get; set; }
//     public string statistic { get; set; }
//     public List<int> deviceIds { get; set; }
//     public List<string> dataTypes { get; set; }
//     public KostalUserRoleDateSelector dateSelector { get; set; }
//     public string byTimeInterval { get; set; }
// }
// public class KostalUserRoleDateSelector
// {
//     public string from { get; set; }
//     public string to { get; set; }
//     public string fromTimeOfDay { get; set; }
//     public string toTimeOfDay { get; set; }
// }
//
//
//
//
//
//
