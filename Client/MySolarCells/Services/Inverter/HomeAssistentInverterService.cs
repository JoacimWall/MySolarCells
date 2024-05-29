using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using MySolarCells.Services.Inverter.Models;
using MySolarCellsSQLite.Sqlite.Models;

namespace MySolarCells.Services.Inverter;

public class HomeAssistentInverterService : IInverterServiceInterface
{

    //private HttpClient restClient;
    private readonly MscDbContext mscDbContext;
    private readonly IHomeService homeService;
    private readonly ILogService logService;
    private InverterLoginResponse? inverterLoginResponse;
    //private readonly JsonSerializerOptions jsonOptions;

    public string InverterGuideText => "WebSockets API is on the same url as the HA web frontend." + Environment.NewLine +
                    "ws or wss://IP_ADDRESS:8123/api/websocket" + Environment.NewLine + "You obtain a token under your profile in HA.";
    public string DefaultApiUrl => "wss://yourserver.duckdns.org/api/websocket";
    public bool ShowUserName => false;

    public bool ShowPassword => false;

    public bool ShowApiUrl => true;

    public bool ShowApiKey => true;

    public HomeAssistentInverterService(MscDbContext mscDbContext,IHomeService homeService,ILogService logService)
    {
        this.mscDbContext = mscDbContext;
        this.homeService = homeService;
        this.logService = logService;
       
        client = new ClientWebSocket();
        cts = new CancellationTokenSource();
        apiKey = "";
        pickerOneReturnlist = new();
        lastEnergyResult = new();

    }
    #region WebSocket

    private string apiKey;
    private string receiveMode="";
    private ClientWebSocket client;
    private CancellationTokenSource cts;

    private bool IsConnected => client.State == WebSocketState.Open;

    private bool connected;
    async Task ConnectToServerAsync(string receiveMode, string userName, string password, string apiUrl, string apiKey)
    {
        this.apiKey = apiKey;
        this.receiveMode = receiveMode;
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
                if (receiveMode == "TestApiKey")
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
                    var returnList = new List<InverterSite>();
                    int fakeId = 1;
                    foreach (var item in prefs.result.energy_sources)
                    {
                        if (item.type == "solar")
                        {
                            returnList.Add(new InverterSite { Id = fakeId.ToString(), Name = item.stat_energy_from });
                            fakeId++;
                        }
                    }
                    pickerOneReturnlist = returnList;
                }
                pickerReslutDone = true;
            }
            else if (filterResponse.id > 10)
            {
                var energy = JsonSerializer.Deserialize<EnergyProductionResult>(receivedMessage)!;
                lastEnergyResult = energy;
                energyDataResponseExist = true;
            }
            logService.ConsoleWriteLineDebug($"Received: {receivedMessage}");
        
        }
        while (!result.EndOfMessage);
    }
    async void SendMessageAsync(string message)
    {
        if (!CanSendMessage(message))
            return;

        var byteMessage = Encoding.UTF8.GetBytes(message);
        var segment = new ArraySegment<byte>(byteMessage);

        await client.SendAsync(segment, WebSocketMessageType.Text, true, cts.Token);
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
        var inverter = await mscDbContext.Inverter.OrderByDescending(s => s.FromDate).FirstAsync(x => x.HomeId == homeService.CurrentHome().HomeId);
        //LOGIN
        string inverterApiKey="";
        
        if (inverter.ApiKey != null)
            inverterApiKey = inverter.ApiKey.Decrypt(AppConstants.Secretkey);
        
        var loginResult = await TestConnection("", "", inverter.ApiUrl?? "", inverterApiKey);
        if (!loginResult.WasSuccessful)
            return new Result<DataSyncResponse>(loginResult.ErrorMessage);
        inverterLoginResponse = loginResult.Model;

        try
        {
            int batch100 = 0;
            var energyList = new List<Energy>();
            var end = DateTime.Now;
            progress.Report(progressStartNr);
            const int daysScope = 4;
            while (start < end)
            {
                //Get daysScope mounts per request
                DateTime nextStart;
                DateTime processingDateFrom;
                DateTime processingDateTo;
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
                    if (end.Day == DateTime.Now.Day)
                    {
                        var nextDay = DateTime.Now.AddDays(1);
                        processingDateTo = new DateTime(nextDay.Year, nextDay.Month, nextDay.Day);
                    }

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
                    batch100++;
                    var timestampProd = Convert.ToDateTime(item.Key);
                    var energyExist = mscDbContext.Energy.FirstOrDefault(x => x.Timestamp == timestampProd);
                    var dateTimeMinus15MinHeltimma = DateTime.Now.AddMinutes(-15);
                    
                    if (energyExist != null && energyExist.ProductionSoldSynced && (energyExist.Timestamp <= new DateTime(dateTimeMinus15MinHeltimma.Year, dateTimeMinus15MinHeltimma.Month, dateTimeMinus15MinHeltimma.Day, dateTimeMinus15MinHeltimma.Hour,0,0)))
                    {
                        if (item.Value > 0)
                        {
                            energyExist.ProductionOwnUse = item.Value - Convert.ToDouble(energyExist.ProductionSold);
                            //Only add if price over zero
                            if (energyExist.UnitPriceBuy > 0)
                                energyExist.ProductionOwnUseProfit = energyExist.ProductionOwnUse * energyExist.UnitPriceBuy;
                            else
                                energyExist.ProductionOwnUseProfit = 0;

                            
                        }
                        energyExist.ProductionOwnUseSynced = true;
                        energyExist.InverterTypeProductionOwnUse = (int)InverterTypeEnum.HomeAssistent;
                        energyList.Add(energyExist);

                    }
                    if (batch100 == 100)
                    {
                        batch100 = 0;
                        await mscDbContext.BulkUpdateAsync(energyList);
                        energyList = new List<Energy>();
                    }
                }
                //Update progress
                progressStartNr = progressStartNr + ((daysScope * 24));
                progress.Report(progressStartNr);
                await Task.Delay(200); //Så att GUI hinner uppdatera
                start = nextStart;
            }

            if (energyList.Count > 0)
            {
                
                await mscDbContext.BulkUpdateAsync(energyList);
            }
            
            progress.Report(progressStartNr);
            await Task.Delay(200); //So that the GUI has time to update
            return new Result<DataSyncResponse>(new DataSyncResponse
            {
                SyncState = DataSyncState.ProductionSync,
                Message = AppResources.Import_Of_Production_Done
            });
        }
        catch (Exception ex)
        {
            return new Result<DataSyncResponse>(ex.Message);
        }



    }
}









