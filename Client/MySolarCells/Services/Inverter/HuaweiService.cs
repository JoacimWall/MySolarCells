using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Text.Json;
using Acr.UserDialogs;
using Syncfusion.XlsIO.Parser.Biff_Records.MsoDrawing;

namespace MySolarCells.Services.Inverter;


public class HuaweiService : IInverterServiceInterface
{
    private IRestClient restClient;
    private InverterLoginResponse inverterLoginResponse;
    private HuaweiSiteResponse sitesResponse;
    private HuaweiDevicesResponse deviceResponse;

    public string InverterGuideText => "Guide text for the Huawei Inverter";
    public string DefaultApiUrl => "";
    public bool ShowUserName => true;

    public bool ShowPassword => true;

    public bool ShowApiUrl => false;

    public bool ShowApiKey => false;
    private readonly JsonSerializerOptions jsonOptions;

    public HuaweiService(IRestClient restClient)
    {
        this.restClient = restClient;
        Dictionary<string, string> defaultRequestHeaders = new Dictionary<string, string>();
        jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        this.restClient.ApiSettings = new ApiSettings { BaseUrl = "https://eu5.fusionsolar.huawei.com", defaultRequestHeaders = defaultRequestHeaders };
        this.restClient.ReInit();
    }
    public async Task<Result<InverterLoginResponse>> TestConnection(string userName, string password, string apiUrl, string apiKey)
    {
        //LOGIN
        var body = new HuaweiLoginRequest { userName = userName.Trim(), systemCode = password.Trim() };
        var loginResult = await this.restClient.ExecutePostReturnResponseHeadersAsync<HuaweiLoginResponse>("/thirdData/login", body);
        if (!loginResult.Item1.WasSuccessful || !loginResult.Item1.Model.success)
            return new Result<InverterLoginResponse>(loginResult.Item1.ErrorMessage == null ? loginResult.Item1.Model.message.ToString():loginResult.Item1.ErrorMessage);
        else
        {
            this.inverterLoginResponse = new InverterLoginResponse
            {
                token = loginResult.Item2.FirstOrDefault(x => x.Key=="xsrf-token").Value.First().ToString(),
                expiresIn = 0,
                tokenType = "",
            };

        }

        Dictionary<string, string> tokens = new Dictionary<string, string>
        {
            { "XSRF-TOKEN",  this.inverterLoginResponse.token}
        };
        this.restClient.UpdateToken(tokens);

        return new Result<InverterLoginResponse>(inverterLoginResponse);
    }
    public async Task<Result<List<InverterSite>>> GetPickerOne()
    {
        //SITES
        SiteListRequest body = new SiteListRequest { pageNo = 1, pageSize = 100 };
        var resultSites = await this.restClient.ExecutePostAsync<HuaweiSiteResponse>("/thirdData/stations", body);
        if (!resultSites.WasSuccessful || !resultSites.Model.success)
            return new Result<List<InverterSite>>(resultSites.ErrorMessage == null ? resultSites.Model.message.ToString() : resultSites.ErrorMessage);
        else
        {
            sitesResponse = resultSites.Model;
            var returnlist = new List<InverterSite>();
            foreach (var item in sitesResponse.data.list)
            {
                returnlist.Add(new InverterSite { Id = item.plantCode.ToString(), Name = item.plantName.ToString(), InstallationDate = Convert.ToDateTime(item.gridConnectionDate) });
            }
            return new Result<List<InverterSite>>(returnlist);
        }
    }
    public async Task<Result<GetInverterResponse>> GetInverter(InverterSite inverterSite)
    {
        HuaweiDevListRequest body = new HuaweiDevListRequest {  stationCodes = inverterSite.Id };
        var resultDevice = await this.restClient.ExecutePostAsync<HuaweiDevicesResponse>("/thirdData/getDevList", body);
        if (!resultDevice.WasSuccessful || !resultDevice.Model.success)
            return new Result<GetInverterResponse>(resultDevice.ErrorMessage == null ? resultDevice.Model.message.ToString() : resultDevice.ErrorMessage);
        else
        {
            deviceResponse = resultDevice.Model;
            var returnlist = new List<InverterSite>();
            inverterSite.InverterName = "unknown";
            foreach (var item in deviceResponse.data)
            {
                if (item.devTypeId == 1)
                    inverterSite.InverterName = item.invType;
            }
        }
        return new Result<GetInverterResponse>(new GetInverterResponse { InverterId = inverterSite.Id, Name = inverterSite.InverterName });
    }
    

