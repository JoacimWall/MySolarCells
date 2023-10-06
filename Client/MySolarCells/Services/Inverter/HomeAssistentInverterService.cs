using Syncfusion.XlsIO.Parser.Biff_Records.MsoDrawing;
using System.IO;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

namespace MySolarCells.Services.Inverter;

public class HomeAssistentInverterService : IInverterServiceInterface
{
    private HttpClient restClient;
    private InverterLoginResponse inverterLoginResponse;
    private readonly JsonSerializerOptions jsonOptions;

    public string InverterGuideText => "Rest API is on the same aurl as the HA web frontend." + Environment.NewLine +
                    " https://IP_ADDRESS:8123/api/" + Environment.NewLine + "You obtain a token under your profile in HA.";
    public string DefaultApiUrl => "https://wallhome.duckdns.org:8123/api/";
    public bool ShowUserName => false;

    public bool ShowPassword => false;

    public bool ShowApiUrl => true;

    public bool ShowApiKey => true;
    private HttpClient GetHttpClient(string apiUrl, string apiKey)
    {
        //Vi använder detta för att den vanliga rest client med ReadAsStreamAsync verkar inte vilja fungera med HA
        HttpClient testc = new HttpClient();


        testc.BaseAddress = new Uri(apiUrl);
        //testc.DefaultRequestHeaders.Add(AppConstants.Authorization, "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiIyNzZjN2E3ZmY5OWU0MGI2OGNhZDE1NmM2YTgzMDI4OCIsImlhdCI6MTY5NjM5NjI1NiwiZXhwIjoyMDExNzU2MjU2fQ.o_JFjiPNdPyJl0YtBmY5fKJO_A6ms3Gs40jDfZG9ofY" ); //string.Format("Bearer {0}", apiKey)
        testc.DefaultRequestHeaders.Add(AppConstants.Authorization, string.Format("Bearer {0}", apiKey)); //

        testc.DefaultRequestHeaders.Add("User-Accept", "*/*");
        //testc.DefaultRequestHeaders.Add("Host", "wallhome.duckdns.org:8123");
        testc.DefaultRequestHeaders.Add("User-Agent", "PostmanRuntime/7.33.0");
        testc.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
        testc.DefaultRequestHeaders.Add("Connection", "keep-alive");
        return testc;
    }
    public HomeAssistentInverterService()
    {
        
        jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

    }
    public async Task<Result<InverterLoginResponse>> TestConnection(string userName, string password, string apiUrl, string apiKey)
    {
        this.restClient = GetHttpClient(apiUrl, apiKey);
        var r = await this.restClient.GetAsync("");
        string content = await r.Content.ReadAsStringAsync();
        var result = new Result<HaApiStatus>(JsonSerializer.Deserialize<HaApiStatus>(content, jsonOptions));

        //Test APi Connection 
        
        if (!result.WasSuccessful)
            return new Result<InverterLoginResponse>(result.ErrorMessage);
        else
        {
             this.inverterLoginResponse = new InverterLoginResponse
            {
                token = apiKey,
                expiresIn = 0,
                tokenType = "",
            };

        }


        return new Result<InverterLoginResponse>(inverterLoginResponse);
    }
    public async Task<Result<List<InverterSite>>> GetPickerOne()
    {
        
        var r = await this.restClient.GetAsync("states");
        string contnet = await r.Content.ReadAsStringAsync();
        var result = new Result<List<HaStates>>(JsonSerializer.Deserialize<List<HaStates>>(contnet, jsonOptions));
        if (!result.WasSuccessful)
            return new Result<List<InverterSite>>(result.ErrorMessage);
        else
        {
           
            var returnlist = new List<InverterSite>();
            int fakeId = 1;
            foreach (var item in result.Model)
            {
                returnlist.Add(new InverterSite { Id = fakeId.ToString(), Name = item.entity_id.ToString() });
                fakeId++;
            }
            return new Result<List<InverterSite>>(returnlist);

        }
    }
    public async Task<Result<GetInverterResponse>> GetInverter(InverterSite inverterSite)
    {
        //we return the same this for the general flow should work
        return new Result<GetInverterResponse>( new GetInverterResponse { InverterId = inverterSite.Name, Name = inverterSite.Name });
    }

