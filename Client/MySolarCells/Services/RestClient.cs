using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;

namespace MySolarCells.Services;

public interface IRestClient
{
    Task<Result<T>> ExecutePostAsync<T>(string query, object? data = null,Dictionary<string, string>? parameters = null, bool autoLogInOnUnauthorized = true);
    Task<Tuple<Result<T>, HttpResponseHeaders?>> ExecutePostReturnResponseHeadersAsync<T>(string query, object? data = null, Dictionary<string, string>? parameters = null, bool autoLogInOnUnauthorized = true);
    Task<Result<bool>> ExecutePostBoolAsync(string query, object? data = null,Dictionary<string, string>? parameters = null, bool autoLogInOnUnauthorized = true);
    Task<Result<T>> ExecuteGetAsync<T>(string query, Dictionary<string, string>? parameters = null,bool autoLogInOnUnauthorized = true);
    Task<Result<bool>> ExecuteDeleteBoolAsync(string query, Dictionary<string, string>? parameters = null,bool autoLogInOnUnauthorized = true);
    Task<Result<T>> ExecuteDeleteAsync<T>(string query, Dictionary<string, string>? parameters = null,
        bool autoLogInOnUnauthorized = true);

    Task<Result<T>> ExecutePatchAsync<T>(string query, object? data = null,Dictionary<string, string>? parameters = null, bool autoLogInOnUnauthorized = true);
    public bool RemoveTokens();
    public void ReInit();
    public void ClearCookieContainer();
    Result<bool> UpdateToken(Dictionary<string, string> defaultRequestHeaders);
    public ApiSettings ApiSettings { get; set; }
}
public interface IMyRestClientGeneric
{
    Task<bool> SilentLogin(bool navigateToLoginOnError = false);
}
public class RestClient : IRestClient
{
    private readonly IInternetConnectionService internetConnectionService;
    private readonly JsonSerializerOptions jsonOptions;
    private readonly ILogService logService;
    private readonly bool logTimeToDebugWindows = true;
    private readonly IMyRestClientGeneric myRestClientGeneric;
    private HttpClient client;
    private bool initIsDone;

