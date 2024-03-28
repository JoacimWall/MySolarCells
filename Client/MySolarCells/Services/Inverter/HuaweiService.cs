using System.Collections.Generic;
using System.Formats.Asn1;
using System.IO;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Acr.UserDialogs;
using MySolarCellsSQLite.Sqlite;
using MySolarCellsSQLite.Sqlite.Models;
using SQLitePCL;
using Syncfusion.XlsIO.FormatParser.FormatTokens;
using Syncfusion.XlsIO.Parser.Biff_Records.MsoDrawing;

namespace MySolarCells.Services.Inverter;


public class HuaweiService : IInverterServiceInterface
{
    private readonly IRestClient restClient;
    private readonly MscDbContext mscDbContext;
    private InverterLoginResponse? inverterLoginResponse;
    private HuaweiSiteResponse? sitesResponse;
    private HuaweiDevicesResponse? deviceResponse;

    public string InverterGuideText => "Guide text for the Huawei Inverter";
    public string DefaultApiUrl => "";
    public bool ShowUserName => true;

    public bool ShowPassword => true;

    public bool ShowApiUrl => false;

    public bool ShowApiKey => false;
    private readonly JsonSerializerOptions jsonOptions;

    public HuaweiService(IRestClient restClient, MscDbContext mscDbContext)
    {
        this.restClient = restClient;
        this.mscDbContext = mscDbContext;
        Dictionary<string, string> defaultRequestHeaders = new Dictionary<string, string>();
        jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        this.restClient.ApiSettings = new ApiSettings { BaseUrl = "https://eu5.fusionsolar.huawei.com", DefaultRequestHeaders = defaultRequestHeaders };
        this.restClient.ReInit();
    }
    public async Task<Result<InverterLoginResponse>> TestConnection(string userName, string password, string apiUrl, string apiKey)
    {
        //LOGIN
        var body = new HuaweiLoginRequest { userName = userName.Trim(), systemCode = password.Trim() };
        var loginResult = await restClient.ExecutePostReturnResponseHeadersAsync<HuaweiLoginResponse>("/thirdData/login", body);
        var resultItem1 = loginResult.Item1;
        var resultItem2 = loginResult.Item2;
        if (resultItem1.Model != null && (!resultItem1.WasSuccessful || !resultItem1.Model.success))
            return new Result<InverterLoginResponse>(loginResult.Item1.ErrorMessage);
        else if (resultItem2 != null)
        {
            inverterLoginResponse = new InverterLoginResponse
            {
                token = resultItem2.FirstOrDefault(x => x.Key == "xsrf-token").Value.First().ToString(),
                expiresIn = 0,
                tokenType = "",
            };

        }
        if (inverterLoginResponse != null)
        {
            Dictionary<string, string> tokens = new Dictionary<string, string>
            {
                { "XSRF-TOKEN",  inverterLoginResponse.token}
            };
            restClient.UpdateToken(tokens);
        }
        else
        {
            return new Result<InverterLoginResponse>("Error in setting token");
        }
        return new Result<InverterLoginResponse>(inverterLoginResponse);
    }
    public async Task<Result<List<InverterSite>>> GetPickerOne()
    {
        //SITES
        SiteListRequest body = new SiteListRequest { pageNo = 1, pageSize = 100 };
        var resultSites = await restClient.ExecutePostAsync<HuaweiSiteResponse>("/thirdData/stations", body);
           
        if (resultSites is { WasSuccessful: false, Model: null })
            return new Result<List<InverterSite>>(resultSites.ErrorMessage );
        else if (resultSites is { WasSuccessful: false, Model: not null })
        {
            return new Result<List<InverterSite>>(resultSites.Model.message.ToString()!);
        }
        else if (resultSites is { WasSuccessful: true, Model: not null })
        {
            sitesResponse = resultSites.Model;
            var returnList = new List<InverterSite>();
            foreach (var item in sitesResponse.data.list)
            {
                returnList.Add(new InverterSite { Id = item.plantCode.ToString(), Name = item.plantName.ToString(), InstallationDate = Convert.ToDateTime(item.gridConnectionDate) });
            }
            return new Result<List<InverterSite>>(returnList);
        }

        return new Result<List<InverterSite>>("Error getting pickerOne");
    }
    public async Task<Result<GetInverterResponse>> GetInverter(InverterSite inverterSite)
    {
        HuaweiDevListRequest body = new HuaweiDevListRequest { stationCodes = inverterSite.Id };
        var resultDevice = await restClient.ExecutePostAsync<HuaweiDevicesResponse>("/thirdData/getDevList", body);
        
        if (resultDevice is { WasSuccessful: false, Model: null })
        {
            return new Result<GetInverterResponse>(resultDevice.ErrorMessage );  
        }  
        if (resultDevice is { WasSuccessful: false, Model: not null })
        {
            return new Result<GetInverterResponse>(resultDevice.Model.message.ToString()!);
        }

        if (resultDevice is { WasSuccessful: true, Model: not null })
        {
            deviceResponse = resultDevice.Model;
            inverterSite.InverterName = "unknown";
            foreach (var item in deviceResponse.data)
            {
                if (item.devTypeId == 1)
                    inverterSite.InverterName = item.invType;
            }
        }
        return new Result<GetInverterResponse>(new GetInverterResponse { InverterId = inverterSite.Id, Name = inverterSite.InverterName });
    }