    public async Task<bool> SyncProductionOwnUse(DateTime start, IProgress<int> progress, int progressStartNr)
    {

        using var dbContext = new MscDbContext();
        var inverter = await dbContext.Inverter.FirstOrDefaultAsync(x => x.HomeId == MySolarCellsGlobals.SelectedHome.HomeId);
        //LOGIN
        var loginResult = await TestConnection(inverter.UserName, StringHelper.Decrypt(inverter.Password, AppConstants.Secretkey), "", "");
        inverterLoginResponse = loginResult.Model;

        try
        {


            int batch100 = 0;
            string processingDateFrom;
            string processingDateTo;
            string toTimeOfDay = "T23:59:59";
            string fromTimeOfDay = "T00:00:00";
            int homeId = MySolarCellsGlobals.SelectedHome.HomeId;
            List<Sqlite.Models.Energy> eneryList = new List<Sqlite.Models.Energy>();
            DateTime end = DateTime.Now;
            DateTime nextStart = new DateTime();

            var deviceIds = new List<int>();
            deviceIds.Add(Convert.ToInt32(inverter.SubSystemEntityId));
            var dataTypes = new List<string>();
            dataTypes.Add("ac_hourly_yield");
            //HAaUserRoleDeviceProductionResponse dayProduction = new HAaUserRoleDeviceProductionResponse();

            while (start < end)
            {
                //Get 3 mounts per request
                if (start.AddMonths(1) < end)
                {
                    processingDateFrom = new DateTime(start.Year, start.Month, start.Day).ToString("yyyy-MM-dd") + fromTimeOfDay;
                    processingDateTo = new DateTime(start.AddMonths(1).Year, start.AddMonths(3).Month, start.AddMonths(3).Day).ToString("yyyy-MM-dd") + toTimeOfDay;
                    nextStart = start.AddMonths(1);
                }
                else
                {
                    processingDateFrom = new DateTime(start.Year, start.Month, start.Day).ToString("yyyy-MM-dd") + fromTimeOfDay;
                    processingDateTo = new DateTime(end.Year, end.Month, end.Day).ToString("yyyy-MM-dd") + toTimeOfDay;
                    nextStart = end;
                }


                //var dataSelector = new HAaUserRoleDateSelector { from = processingDateFrom, to = processingDateTo, fromTimeOfDay = "00:00:00", toTimeOfDay = "23:59:59" };
                //var query = "query FETCH_DEVICE_DATA($deviceIds: [Long!]!, $dataTypes: [String!]!, $dateSelector: TimeSpanSelectorInput!, $padWithNull: Boolean = true, $byTimeInterval: AggregationInterval!, $byDevice: Boolean = true, $statistic: AggregationStatistic = SUM) {  deviceData(deviceIds: $deviceIds, dataTypes: $dataTypes, dateSelector: $dateSelector, padWithNull: $padWithNull) {    aggregate(byDevice: $byDevice, byTimeInterval: $byTimeInterval, statistic: $statistic) {      timeSeries {        points {          timestamp          value          __typename        }        __typename      }      __typename    }    __typename  }}";
                //var graphQlRequest = new HAaUserRoleGraphQlRequest { operationName = "FETCH_DEVICE_DATA", query = query, variables = new HAaUserRoleProductionHourVariables { padWithNull = true, byDevice = true, statistic = "SUM", deviceIds = deviceIds, dataTypes = dataTypes, dateSelector = dataSelector, byTimeInterval = "HOUR" } };
                //var resultDay = await this.restClient.ExecutePostAsync<HAaUserRoleDeviceProductionResponse>(string.Empty, graphQlRequest);
                string quary = string.Format("history/period/{0}?minimal_response=&filter_entity_id={1}&end_time={2}", processingDateFrom, inverter.SubSystemEntityId, processingDateTo);
                var r = await this.restClient.GetAsync(quary);
                string contnet = await r.Content.ReadAsStringAsync();
                var result = new Result<List<HaStates>>(JsonSerializer.Deserialize<List<HaStates>>(contnet, jsonOptions));
                //ska värden per timma.

                //dayProduction = resultDay.Model;
                //List<HAaUserRolePoint> listPoints = new List<HAaUserRolePoint>();
                //if (resultDay.WasSuccessful && resultDay.Model != null)
                //{
                //    listPoints = dayProduction.data.deviceData.aggregate.timeSeries.First().points;

                //}
                //else
                //{
                //    //TODO:Show error
                //}
                //var producedI = listPoints.Count;

                //for (int i = 0; i < producedI; i++)
                //{
                //    progressStartNr++;
                //    batch100++;
                //    var energyExist = dbContext.Energy.FirstOrDefault(x => x.Timestamp == listPoints[i].timestamp);
                //    if (energyExist != null)
                //    {
                //        if (listPoints[i].value.HasValue && listPoints[i].value.Value > 0)
                //        {
                //            energyExist.ProductionOwnUse = (Convert.ToDouble(listPoints[i].value.Value) / 1000) - Convert.ToDouble(energyExist.ProductionSold);
                //            //Only add if price over zero
                //            if (energyExist.UnitPriceBuy > 0)
                //                energyExist.ProductionOwnUseProfit = energyExist.ProductionOwnUse * energyExist.UnitPriceBuy;
                //            else
                //                energyExist.ProductionOwnUseProfit = 0;
                //        }
                //        energyExist.InverterTypProductionOwnUse = (int)InverterTyp.HomeAssistent;
                //        energyExist.ProductionOwnUseSynced = true;
                //        eneryList.Add(energyExist);

                //    }

                //    if (batch100 == 100)
                //    {
                //        await Task.Delay(100); //Så att GUI hinner uppdatera
                //        progress.Report(progressStartNr);

                //        batch100 = 0;
                //        await dbContext.BulkUpdateAsync(eneryList);
                //        eneryList = new List<Sqlite.Models.Energy>();
                //    }

                //}

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


        return false;


      
    }
}
//Response
public class HaApiStatus
{
    public string message { get; set; }
}

public class HaStates
{
    public string entity_id { get; set; }
    
}





//Request







