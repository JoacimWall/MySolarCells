using SQLitePCL;
using Syncfusion.Maui.Charts;
using Syncfusion.XlsIO.Parser.Biff_Records.MsoDrawing;
using System;
using System.ComponentModel;
using System.IO;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using MySolarCellsSQLite.Sqlite;
using MySolarCellsSQLite.Sqlite.Models;
using static System.Net.Mime.MediaTypeNames;

namespace MySolarCells.Services.Inverter;

public class HomeAssistentInverterService : IInverterServiceInterface
{

    //private HttpClient restClient;
    private readonly MscDbContext mscDbContext;
    private InverterLoginResponse? inverterLoginResponse;
    //private readonly JsonSerializerOptions jsonOptions;

    public string InverterGuideText => "WebSockets API is on the same aurl as the HA web frontend." + Environment.NewLine +
                    "ws or wss://IP_ADDRESS:8123/api/websocket" + Environment.NewLine + "You obtain a token under your profile in HA.";
    public string DefaultApiUrl => "wss://yourserver.duckdns.org/api/websocket";
    public bool ShowUserName => false;

    public bool ShowPassword => false;

    public bool ShowApiUrl => true;

    public bool ShowApiKey => true;

    public HomeAssistentInverterService(MscDbContext mscDbContext)
    {
        this.mscDbContext = mscDbContext;
        /*jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };*/

        client = new ClientWebSocket();
        cts = new CancellationTokenSource();
        apiKey = "";
        apiUrl = "";
        pickerOneReturnlist = new();
        lastEnergyResult = new();

    }
    #region WebSocket
    string apiKey;
    string apiUrl;
    string reciveMode="";
    ClientWebSocket client;
    CancellationTokenSource cts;

    public bool IsConnected => client.State == WebSocketState.Open;

    private bool connected;
    async Task ConnectToServerAsync(string reciveMode, string userName, string password, string apiUrl, string apiKey)
    {
        this.apiKey = apiKey;
        this.apiUrl = apiUrl;
        this.reciveMode = reciveMode;
        if (IsConnected)
        {
            await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "", cts.Token);
            client = new ClientWebSocket();
            cts = new CancellationTokenSource();
        }

        await client.ConnectAsync(new Uri(apiUrl), cts.Token);
        await Task.Factory.StartNew(async () =>
        {
            while (connected == false)
            {
                await ReadMessage();
            }
        }, cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

    }
    async Task ReadMessage()
    {
        WebSocketReceiveResult result;
        var message = new ArraySegment<byte>(new byte[8192]);
        do
        {
            result = await client.ReceiveAsync(message, cts.Token);
            if (result.MessageType != WebSocketMessageType.Text)
                break;
            var messageBytes = message.Skip(message.Offset).Take(result.Count).ToArray();
            string receivedMessage = Encoding.UTF8.GetString(messageBytes);
            var filterResponse = JsonSerializer.Deserialize<GenericFilterResponse>(receivedMessage)!;
            if (filterResponse.type == "auth_required")
            {
                SendMessageAsync(JsonSerializer.Serialize(new AuthMessage { type = "auth", access_token = apiKey }));
            }
            else if (filterResponse.type == "auth_ok")
            {
                if (reciveMode == "TestApiKey")
                {
                    inverterLoginResponse = new InverterLoginResponse
                    {
                        token = apiKey,
                        expiresIn = 0,
                        tokenType = "",
                    };
                    //cts.Cancel();
                }
                connected = true;
            }
            else if (filterResponse.id == 2)
            {
                var prefs = JsonSerializer.Deserialize<EnergyPrefsResult>(receivedMessage)!;
                if (prefs.success)
                {
                    var returnlist = new List<InverterSite>();
                    int fakeId = 1;
                    foreach (var item in prefs.result.energy_sources)
                    {
                        if (item.type == "solar")
                        {
                            returnlist.Add(new InverterSite { Id = fakeId.ToString(), Name = item.stat_energy_from });
                            fakeId++;
                        }
                    }
                    pickerOneReturnlist = returnlist;
                }
                pickerReslutDone = true;
                //cts.Cancel();
            }
            else if (filterResponse.id > 10)
            {
                var energy = JsonSerializer.Deserialize<EnergyProductionResult>(receivedMessage)!;
                lastEnergyResult = energy;
                energyDataResponseExist = true;
            }
            Console.WriteLine("Received: {0}", receivedMessage);

        }
        while (!result.EndOfMessage);
    }
    async void SendMessageAsync(string message)
    {
        if (!CanSendMessage(message))
            return;

        var byteMessage = Encoding.UTF8.GetBytes(message);
        var segmnet = new ArraySegment<byte>(byteMessage);

        await client.SendAsync(segmnet, WebSocketMessageType.Text, true, cts.Token);
        Console.WriteLine("Send: {0}", Encoding.UTF8.GetString(byteMessage));
    }