    public async Task<Result<DataSyncResponse>> Sync(DateTime start, IProgress<int> progress, int progressStartNr)
    {

        
        var inverter = await mscDbContext.Inverter.OrderByDescending(s => s.FromDate).FirstAsync(x => x.HomeId == MySolarCellsGlobals.SelectedHome.HomeId);
        //LOGIN
        var loginResult = await TestConnection(inverter.UserName!, StringHelper.Decrypt(inverter.Password!, AppConstants.Secretkey), "", "");
        inverterLoginResponse = loginResult.Model;
        var returValue = new Result<DataSyncResponse>(new DataSyncResponse { SyncState = DataSyncState.ProductionSync, Message = AppResources.Import_Of_Production_Done}, true);

        try
        {
            int batch100 = 0;
            int homeId = MySolarCellsGlobals.SelectedHome.HomeId;
            List<Energy> eneryList = new List<Energy>();
            var datehelp = DateHelper.GetRelatedDates(DateTime.Now);
            DateTime end = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            DateTime nextStart = new DateTime();
            start = new DateTime(start.Year, start.Month, start.Day);
            HuaweiProductionResponse dayProduction = new HuaweiProductionResponse();
            int countApiRequests = 0;
            while (start < end)
            {
                //Get 24 hours per request
                long datemillis = DateHelper.DateTimeToMillis(start);
                nextStart = start.AddDays(1);
                var body = new HuaweiProductioRequest { stationCodes = inverter.SubSystemEntityId, collectTime = datemillis };
                var resultDay = await restClient.ExecutePostAsync<HuaweiProductionResponse>("/thirdData/getKpiStationHour", body);


                //Result<HuaweiProductionResponse> resultDay = new Result<HuaweiProductionResponse>("sdasd");
                if (resultDay is { WasSuccessful: false, Model: null })
                {
                    returValue= new Result<DataSyncResponse>(resultDay.ErrorMessage );  
                }  
                if (!resultDay.WasSuccessful || resultDay.Model is { success: false })
                {
                    if (resultDay.Model is { failCode: 407 } )
                    {
                        MySolarCellsGlobals.ConsoleWriteLineDebug("Antal Huawei " + countApiRequests.ToString());
                        returValue= new Result<DataSyncResponse>(new DataSyncResponse
                        {
                            SyncState = DataSyncState.ProductionSync,
                            Message = string.Format(AppResources.Importing_Data_From_Your_Inverter_Ended_As_It_Only_Allows_And_More, "24", countApiRequests.ToString(),start.ToShortDateString())
                            
                        });
                        break;
                    }
                    else 
                    {//error
                        returValue=  new Result<DataSyncResponse>(new DataSyncResponse
                        {
                            SyncState = DataSyncState.ProductionSync,
                            Message =  resultDay.Model!.message.ToString()!
                        }, false);
                    }
                    break;
                }
                else if (resultDay.Model != null)
                {
                    countApiRequests ++;


                    dayProduction = resultDay.Model;
                    List<HuaweiPoint> listPoints = new List<HuaweiPoint>();
                    //var dataList = JsonSerializer.Deserialize<List<HuaweiProductionData>>(resultDay.Model.data, jsonOptions);

                    foreach (var item in dayProduction.data)
                    {
                        if (item.collectTime != null)
                            listPoints.Add(new HuaweiPoint
                            {
                                timestamp = DateHelper.MillisToDateTime(item.collectTime.Value),
                                value = item.dataItemMap.inverter_power, __typename = "totalprod"
                            });
                    }

                    var producedI = listPoints.Count;

                    for (int i = 0; i < producedI; i++)
                    {
                        progressStartNr++;
                        batch100++;
                        var energyExist = mscDbContext.Energy.FirstOrDefault(x => x.Timestamp == listPoints[i].timestamp);
                        if (energyExist != null)
                        {
                            HuaweiPoint value = listPoints[i];
                            if ( value.value > 0)
                            {
                                if (value.value - energyExist.ProductionSold < 0)
                                    MySolarCellsGlobals.ConsoleWriteLineDebug("minus prod");

                                energyExist.ProductionOwnUse = Convert.ToDouble(value.value - energyExist.ProductionSold);
                                //Only add if price over zero
                                if (energyExist.UnitPriceBuy > 0)
                                    energyExist.ProductionOwnUseProfit = energyExist.ProductionOwnUse * energyExist.UnitPriceBuy;
                                else
                                    energyExist.ProductionOwnUseProfit = 0;
                            }
                            energyExist.InverterTypProductionOwnUse = (int)InverterTyp.Huawei;
                            if (energyExist.Timestamp < end.AddHours(-2))
                                energyExist.ProductionOwnUseSynced = true;

                            eneryList.Add(energyExist);

                        }

                        if (batch100 == 100)
                        {
                            await Task.Delay(100); //Så att GUI hinner uppdatera
                            progress.Report(progressStartNr);

                            batch100 = 0;
                            await mscDbContext.BulkUpdateAsync(eneryList);
                            eneryList = new List<Energy>();
                        }

                    }

                    start = nextStart;
                }
            }

            if (eneryList.Count > 0)
            {
                await Task.Delay(100); //Så att GUI hinner uppdatera
                progress.Report(progressStartNr);

                batch100 = 0;
                await mscDbContext.BulkUpdateAsync(eneryList);
                eneryList = new List<Energy>();
            }
            
        }
        catch (Exception ex)
        {
            return new Result<DataSyncResponse>(ex.Message);
        }
        return returValue;
    }
}
//Response
public class HuaweiLoginResponse
{
    public object data { get; set; } = new();
    public bool success { get; set; }
    public int failCode { get; set; }
    public object message { get; set; }= new();
}