    public async Task<bool> Sync(DateTime start, IProgress<int> progress, int progressStartNr)
    {

        using var dbContext = new MscDbContext();
        var inverter = await dbContext.Inverter.FirstOrDefaultAsync(x => x.HomeId == MySolarCellsGlobals.SelectedHome.HomeId);
        //LOGIN
        var loginResult = await TestConnection(inverter.UserName, StringHelper.Decrypt(inverter.Password, AppConstants.Secretkey), "", "");
        inverterLoginResponse = loginResult.Model;

        try
        {


            int batch100 = 0;
            
            int homeId = MySolarCellsGlobals.SelectedHome.HomeId;
            List<Sqlite.Models.Energy> eneryList = new List<Sqlite.Models.Energy>();
            var datehelp = DateHelper.GetRelatedDates(DateTime.Now);
            DateTime end = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            DateTime nextStart = new DateTime();

            HuaweiProductionResponse dayProduction = new HuaweiProductionResponse();

            while (start < end)
            {
                //Get 24 hours per request
                long datemillis = DateHelper.DateTimeToMillis(start);
                nextStart = start.AddDays(1);
                var body = new HuaweiProductioRequest { stationCodes = inverter.SubSystemEntityId, collectTime = datemillis };
                var resultDay = await this.restClient.ExecutePostAsync<HuaweiProductionResponse>("/thirdData/getKpiStationHour", body);
                if (!resultDay.WasSuccessful || !resultDay.Model.success)
                {
                    if (resultDay.Model.failCode == 407)
                        return true;

                    return false;
                   
                }
                dayProduction = resultDay.Model;
                List<HuaweiPoint> listPoints = new List<HuaweiPoint>();
                var dataList =  JsonSerializer.Deserialize<List<HuaweiProductionData>>(resultDay.Model.data, jsonOptions);
               
                foreach (var item in dataList)
                {
                    listPoints.Add(new HuaweiPoint { timestamp = DateHelper.MillisToDateTime(item.collectTime.Value), value = item.dataItemMap.inverter_power, __typename = "totalprod" });
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
                            energyExist.ProductionOwnUse = Convert.ToDouble(listPoints[i].value.Value - energyExist.ProductionSold);
                            //Only add if price over zero
                            if (energyExist.UnitPriceBuy > 0)
                                energyExist.ProductionOwnUseProfit = energyExist.ProductionOwnUse * energyExist.UnitPriceBuy;
                            else
                                energyExist.ProductionOwnUseProfit = 0;
                        }
                        energyExist.InverterTypProductionOwnUse = (int)InverterTyp.Huawei;
                        if (energyExist.Timestamp < end.AddHours(-1))
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


        return true;


      
    }
}
//Response
public class HuaweiLoginResponse
{
    public object data { get; set; }
    public bool success { get; set; }
    public int failCode { get; set; }
    public object message { get; set; }
}

public class HuaweiSiteData
{
    public List<HuaweiSiteList> list { get; set; }
    public int pageCount { get; set; }
    public int pageNo { get; set; }
    public int pageSize { get; set; }
    public int total { get; set; }
}

public class HuaweiSiteList
{
    public double capacity { get; set; }
    public string contactMethod { get; set; }
    public string contactPerson { get; set; }
    public DateTime gridConnectionDate { get; set; }
    public string latitude { get; set; }
    public string longitude { get; set; }
    public string plantAddress { get; set; }
    public string plantCode { get; set; }
    public string plantName { get; set; }
}

public class HuaweiSiteResponse
{
    public HuaweiSiteData data { get; set; }
    public int failCode { get; set; }
    public string message { get; set; }
    public bool success { get; set; }
}

public class HuaweiDevicesData
{
    public string devName { get; set; }
    public int devTypeId { get; set; }
    public string esnCode { get; set; }
    public object id { get; set; }
    public string invType { get; set; }
    public double latitude { get; set; }
    public double longitude { get; set; }
    public int? optimizerNumber { get; set; }
    public string softwareVersion { get; set; }
    public string stationCode { get; set; }
}

public class HuaweiDevicesParams
{
    public long currentTime { get; set; }
    public string stationCodes { get; set; }
}

public class HuaweiDevicesResponse
{
    public List<HuaweiDevicesData> data { get; set; }
    public int failCode { get; set; }
    public object message { get; set; }
    //public HuaweiDevicesParams params { get; set; }
    public bool success { get; set; }
}


// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
public class HuaweiProductioItemMap
{
    public double? radiation_intensity { get; set; }
    public double? inverter_power { get; set; }
    public double? power_profit { get; set; }
    public double? theory_power { get; set; }
    public double? ongrid_power { get; set; }
}

public class HuaweiProductionData
{
    public long? collectTime { get; set; }
    public string stationCode { get; set; }
    public HuaweiProductioItemMap dataItemMap { get; set; }
}

//public class Params
//{
//    public long currentTime { get; set; }
//    public long collectTime { get; set; }
//    public string stationCodes { get; set; }
//}

public class HuaweiProductionResponse
{
    //List<HuaweiProductionData>
    public string data { get; set; }
    public int failCode { get; set; }
    public object message { get; set; }
    //public Params @params { get; set; }
    public bool success { get; set; }
}

////Request
public class HuaweiLoginRequest
{
    public string userName { get; set; }
    public string systemCode { get; set; }

}
public class SiteListRequest
{
    public int pageNo { get; set; }
    public int pageSize { get; set; }

}
public class HuaweiDevListRequest
{
    public string stationCodes { get; set; }


}
public class HuaweiProductioRequest
{
    public string stationCodes { get; set; }
    public long collectTime { get; set; }

}


public class HuaweiPoint
{
    public DateTime timestamp { get; set; }
    public double? value { get; set; }
    public string __typename { get; set; }
}