    bool CanSendMessage(string message)
    {
        return IsConnected && !string.IsNullOrEmpty(message);
    }
    #endregion


    public async Task<Result<InverterLoginResponse>> TestConnection(string userName, string password, string apiUrl, string apiKey)
    {
        try
        {
            await ConnectToServerAsync("TestApiKey", "", "", apiUrl, apiKey);
            while (connected == false)
                await Task.Delay(1000);
            if (inverterLoginResponse == null)
                return new Result<InverterLoginResponse>("inverterLoginResponse is null");
            
            return new Result<InverterLoginResponse>(inverterLoginResponse);
        }
        catch (Exception ex)
        {
            return new Result<InverterLoginResponse>(ex.Message);
        }
    }
    private List<InverterSite> pickerOneReturnlist;
    private bool pickerReslutDone;
    public async Task<Result<List<InverterSite>>> GetPickerOne()
    {
        await Task.Factory.StartNew(async () =>
        {
            while (pickerReslutDone == false)
            {
                await ReadMessage();
            }
        }, cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

        SendMessageAsync(JsonSerializer.Serialize(new GetPrefsRequest { id = 2, type = "energy/get_prefs" }));

        while (pickerReslutDone == false)
            await Task.Delay(1000);

        return new Result<List<InverterSite>>(pickerOneReturnlist);


    }
    public Task<Result<GetInverterResponse>> GetInverter(InverterSite inverterSite)
    {
        //we return the same this for the general flow should work
        return Task.FromResult(new Result<GetInverterResponse>(new GetInverterResponse { InverterId = inverterSite.Name, Name = inverterSite.Name }));
    }


    bool energyDataResponseExist;
    EnergyProductionResult lastEnergyResult;
    public async Task<Result<DataSyncResponse>> Sync(DateTime start, IProgress<int> progress, int progressStartNr)
    {
        int websocketId = 11;
        var inverter = await mscDbContext.Inverter.OrderByDescending(s => s.FromDate).FirstAsync(x => x.HomeId == MySolarCellsGlobals.SelectedHome.HomeId);
        //LOGIN
        string apiKey="";
        
        if (inverter.ApiKey != null)
            apiKey = StringHelper.Decrypt(inverter.ApiKey, AppConstants.Secretkey);
        
        var loginResult = await TestConnection("", "", inverter.ApiUrl?? "", apiKey);
        if (!loginResult.WasSuccessful)
            return new Result<DataSyncResponse>(loginResult.ErrorMessage);
        inverterLoginResponse = loginResult.Model;

        try
        {
            int batch100 = 0;
            DateTime processingDateFrom;
            DateTime processingDateTo;
            int homeId = MySolarCellsGlobals.SelectedHome.HomeId;
            List<Energy> eneryList = new List<Energy>();
            DateTime end = DateTime.Now;
            DateTime nextStart = new DateTime();

            //var deviceIds = new List<int>();
            //deviceIds.Add(Convert.ToInt32(inverter.SubSystemEntityId));
            //var dataTypes = new List<string>();
            //dataTypes.Add("ac_hourly_yield");
            //KostalUserRoleDeviceProductionResponse dayProduction = new KostalUserRoleDeviceProductionResponse();
            int daysScope = 4;
            while (start < end)
            {
                //Get daysScope mounts per request
                if (start.AddDays(daysScope) < end)
                {
                    processingDateFrom = new DateTime(start.Year, start.Month, start.Day);//.ToString("yyyy-MM-dd");
                    processingDateTo = new DateTime(start.AddDays(daysScope).Year, start.AddDays(daysScope).Month, start.AddDays(daysScope).Day);//.ToString("yyyy-MM-dd");
                    nextStart = start.AddDays(daysScope);
                }
                else
                {
                    processingDateFrom = new DateTime(start.Year, start.Month, start.Day);//.ToString("yyyy-MM-dd");
                    processingDateTo = new DateTime(end.Year, end.Month, end.Day);//.ToString("yyyy-MM-dd");
                    //Sista dagen då kör vi till midnatt
                    if (end.Day == start.Day)
                        processingDateTo = processingDateTo.AddDays(1);

                    nextStart = end;
                }

                energyDataResponseExist = false;

                await Task.Factory.StartNew(async () =>
                {
                    while (pickerReslutDone == false)
                    {
                        await ReadMessage();
                    }
                }, cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

                SendMessageAsync(JsonSerializer.Serialize(new EnergyProductionRequest
                {
                    id = websocketId,
                    type = "energy/fossil_energy_consumption",
                    co2_statistic_id = "power",
                    energy_statistic_ids = new List<string> { inverter.SubSystemEntityId },
                    period = "hour",
                    start_time = processingDateFrom.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'"),
                    end_time = processingDateTo.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'")
                }));

                websocketId++;

                while (energyDataResponseExist == false)
                    await Task.Delay(1000);


                foreach (var item in lastEnergyResult.result)
                {
                    progressStartNr++;
                    batch100++;
                    var timestampProd = Convert.ToDateTime(item.Key);
                    var energyExist = mscDbContext.Energy.FirstOrDefault(x => x.Timestamp == timestampProd);
                    if (energyExist != null && (energyExist.Timestamp < end.AddHours(-2)))
                    {
                        if (item.Value > 0 && energyExist.ProductionSoldSynced)
                        {
                            energyExist.ProductionOwnUse = item.Value - Convert.ToDouble(energyExist.ProductionSold);
                            //Only add if price over zero
                            if (energyExist.UnitPriceBuy > 0)
                                energyExist.ProductionOwnUseProfit = energyExist.ProductionOwnUse * energyExist.UnitPriceBuy;
                            else
                                energyExist.ProductionOwnUseProfit = 0;
                        }
                        energyExist.InverterTypProductionOwnUse = (int)InverterTyp.HomeAssistent;

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
                if (lastEnergyResult.result.Count < (daysScope * 24))
                {
                    progressStartNr = progressStartNr + ((daysScope * 24) - lastEnergyResult.result.Count);
                }
                start = nextStart;
            }

            if (eneryList.Count > 0)
            {
                await Task.Delay(100); //Så att GUI hinner uppdatera
                progress.Report(progressStartNr);

                batch100 = 0;
                await mscDbContext.BulkUpdateAsync(eneryList);
                eneryList = new List<Energy>();
            }
            return new Result<DataSyncResponse>(new DataSyncResponse
            {
                SyncState = DataSyncState.ProductionSync,
                Message = AppResources.Import_Of_Production_Done
            }, true);
        }
        catch (Exception ex)
        {
            return new Result<DataSyncResponse>(ex.Message);
        }



    }
}
//Response
#region Websocket
class AuthMessage
{
    public string type { get; set; } = "";
    public string access_token { get; set; }= "";
}
class GetPrefsRequest
{
    public int id { get; set; }
    public string type { get; set; }= "";
}
//used to filter incoming messags
class GenericFilterResponse
{
    public int id { get; set; }
    public string type { get; set; }= "";
    public string ha_version { get; set; }= "";
}



class EnergyPrefsResult
{
    public int id { get; set; }
    public string type { get; set; }= "";
    public bool success { get; set; }
    public EnergyPrefs result { get; set; } = new();
}
class EnergyPrefs
{
    public List<EnergySource> energy_sources { get; set; }= new();
    //public List<DeviceConsumption> device_consumption { get; set; }
}

class EnergySource
{
    public string type { get; set; } = "";
    //public List<FlowFrom> flow_from { get; set; }
    //public List<FlowTo> flow_to { get; set; }
    public double cost_adjustment_day { get; set; }
    public string stat_energy_from { get; set; } = "";
    public object config_entry_solar_forecast { get; set; }= new();
    public string stat_cost { get; set; } = "";
    public object entity_energy_price { get; set; }= new();
    public object number_energy_price { get; set; }= new();
}
//public class DeviceConsumption
//{
//    public string stat_consumption { get; set; }
//}



//public class FlowFrom
//{
//    public string stat_energy_from { get; set; }
//    public object entity_energy_price { get; set; }
//    public object number_energy_price { get; set; }
//    public string stat_cost { get; set; }
//}

//public class FlowTo
//{
//    public string stat_energy_to { get; set; }
//    public string stat_compensation { get; set; }
//    public object entity_energy_price { get; set; }
//    public object number_energy_price { get; set; }
//}
class EnergyProductionRequest
{
    public int id { get; set; }
    public string type { get; set; } = "";
    public string start_time { get; set; } = "";
    public string end_time { get; set; } = "";
    public List<string> energy_statistic_ids { get; set; }= new();
    public string period { get; set; } = "";
    public string co2_statistic_id { get; set; } = "";
}
class EnergyProductionResult
{
    public int id { get; set; }
    public string type { get; set; } = "";
    public bool success { get; set; }
    //public List<object> result { get; set; }
    public Dictionary<string, double> result { get; set; } = new();

}
#endregion