public class HuaweiSiteData
{
    public List<HuaweiSiteList> list { get; set; }= new();
    public int pageCount { get; set; }
    public int pageNo { get; set; }
    public int pageSize { get; set; }
    public int total { get; set; }
}

public class HuaweiSiteList
{
    public double capacity { get; set; }
    public string contactMethod { get; set; } = "";
    public string contactPerson { get; set; }= "";
    public DateTime gridConnectionDate { get; set; }
    public string latitude { get; set; }= "";
    public string longitude { get; set; }= "";
    public string plantAddress { get; set; }= "";
    public string plantCode { get; set; }= "";
    public string plantName { get; set; }= "";
}

public class HuaweiSiteResponse
{
    public HuaweiSiteData data { get; set; }= new();
    public int failCode { get; set; }
    public string message { get; set; }= "";
    public bool success { get; set; }
}

public class HuaweiDevicesData
{
    public string devName { get; set; }= "";
    public int devTypeId { get; set; }
    public string esnCode { get; set; }= "";
    public object id { get; set; }= new();
    public string invType { get; set; }= "";
    public double latitude { get; set; }
    public double longitude { get; set; }
    public int? optimizerNumber { get; set; }
    public string softwareVersion { get; set; }= "";
    public string stationCode { get; set; }= "";
}

public class HuaweiDevicesParams
{
    public long currentTime { get; set; }
    public string stationCodes { get; set; }= "";
}

public class HuaweiDevicesResponse
{
    public List<HuaweiDevicesData> data { get; set; }= new();
    public int failCode { get; set; }
    public object message { get; set; }= new();
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
    public string stationCode { get; set; }= "";
    public HuaweiProductioItemMap dataItemMap { get; set; }= new();
}

//public class Params
//{
//    public long currentTime { get; set; }
//    public long collectTime { get; set; }
//    public string stationCodes { get; set; }
//}

public class HuaweiProductionResponse
{
    [JsonPropertyName("data")]
    [JsonConverter(typeof(SingleOrArrayConverter<HuaweiProductionData>))]
    public List<HuaweiProductionData> data { get; set; }= new();
    public int failCode { get; set; }
    public object message { get; set; }= new();
    //public Params @params { get; set; }
    public bool success { get; set; }
}

////Request
public class HuaweiLoginRequest
{
    public string userName { get; set; }= "";
    public string systemCode { get; set; }= "";

}
public class SiteListRequest
{
    public int pageNo { get; set; }
    public int pageSize { get; set; }

}
public class HuaweiDevListRequest
{
    public string stationCodes { get; set; }= "";


}
public class HuaweiProductioRequest
{
    public string stationCodes { get; set; }= "";
    public long collectTime { get; set; }

}