    public RestClient(IInternetConnectionService internetConnectionService, ILogService logService,
        IMyRestClientGeneric myRestClientGeneric)
    {
        client = new HttpClient();
        this.internetConnectionService = internetConnectionService;
        this.logService = logService;
        this.myRestClientGeneric = myRestClientGeneric;
        jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    #region Private

    private static string AddParameters(string query, Dictionary<string, string> parameters)
    {
        var first = true;

        foreach (var item in parameters)
            if (first)
            {
                var array = item.Value.Split(Convert.ToChar(Environment.NewLine));
                if (array.Count() > 1)
                    foreach (var itemString in array)
                        query += $@"?{item.Key}={HttpUtility.UrlEncode(itemString)}";
                else
                    query += $@"?{item.Key}={HttpUtility.UrlEncode(item.Value)}";

                first = false;
            }
            else
            {
                var array = item.Value.Split(Convert.ToChar(Environment.NewLine));
                if (array.Count() > 1)
                    foreach (var itemString in array)
                        query += $@"&{item.Key}={HttpUtility.UrlEncode(itemString)}";
                else
                    query += $@"&{item.Key}={HttpUtility.UrlEncode(item.Value)}";
            }

        return query;
    }

    #endregion

    #region Public

    public ApiSettings ApiSettings { get; set; } = new();

    public void ReInit()
    {
        client = new HttpClient();

        client.BaseAddress = new Uri(ApiSettings.BaseUrl);
        client.Timeout = new TimeSpan(0, 0, 60);

        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
        client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
        client.DefaultRequestHeaders.Add("Connection", "keep-alive");
        foreach (var header in ApiSettings.DefaultRequestHeaders)
            client.DefaultRequestHeaders.Add(header.Key, header.Value);


        initIsDone = true;
    }

    public Result<bool> UpdateToken(Dictionary<string, string> defaultRequestHeaders)
    {
        try
        {
            foreach (var header in defaultRequestHeaders)
            {
                client.DefaultRequestHeaders.Remove(header.Key);
                client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }

            return new Result<bool>(true);
        }
        catch (Exception e)
        {
            return new Result<bool>(e.Message);
        }
    }

    public void ClearCookieContainer()
    {
        // DependencyService.Resolve<IMyHttpClientHandler>().ClearCookieContainer(_httpClientHandler);
    }

    private void LogExecuteTime(DateTime start, string query)
    {
#if DEBUG
        logService.ConsoleWriteLineDebug(
            $"Log Api: {(DateTime.Now - start).TotalMilliseconds.ToString(CultureInfo.InvariantCulture)} milliseconds for Api call:{query}");
#endif
    }
public async Task<Tuple<Result<T>, HttpResponseHeaders?>> ExecutePostReturnResponseHeadersAsync<T>(string query, object? data = null, Dictionary<string, string>? parameters = null, bool autoLogInOnUnauthorized = true)
{
    var logTime = DateTime.Now;
        try
        {
            //This so only run this when needed 
            if (!initIsDone)
                ReInit();
            if (internetConnectionService.InternetAccess())
            {
                StringContent content = new StringContent("");
                if (data != null) //, Encoding.UTF8, "application/json")
                    content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");

                if (parameters != null)
                    query = AddParameters(query, parameters);

                var result = await client.PostAsync(query, content).ConfigureAwait(false);

                if (logTimeToDebugWindows)
                    LogExecuteTime(logTime, query);

                await using var stream = await result.Content.ReadAsStreamAsync();
                if (result.IsSuccessStatusCode)
                {
                    var model = await JsonSerializer.DeserializeAsync<T>(stream, jsonOptions);
                    if (model is null)
                        return new Tuple<Result<T>, HttpResponseHeaders?>(
                            new Result<T>("Unable to Deserialize model"), null);

                    return new Tuple<Result<T>, HttpResponseHeaders?>(new Result<T>(model), result.Headers);

                }
                else if (result.StatusCode == HttpStatusCode.Unauthorized)
                {

                    // if (autoLogInOnUnauthorized)
                    // {
                    //     var generic = ServiceHelper.GetService<IMyRestClientGeneric>();
                    //     var resultLogin = await generic.SilentLogin(true);
                    //     if (resultLogin)
                    //         return await ExecutePostReturnResponseHeadersAsync<T>(query, data, null, false);
                    // }

                    return new Tuple<Result<T>, HttpResponseHeaders?>(new Result<T>("User_Need_To_Log_In_Again"),
                        null);
                }
                else if (result.StatusCode == HttpStatusCode.NotFound)
                {
                    return new Tuple<Result<T>, HttpResponseHeaders?>(
                        new Result<T>("Current_API_Not_Found_On_Server"), null);
                }
                else if (result.StatusCode == HttpStatusCode.InternalServerError)
                {
                    return new Tuple<Result<T>, HttpResponseHeaders?>(new Result<T>("AppResources.Server_Error"),
                        null);

                }
                else
                {
                    try
                    {
                        var resultModel =
                            await JsonSerializer.DeserializeAsync<GenericResponse>(stream, jsonOptions);
                        return new Tuple<Result<T>, HttpResponseHeaders?>(new Result<T>(resultModel), null);

                    }
                    catch (Exception ex)
                    {
                        var logic = new Dictionary<string, string> { { "Info", "Error in ExecutePostAsync" } };
                        logService.ReportErrorToAppCenter(ex, logic);
                        return new Tuple<Result<T>, HttpResponseHeaders?>(
                            new Result<T>("AppResources.Server_Error" + Environment.NewLine +
                                          result.Content.ReadAsStringAsync().Result), null);
                    }

                }
            }


            return new Tuple<Result<T>, HttpResponseHeaders?>(new Result<T>("No_Internet"), null);

        }
        catch (Exception ex)
        {
            var logic = new Dictionary<string, string>
            {
                { "Info", "Error in ExecuteDeleteBoolAsync" },
                { "Query", query },
                { "QueryTime", (DateTime.Now - logTime).TotalSeconds.ToString(CultureInfo.InvariantCulture) }
               
            };
            logService.ReportErrorToAppCenter(ex, logic);

           
           return new Tuple<Result<T>, HttpResponseHeaders?>(new Result<T>(ex.Message), null);
        }
      
}

    public async Task<Result<T>> ExecuteGetAsync<T>(string query, Dictionary<string, string>? parameters = null,bool autoLogInOnUnauthorized = true)
    {
        var logTime = DateTime.Now;
        var retryCount = 0;
        while (retryCount < ApiSettings.RequestRetryAttempts)
        {
            retryCount++;
            try
            {
                //This so only run this when needed 
                if (!initIsDone)
                    ReInit();

                if (internetConnectionService.InternetAccess())
                {
                    if (parameters != null && retryCount == 1)
                        query = AddParameters(query, parameters);
                    //Logger
                    logTime = DateTime.Now;

                    var result = await client.GetAsync(query);

                    if (logTimeToDebugWindows)
                        LogExecuteTime(logTime, query);

                    await using var stream = await result.Content.ReadAsStreamAsync();
                    if (result.IsSuccessStatusCode)
                    {
                        if (typeof(T).Name == "Boolean" || result.StatusCode == HttpStatusCode.NoContent)
                            return new Result<T>(true);

                        if (typeof(T) == new BoolModel().GetType())
                        {
                            var answer = new BoolModel
                                { ApiResponse = await JsonSerializer.DeserializeAsync<bool>(stream, jsonOptions) };
                            //"rememberMe": true
                            var modelString = JsonSerializer.Serialize(answer, jsonOptions);

                            var model = JsonSerializer.Deserialize<T>(modelString, jsonOptions);
                            if (model is null)
                                return new Result<T>("Unable to deserialize to model");
                            return new Result<T>(model);
                        }
                        else
                        {
                            logService.ConsoleWriteLineDebug(typeof(T).ToString());
                            var model = await JsonSerializer.DeserializeAsync<T>(stream, jsonOptions);
                            if (model is null)
                                return new Result<T>("Unable to Deserialize model");
                            return new Result<T>(model);
                        }
                    }

                    if (result.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        if (autoLogInOnUnauthorized)
                        {
                            var resultLogin = await myRestClientGeneric.SilentLogin(true);
                            if (resultLogin)
                                return await ExecuteGetAsync<T>(query, null, false);
                        }

                        return new Result<T>(AppResources.User_Need_To_Log_In_Again);
                    }

                    if (result.StatusCode == HttpStatusCode.NotFound)
                        return new Result<T>(AppResources.Current_API_Not_Found_On_Server);
                    if (result.StatusCode == HttpStatusCode.InternalServerError)
                        return new Result<T>(AppResources.Server_Error);
                    try
                    {
                        var t = await JsonSerializer.DeserializeAsync<GenericResponse>(stream, jsonOptions);
                        if (t is null)
                            return new Result<T>("Unable to convert to GenericResponse");
                        return new Result<T>(t);
                    }
                    catch (Exception ex)
                    {
                        var logic = new Dictionary<string, string>
                        {
                            { "Info", "Error in ExecuteGetAsync" },
                            { "Query", query }
                        };
                        logService.ReportErrorToAppCenter(ex, logic);
                        return new Result<T>(AppResources.Server_Error + Environment.NewLine +
                                             result.Content.ReadAsStringAsync().Result);
                    }
                }

                if (retryCount < ApiSettings.RequestRetryAttempts)
                    await Task.Delay(ApiSettings.RequestRetryAttemptsSleepTime * 1000);
                else
                    return new Result<T>(AppResources.No_Internet);
            }

            catch (Exception ex)
            {
                var logic = new Dictionary<string, string>
                {
                    { "Info", "Exception in ExecuteGetAsync" },
                    { "Query", query },
                    { "QueryTime", (DateTime.Now - logTime).TotalSeconds.ToString(CultureInfo.InvariantCulture) },
                    { "RequestAttempts", retryCount.ToString() }
                };
                logService.ReportErrorToAppCenter(ex, logic);

                if (retryCount < ApiSettings.RequestRetryAttempts)
                    await Task.Delay(ApiSettings.RequestRetryAttemptsSleepTime * 1000);
                else
                    return new Result<T>(ex.Message);
            }
        }

        return new Result<T>("Logic error in code");
    }

    public async Task<Result<bool>> ExecuteDeleteBoolAsync(string query, Dictionary<string, string>? parameters = null,bool autoLogInOnUnauthorized = true)
    {
        var logTime = DateTime.Now;
        var retryCount = 0;
        while (retryCount < ApiSettings.RequestRetryAttempts)
        {
            retryCount++;
            try
            {
                //This so only run this when needed 
                if (!initIsDone)
                    ReInit();
                if (internetConnectionService.InternetAccess())
                {
                    if (parameters != null && retryCount == 1)
                        query = AddParameters(query, parameters);
                    //Logger
                    logTime = DateTime.Now;


                    var result = await client.DeleteAsync(query);

                    if (logTimeToDebugWindows)
                        LogExecuteTime(logTime, query);

                    if (result.IsSuccessStatusCode) return new Result<bool>(true);

                    if (result.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        if (autoLogInOnUnauthorized)
                        {
                            var resultLogin = await myRestClientGeneric.SilentLogin(true);
                            if (resultLogin)
                                return await ExecuteDeleteBoolAsync(query, null, false);
                        }

                        return new Result<bool>(AppResources.User_Need_To_Log_In_Again);
                    }

                    if (result.StatusCode == HttpStatusCode.NotFound)
                        return new Result<bool>(AppResources.Current_API_Not_Found_On_Server);

                    if (result.StatusCode == HttpStatusCode.InternalServerError)
                        return new Result<bool>(AppResources.Server_Error);

                    await using var stream = await result.Content.ReadAsStreamAsync();
                    try
                    {
                        var t = await JsonSerializer.DeserializeAsync<GenericResponse>(stream, jsonOptions);
                        if (t is null)
                            return new Result<bool>("Unable to convert to GenericResponse");
                        return new Result<bool>(t);
                    }
                    catch (Exception ex)
                    {
                        var logic = new Dictionary<string, string>
                        {
                            { "Info", "Error in ExecuteDeleteBoolAsync" },
                            { "Query", query }
                        };
                        logService.ReportErrorToAppCenter(ex, logic);
                        return new Result<bool>(AppResources.Server_Error + Environment.NewLine +
                                                result.Content.ReadAsStringAsync().Result);
                    }
                }

                if (retryCount < ApiSettings.RequestRetryAttempts)
                    await Task.Delay(ApiSettings.RequestRetryAttemptsSleepTime * 1000);
                else
                    return new Result<bool>(AppResources.No_Internet);
            }
            catch (Exception ex)
            {
                var logic = new Dictionary<string, string>
                {
                    { "Info", "Error in ExecuteDeleteBoolAsync" },
                    { "Query", query },
                    { "QueryTime", (DateTime.Now - logTime).TotalSeconds.ToString(CultureInfo.InvariantCulture) },
                    { "RequestAttempts", retryCount.ToString() }
                };
                logService.ReportErrorToAppCenter(ex, logic);

                if (retryCount < ApiSettings.RequestRetryAttempts)
                    await Task.Delay(ApiSettings.RequestRetryAttemptsSleepTime * 1000);
                else
                    return new Result<bool>(ex.Message);
            }
        }

        return new Result<bool>("Logic error in code");
    }

    public async Task<Result<T>> ExecuteDeleteAsync<T>(string query, Dictionary<string, string>? parameters = null,bool autoLogInOnUnauthorized = true)
    {
        var logTime = DateTime.Now;
        var retryCount = 0;
        while (retryCount < ApiSettings.RequestRetryAttempts)
        {
            retryCount++;
            try
            {
                //This so only run this when needed 
                if (!initIsDone)
                    ReInit();
                if (internetConnectionService.InternetAccess())
                {
                    if (parameters != null && retryCount == 1)
                        query = AddParameters(query, parameters);

                    //Logger
                    logTime = DateTime.Now;


                    var result = await client.DeleteAsync(query);

                    if (logTimeToDebugWindows)
                        LogExecuteTime(logTime, query);

                    await using var stream = await result.Content.ReadAsStreamAsync();
                    if (result.IsSuccessStatusCode)
                    {
                        var model = await JsonSerializer.DeserializeAsync<T>(stream, jsonOptions);
                        if (model is null)
                            return new Result<T>("Unable to Deserialize model");
                        return new Result<T>(model);
                    }

                    if (result.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        if (autoLogInOnUnauthorized)
                        {
                            var resultLogin = await myRestClientGeneric.SilentLogin(true);
                            if (resultLogin)
                                return await ExecuteDeleteAsync<T>(query, null, false);
                        }

                        return new Result<T>(AppResources.User_Need_To_Log_In_Again);
                    }

                    if (result.StatusCode == HttpStatusCode.NotFound)
                        return new Result<T>(AppResources.Current_API_Not_Found_On_Server);
                    if (result.StatusCode == HttpStatusCode.InternalServerError)
                        return new Result<T>(AppResources.Server_Error);
                    try
                    {
                        var t = await JsonSerializer.DeserializeAsync<GenericResponse>(stream, jsonOptions);
                        if (t is null)
                            return new Result<T>("Unable to convert to GenericResponse");
                        return new Result<T>(t);
                    }
                    catch (Exception ex)
                    {
                        var logic = new Dictionary<string, string>
                        {
                            { "Info", "Error in ExecuteDeleteAsync" },
                            { "Query", query }
                        };
                        logService.ReportErrorToAppCenter(ex, logic);
                        return new Result<T>(AppResources.Server_Error + Environment.NewLine +
                                             result.Content.ReadAsStringAsync().Result);
                    }
                }

                if (retryCount < ApiSettings.RequestRetryAttempts)
                    await Task.Delay(ApiSettings.RequestRetryAttemptsSleepTime * 1000);
                else
                    return new Result<T>(AppResources.No_Internet);
            }
            catch (Exception ex)
            {
                var logic = new Dictionary<string, string>
                {
                    { "Info", "Error in ExecuteDeleteAsync" },
                    { "Query", query },
                    { "QueryTime", (DateTime.Now - logTime).TotalSeconds.ToString(CultureInfo.InvariantCulture) },
                    { "RequestAttempts", retryCount.ToString() }
                };
                logService.ReportErrorToAppCenter(ex, logic);
                if (retryCount < ApiSettings.RequestRetryAttempts)
                    await Task.Delay(ApiSettings.RequestRetryAttemptsSleepTime * 1000);
                else
                    return new Result<T>(ex.Message);
            }
        }

        return new Result<T>("Logic error in code");
    }

    public async Task<Result<T>> ExecutePostAsync<T>(string query, object? data = null,
        Dictionary<string, string>? parameters = null, bool autoLogInOnUnauthorized = true)
    {
        var logTime = DateTime.Now;
        var retryCount = 0;
        while (retryCount < ApiSettings.RequestRetryAttempts)
        {
            retryCount++;
            try
            {
                //This so only run this when needed 
                if (!initIsDone)
                    ReInit();
                if (internetConnectionService.InternetAccess())
                {
                    var content = new StringContent("");
                    if (data != null) //, Encoding.UTF8, "application/json")
                        content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");

                    if (parameters != null && retryCount == 1)
                        query = AddParameters(query, parameters);

                    //Logger
                    logTime = DateTime.Now;


                    var result = await client.PostAsync(query, content);

                    if (logTimeToDebugWindows)
                        LogExecuteTime(logTime, query);

                    await using var stream = await result.Content.ReadAsStreamAsync();
                    if (result.IsSuccessStatusCode)
                    {
                        var model = await JsonSerializer.DeserializeAsync<T>(stream, jsonOptions);
                        if (model is null)
                            return new Result<T>("Unable to Deserialize model");
                        return new Result<T>(model);
                    }

                    if (result.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        if (autoLogInOnUnauthorized)
                        {
                            var resultLogin = await myRestClientGeneric.SilentLogin(true);
                            if (resultLogin)
                                return await ExecutePostAsync<T>(query, data, null, false);
                        }

                        return new Result<T>(AppResources.User_Need_To_Log_In_Again);
                    }

                    if (result.StatusCode == HttpStatusCode.NotFound)
                        return new Result<T>(AppResources.Current_API_Not_Found_On_Server);
                    if (result.StatusCode == HttpStatusCode.InternalServerError)
                        return new Result<T>(AppResources.Server_Error);
                    try
                    {
                        var t = await JsonSerializer.DeserializeAsync<GenericResponse>(stream, jsonOptions);
                        if (t is null)
                            return new Result<T>("Unable to convert to GenericResponse");
                        return new Result<T>(t);
                    }
                    catch (Exception ex)
                    {
                        var logic = new Dictionary<string, string>
                        {
                            { "Info", "Error in ExecutePostAsync" },
                            { "Query", query }
                        };
                        logService.ReportErrorToAppCenter(ex, logic);
                        return new Result<T>(AppResources.Server_Error + Environment.NewLine +
                                             result.Content.ReadAsStringAsync().Result);
                    }
                }

                if (retryCount < ApiSettings.RequestRetryAttempts)
                    await Task.Delay(ApiSettings.RequestRetryAttemptsSleepTime * 1000);
                else
                    return new Result<T>(AppResources.No_Internet);
            }
            catch (Exception ex)
            {
                var logic = new Dictionary<string, string>
                {
                    { "Info", "Error in ExecutePostAsync" },
                    { "Query", query },
                    { "QueryTime", (DateTime.Now - logTime).TotalSeconds.ToString(CultureInfo.InvariantCulture) },
                    { "RequestAttempts", retryCount.ToString() }
                };
                logService.ReportErrorToAppCenter(ex, logic);

                if (retryCount < ApiSettings.RequestRetryAttempts)
                    await Task.Delay(ApiSettings.RequestRetryAttemptsSleepTime * 1000);
                else
                    return new Result<T>(ex.Message);
            }
        }

        return new Result<T>("Logic error in code");
    }

    public async Task<Result<bool>> ExecutePostBoolAsync(string query, object? data = null,
        Dictionary<string, string>? parameters = null, bool autoLogInOnUnauthorized = true)
    {
        var logTime = DateTime.Now;
        var retryCount = 0;
        while (retryCount < ApiSettings.RequestRetryAttempts)
        {
            retryCount++;
            try
            {
                //This so only run this when needed 
                if (!initIsDone)
                    ReInit();
                if (internetConnectionService.InternetAccess())
                {
                    var content = new StringContent("");
                    if (data != null)
                        content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");

                    if (parameters != null && retryCount == 1)
                        query = AddParameters(query, parameters);

                    //Logger
                    logTime = DateTime.Now;


                    var result = await client.PostAsync(query, content);

                    if (logTimeToDebugWindows)
                        LogExecuteTime(logTime, query);

                    await using var stream = await result.Content.ReadAsStreamAsync();
                    if (result.IsSuccessStatusCode) return new Result<bool>(true);

                    if (result.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        if (autoLogInOnUnauthorized)
                        {
                            var resultLogin = await myRestClientGeneric.SilentLogin(true);
                            if (resultLogin)
                                return await ExecutePostAsync<bool>(query, data, null, false);
                        }

                        return new Result<bool>(AppResources.User_Need_To_Log_In_Again);
                    }

                    if (result.StatusCode == HttpStatusCode.NotFound)
                        return new Result<bool>(AppResources.Current_API_Not_Found_On_Server);
                    if (result.StatusCode == HttpStatusCode.InternalServerError)
                        return new Result<bool>(AppResources.Server_Error);
                    try
                    {
                        var t = await JsonSerializer.DeserializeAsync<GenericResponse>(stream, jsonOptions);
                        if (t is null)
                            return new Result<bool>("Unable to convert to GenericResponse");
                        return new Result<bool>(t);
                    }
                    catch (Exception ex)
                    {
                        var logic = new Dictionary<string, string>
                        {
                            { "Info", "Error in ExecutePostBoolAsync" },
                            { "Query", query }
                        };
                        logService.ReportErrorToAppCenter(ex, logic);
                        return new Result<bool>(AppResources.Server_Error + Environment.NewLine +
                                                result.Content.ReadAsStringAsync().Result);
                    }
                }

                if (retryCount < ApiSettings.RequestRetryAttempts)
                    await Task.Delay(ApiSettings.RequestRetryAttemptsSleepTime * 1000);
                else
                    return new Result<bool>(AppResources.No_Internet);
            }
            catch (Exception ex)
            {
                var logic = new Dictionary<string, string>
                {
                    { "Info", "Error in ExecutePostBoolAsync" },
                    { "Query", query },
                    { "QueryTime", (DateTime.Now - logTime).TotalSeconds.ToString(CultureInfo.InvariantCulture) },
                    { "RequestAttempts", retryCount.ToString() }
                };
                logService.ReportErrorToAppCenter(ex, logic);

                if (retryCount < ApiSettings.RequestRetryAttempts)
                    await Task.Delay(ApiSettings.RequestRetryAttemptsSleepTime * 1000);
                else
                    return new Result<bool>(ex.Message);
            }
        }

        return new Result<bool>("Logic error in code");
    }

    public async Task<Result<T>> ExecutePatchAsync<T>(string query, object? data = null,
        Dictionary<string, string>? parameters = null, bool autoLogInOnUnauthorized = true)
    {
        var logTime = DateTime.Now;
        var retryCount = 0;
        while (retryCount < ApiSettings.RequestRetryAttempts)
        {
            retryCount++;
            try
            {
                //This so only run this when needed 
                if (!initIsDone)
                    ReInit();
                if (internetConnectionService.InternetAccess())
                {
                    var content = new StringContent("");
                    if (data != null)
                        content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");

                    if (parameters != null && retryCount == 1)
                        query = AddParameters(query, parameters);

                    using var requestMessage = new HttpRequestMessage(new HttpMethod("PATCH"), query);
                    requestMessage.Content = content;

                    //Logger
                    logTime = DateTime.Now;

                    var result = await client.SendAsync(requestMessage);

                    if (logTimeToDebugWindows)
                        LogExecuteTime(logTime, query);

                    await using var stream = await result.Content.ReadAsStreamAsync();
                    if (result.IsSuccessStatusCode)
                    {
                        var model = await JsonSerializer.DeserializeAsync<T>(stream, jsonOptions);
                        if (model is null)
                            return new Result<T>("Unable to Deserialize model");
                        return new Result<T>(model);
                    }

                    if (result.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        if (autoLogInOnUnauthorized)
                        {
                            var resultLogin = await myRestClientGeneric.SilentLogin(true);
                            if (resultLogin)
                                return await ExecutePatchAsync<T>(query, data, null, false);
                        }

                        return new Result<T>(AppResources.User_Need_To_Log_In_Again);
                    }

                    if (result.StatusCode == HttpStatusCode.NotFound)
                        return new Result<T>(AppResources.Current_API_Not_Found_On_Server);
                    if (result.StatusCode == HttpStatusCode.InternalServerError)
                        return new Result<T>(AppResources.Server_Error);
                    try
                    {
                        var t = await JsonSerializer.DeserializeAsync<GenericResponse>(stream, jsonOptions);
                        if (t is null)
                            return new Result<T>("Unable to convert to GenericResponse");
                        return new Result<T>(t);
                    }
                    catch (Exception ex)
                    {
                        var logic = new Dictionary<string, string>
                        {
                            { "Info", "Error in ExecutePatchAsync" },
                            { "Query", query }
                        };
                        logService.ReportErrorToAppCenter(ex, logic);
                        return new Result<T>(AppResources.Server_Error + Environment.NewLine +
                                             result.Content.ReadAsStringAsync().Result);
                    }
                }

                if (retryCount < ApiSettings.RequestRetryAttempts)
                    await Task.Delay(ApiSettings.RequestRetryAttemptsSleepTime * 1000);
                else
                    return new Result<T>(AppResources.No_Internet);
            }
            catch (Exception ex)
            {
                var logic = new Dictionary<string, string>
                {
                    { "Info", "Error in ExecutePatchAsync" },
                    { "Query", query },
                    { "QueryTime", (DateTime.Now - logTime).TotalSeconds.ToString(CultureInfo.InvariantCulture) },
                    { "RequestAttempts", retryCount.ToString() }
                };
                logService.ReportErrorToAppCenter(ex, logic);

                if (retryCount < ApiSettings.RequestRetryAttempts)
                    await Task.Delay(ApiSettings.RequestRetryAttemptsSleepTime * 1000);
                else
                    return new Result<T>(ex.Message);
            }
        }

        return new Result<T>("Logic error in code");
    }

    public bool RemoveTokens()
    {
        client.DefaultRequestHeaders.Remove("AUTHENTICATION_TOKEN");
        client.DefaultRequestHeaders.Remove("Authorization");
        return true;
    }
    
    #endregion
}
public class MyRestClientGeneric : IMyRestClientGeneric
{
    public async Task<bool> SilentLogin(bool navigateToLoginOnError = false)
    {
        //IMemberService? memberService = ServiceHelper.GetService<IMemberService>();
        //return await memberService.SilentLogin(navigateToLoginOnError);
       return await Task.FromResult(false);
      
    }
}
public class TokenModel
{
    public string? Token { get; set; }

    public DateTime TokenExpire { get; set; }
}
public class ApiSettings
{
    //Property's used for the App Backend
    public int RequestRetryAttempts { get; set; } = 3;
    public int RequestRetryAttemptsSleepTime { get; set; } = 5;
    public string BaseUrl { get; set; } = "";
    public Dictionary<string, string> DefaultRequestHeaders { get; set; } = new Dictionary<string, string>();
    public string Environment { get; set; } = "";

}