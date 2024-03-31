using System.Text.Json;
using System.Text.Json.Serialization;
using MySolarCells.Services.Inverter.Models;

namespace MySolarCells.Services.Inverter;
public class HuaweiService : IInverterServiceInterface
{
    private readonly IRestClient restClient;
    private readonly MscDbContext mscDbContext;
    private readonly IHomeService homeService;
    private readonly ILogService logService;
    private InverterLoginResponse? inverterLoginResponse;
    private HuaweiSiteResponse? sitesResponse;
    private HuaweiDevicesResponse? deviceResponse;

    public string InverterGuideText => "Guide text for the Huawei Inverter";
    public string DefaultApiUrl => "";
    public bool ShowUserName => true;

    public bool ShowPassword => true;

    public bool ShowApiUrl => false;

    public bool ShowApiKey => false;

    public HuaweiService(IRestClient restClient, MscDbContext mscDbContext,IHomeService homeService,ILogService logService)
    {
        this.restClient = restClient;
        this.mscDbContext = mscDbContext;
        this.homeService = homeService;
        this.logService = logService;
        var defaultRequestHeaders = new Dictionary<string, string>();
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
        var body = new SiteListRequest { pageNo = 1, pageSize = 100 };
        var resultSites = await restClient.ExecutePostAsync<HuaweiSiteResponse>("/thirdData/stations", body);
           
        if (resultSites is { WasSuccessful: false, Model: null })
            return new Result<List<InverterSite>>(resultSites.ErrorMessage );
        if (resultSites is { WasSuccessful: false, Model: not null })
            return new Result<List<InverterSite>>(resultSites.Model.message!);
        if (resultSites is not { WasSuccessful: true, Model: not null })
            return new Result<List<InverterSite>>("Error getting pickerOne");
        
        sitesResponse = resultSites.Model;
        var returnList = new List<InverterSite>();
        foreach (var item in sitesResponse.data.list)
            returnList.Add(new InverterSite { Id = item.plantCode, Name = item.plantName, InstallationDate = Convert.ToDateTime(item.gridConnectionDate) });
        
        return new Result<List<InverterSite>>(returnList);

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

        
        var inverter = await mscDbContext.Inverter.OrderByDescending(s => s.FromDate).FirstAsync(x => x.HomeId == homeService.CurrentHome().HomeId);
        //LOGIN
        var loginResult = await TestConnection(inverter.UserName!, inverter.Password!.Decrypt(AppConstants.Secretkey), "", "");
        inverterLoginResponse = loginResult.Model;
        var returnValue = new Result<DataSyncResponse>(new DataSyncResponse { SyncState = DataSyncState.ProductionSync, Message = AppResources.Import_Of_Production_Done});

        try
        {
            int batch100 = 0;
            var energyList = new List<Energy>();
            DateHelper.GetRelatedDates(DateTime.Now);
            var end = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            start = new DateTime(start.Year, start.Month, start.Day);
            int countApiRequests = 0;
            while (start < end)
            {
                //Get 24 hours per request
                long datemillis = DateHelper.DateTimeToMillis(start);
                var nextStart = start.AddDays(1);
                var body = new HuaweiProductioRequest { stationCodes = inverter.SubSystemEntityId, collectTime = datemillis };
                var resultDay = await restClient.ExecutePostAsync<HuaweiProductionResponse>("/thirdData/getKpiStationHour", body);


                //Result<HuaweiProductionResponse> resultDay = new Result<HuaweiProductionResponse>("sdasd");
                if (resultDay is { WasSuccessful: false, Model: null })
                {
                    returnValue= new Result<DataSyncResponse>(resultDay.ErrorMessage );  
                }  
                if (!resultDay.WasSuccessful || resultDay.Model is { success: false })
                {
                    if (resultDay.Model is { failCode: 407 } )
                    {
                        logService.ConsoleWriteLineDebug("Antal Huawei " + countApiRequests);
                        returnValue= new Result<DataSyncResponse>(new DataSyncResponse
                        {
                            SyncState = DataSyncState.ProductionSync,
                            Message = string.Format(AppResources.Importing_Data_From_Your_Inverter_Ended_As_It_Only_Allows_And_More, "24", countApiRequests.ToString(),start.ToShortDateString())
                            
                        });
                    }
                    else 
                    {//error
                        returnValue=  new Result<DataSyncResponse>(new DataSyncResponse
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
                    var dayProduction = resultDay.Model;
                    List<HuaweiPoint> listPoints = new List<HuaweiPoint>();
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
                                    logService.ConsoleWriteLineDebug("minus prod");

                                energyExist.ProductionOwnUse = Convert.ToDouble(value.value - energyExist.ProductionSold);
                                //Only add if price over zero
                                if (energyExist.UnitPriceBuy > 0)
                                    energyExist.ProductionOwnUseProfit = energyExist.ProductionOwnUse * energyExist.UnitPriceBuy;
                                else
                                    energyExist.ProductionOwnUseProfit = 0;
                            }
                            energyExist.InverterTypeProductionOwnUse = (int)InverterTypeEnum.Huawei;
                            if (energyExist.Timestamp < end.AddHours(-2))
                                energyExist.ProductionOwnUseSynced = true;

                            energyList.Add(energyExist);

                        }

                        if (batch100 == 100)
                        {
                            await Task.Delay(100); //Så att GUI hinner uppdatera
                            progress.Report(progressStartNr);

                            batch100 = 0;
                            await mscDbContext.BulkUpdateAsync(energyList);
                            energyList = new List<Energy>();
                        }

                    }

                    start = nextStart;
                }
            }

            if (energyList.Count > 0)
            {
                await Task.Delay(100); //So that the GUI has time to update
                progress.Report(progressStartNr);
                await mscDbContext.BulkUpdateAsync(energyList);
            }
        }
        catch (Exception ex)
        {
            return new Result<DataSyncResponse>(ex.Message);
        }
        return returnValue;
    }
}
public class SingleOrArrayConverter<TItem> : SingleOrArrayConverter<List<TItem>, TItem>
{
    public SingleOrArrayConverter() : this(true) { }
    private SingleOrArrayConverter(bool canWrite) : base(canWrite) { }
}

public class SingleOrArrayConverterFactory : JsonConverterFactory
{
    private bool CanWrite { get; }

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
    protected SingleOrArrayConverter(bool canWrite) => CanWrite = canWrite;

    private bool CanWrite { get; }

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