public class HuaweiPoint
{
    public DateTime timestamp { get; set; }
    public double? value { get; set; }
    public string __typename { get; set; }= "";
}


public class SingleOrArrayConverter<TItem> : SingleOrArrayConverter<List<TItem>, TItem>
{
    public SingleOrArrayConverter() : this(true) { }
    public SingleOrArrayConverter(bool canWrite) : base(canWrite) { }
}

public class SingleOrArrayConverterFactory : JsonConverterFactory
{
    public bool CanWrite { get; }

    public SingleOrArrayConverterFactory() : this(true) { }

    public SingleOrArrayConverterFactory(bool canWrite) => CanWrite = canWrite;

    public override bool CanConvert(Type? typeToConvert)
    {
        var itemType = GetItemType(typeToConvert);
        if (itemType == null)
            return false;
        if (itemType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(itemType))
            return false;
        if (typeToConvert != null && (typeToConvert.GetConstructor(Type.EmptyTypes) == null || typeToConvert.IsValueType))
            return false;
        return true;
    }

    public override JsonConverter? CreateConverter(Type? typeToConvert, JsonSerializerOptions options)
    {
        var itemType = GetItemType(typeToConvert);
        if (itemType != null)
        {
            if (typeToConvert != null)
            {
                var converterType = typeof(SingleOrArrayConverter<,>).MakeGenericType(typeToConvert, itemType);
                return (JsonConverter)Activator.CreateInstance(converterType, new object[] { CanWrite })!;
            }
        }

        return null;
    }

    static Type? GetItemType(Type? type)
    {
        // Quick reject for performance
        if (type != null && (type.IsPrimitive || type.IsArray || type == typeof(string)))
            return null;
        while (type != null)
        {
            if (type.IsGenericType)
            {
                var genType = type.GetGenericTypeDefinition();
                if (genType == typeof(List<>))
                    return type.GetGenericArguments()[0];
                // Add here other generic collection types as required, e.g. HashSet<> or ObservableCollection<> or etc.
            }
            type = type.BaseType;
        }
        return null;
    }
}

public class SingleOrArrayConverter<TCollection, TItem> : JsonConverter<TCollection> where TCollection : class, ICollection<TItem>, new()
{
    public SingleOrArrayConverter() : this(true) { }
    public SingleOrArrayConverter(bool canWrite) => CanWrite = canWrite;

    public bool CanWrite { get; }

    public override TCollection? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                return null;
            case JsonTokenType.StartArray:
                var list = new TCollection();
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndArray)
                        break;
                    list.Add(JsonSerializer.Deserialize<TItem>(ref reader, options)!);
                }
                return list;
            default:
                return null; //new TCollection { JsonSerializer.Deserialize<TItem>(ref reader, options) };
        }
    }

    public override void Write(Utf8JsonWriter writer, TCollection value, JsonSerializerOptions options)
    {
        if (CanWrite && value.Count == 1)
        {
            JsonSerializer.Serialize(writer, value.First(), options);
        }
        else
        {
            writer.WriteStartArray();
            foreach (var item in value)
                JsonSerializer.Serialize(writer, item, options);
            writer.WriteEndArray();
        }
    }
}
//public class SingleOrArrayConverter<T> : JsonConverter<string>
//{
//    //public override bool CanConvert(Type objectType)
//    //{
//    //    return (objectType == typeof(List<T>));
//    //}

//    //public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
//    //{
//    //    JToken token = JToken.Load(reader);
//    //    if (token.Type == JTokenType.Array)
//    //    {
//    //        return token.ToObject<List<T>>();
//    //    }
//    //    if (token.Type == JTokenType.Null)
//    //    {
//    //        return null;
//    //    }
//    //    return new List<T> { token.ToObject<T>() };
//    //}
//    //public override bool CanWrite
//    //{
//    //    get { return false; }
//    //}

//    //public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
//    //{
//    //    throw new NotImplementedException();
//    //}

//    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
//    {
//        JToken token = JToken.Load(reader);
//        if (token.Type == JTokenType.Array)
//        {
//            return token.ToObject<List<T>>();
//        }
//        if (token.Type == JTokenType.Null)
//        {
//            return null;
//        }
//        return new List<T> { token.ToObject<T>() };
//    }

//    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
//    {
//        throw new NotImplementedException();
//    }
//}



