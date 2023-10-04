using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace MySolarCells.Services;

public interface IRestClient
{
    Task<Result<T>> ExecutePostAsync<T>(string quary, object data = null, Dictionary<string, string> parameters = null, bool autoLogInOnUnauthorized = true);
    Task<Result<bool>> ExecutePostBoolAsync(string quary, object data = null, Dictionary<string, string> parameters = null, bool autoLogInOnUnauthorized = true);
    Task<Result<T>> ExecuteGetAsync<T>(string quary, Dictionary<string, string> parameters = null, bool autoLogInOnUnauthorized = true);
    Task<Result<bool>> ExecuteDeleteBoolAsync(string quary, Dictionary<string, string> parameters = null, bool autoLogInOnUnauthorized = true);
    Task<Result<T>> ExecuteDeleteAsync<T>(string quary, Dictionary<string, string> parameters = null, bool autoLogInOnUnauthorized = true);
    Task<Result<T>> ExecutePatchAsync<T>(string quary, object data = null, Dictionary<string, string> parameters = null, bool autoLogInOnUnauthorized = true);
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
    HttpClient client;
    // private JsonSerializer serializer = new JsonSerializer();
    private bool initIsDone;
    private readonly JsonSerializerOptions jsonOptions;
    private readonly bool logTimeToDebugWindows = true;
    public RestClient()
    {
        jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    #region Public
    public ApiSettings ApiSettings { get; set; }

    public void ReInit()
    {

        this.client = new HttpClient();

        this.client.BaseAddress = new Uri(this.ApiSettings.BaseUrl);
        this.client.Timeout = new TimeSpan(0, 0, 60);

        this.client.DefaultRequestHeaders.Accept.Clear();
        this.client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
        this.client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
        this.client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
        this.client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));
        this.client.DefaultRequestHeaders.Add("Connection", "keep-alive");
        if (ApiSettings.defaultRequestHeaders != null)
            foreach (var header in ApiSettings.defaultRequestHeaders)
            {
                client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }

        

        this.initIsDone = true;
    }
    public Result<bool> UpdateToken(Dictionary<string, string> defaultRequestHeaders)
    {
        try
        {
                foreach (var header in defaultRequestHeaders)
                {
                    this.client.DefaultRequestHeaders.Remove(header.Key);
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
    private void LogExecuteTime(DateTime start, string quary)
    {
#if DEBUG
        MySolarCellsGlobals.ConsoleWriteLineDebug(String.Format("Log Api: {0} milliseconds for Api call:{1}", (DateTime.Now - start).TotalMilliseconds.ToString(), quary));
#endif
    }
    public async Task<Result<T>> ExecuteGetAsync<T>(string quary, Dictionary<string, string> parameters = null, bool autoLogInOnUnauthorized = true)
    {
        try
        {
            //This so only run this when needded 
            if (!this.initIsDone)
                ReInit();

            if (InternetConnectionHelper.InternetAccess())
            {

                if (parameters != null)
                    quary = AddParamters(quary, parameters);
                //Logger
                DateTime logTime = DateTime.Now;

                var result = await this.client.GetAsync(quary).ConfigureAwait(false);

                if (this.logTimeToDebugWindows)
                    LogExecuteTime(logTime, quary);

                using (var stream = await result.Content.ReadAsStreamAsync())
                {
                    if (result.IsSuccessStatusCode)
                    { 
                        if (typeof(T).Name == "Boolean")
                        {
                            return new Result<T>(true);
                        }
                         else if (typeof(T) == new BoolModel().GetType())
                        {
                            var answer = new BoolModel { ApiResponse = await JsonSerializer.DeserializeAsync<bool>(stream, jsonOptions) };
                            //"rememberMe": true
                            string modelstring = JsonSerializer.Serialize(answer, jsonOptions);
                            return new Result<T>(JsonSerializer.Deserialize<T>(modelstring, jsonOptions));
                        }
                        else
                            return new Result<T>(await JsonSerializer.DeserializeAsync<T>(stream, jsonOptions));
                    }
                    else if (result.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        if (autoLogInOnUnauthorized)
                        {
                            var generic = ServiceHelper.GetService<IMyRestClientGeneric>();
                            var resultLogin = await generic.SilentLogin(true);
                            if (resultLogin)
                                return await ExecuteGetAsync<T>(quary, null, false);
                        }

                        return new Result<T>("User_Need_To_Log_In_Again");
                    }
                    else if (result.StatusCode == HttpStatusCode.NotFound)
                    {
                        return new Result<T>("Current_API_Not_Found_On_Server");
                    }
                    else if (result.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                    {
                        return new Result<T>("TmResources.Server_Error");
                    }
                    else
                    {
                        try
                        {
                            return new Result<T>(await JsonSerializer.DeserializeAsync<GenericResponse>(stream, jsonOptions));
                        }
                        catch (Exception ex)
                        {
                            var logdic = new Dictionary<string, string>();
                            logdic.Add("Info", "Error in ExecuteGetAsync");
                            MySolarCellsGlobals.ReportErrorToAppCenter(ex, logdic);
                            return new Result<T>("TmResources.Server_Error" + Environment.NewLine + result.Content.ReadAsStringAsync().Result);
                        }
                        
                    }
                }

            }
            return new Result<T>("No_Internet");
        }
        catch (Exception ex)
        {
            return new Result<T>(ex.Message);
        }
    }

    public async Task<Result<bool>> ExecuteDeleteBoolAsync(string quary, Dictionary<string, string> parameters = null, bool autoLogInOnUnauthorized = true)
    {
        try
        {
            //This so only run this when needded 
            if (!initIsDone)
                ReInit();
            if (InternetConnectionHelper.InternetAccess())
            {
                if (parameters != null)
                    quary = AddParamters(quary, parameters);
                //Logger
                DateTime logTime = DateTime.Now;


                var result = await this.client.DeleteAsync(quary).ConfigureAwait(false);

                if (this.logTimeToDebugWindows)
                    LogExecuteTime(logTime, quary);

                if (result.IsSuccessStatusCode)
                {
                    return new Result<bool>(true);
                }
                else if (result.StatusCode == HttpStatusCode.Unauthorized)
                {
                    if (autoLogInOnUnauthorized)
                    {
                        var generic = ServiceHelper.GetService<IMyRestClientGeneric>();
                        var resultLogin = await generic.SilentLogin(true);
                        if (resultLogin)
                            return await ExecuteDeleteBoolAsync(quary, null, false);
                    }
                    return new Result<bool>("User_Need_To_Log_In_Again");
                }
                else if (result.StatusCode == HttpStatusCode.NotFound)
                {
                    return new Result<bool>("Current_API_Not_Found_On_Server");
                }
                else if (result.StatusCode == HttpStatusCode.InternalServerError)
                {
                    return new Result<bool>("TmResources.Server_Error");
                }
                else
                {
                    using (var stream = await result.Content.ReadAsStreamAsync())
                    {
                        try
                        {
                            return new Result<bool>(await JsonSerializer.DeserializeAsync<GenericResponse>(stream, jsonOptions));
                        }
                        catch (Exception ex)
                        {
                            var logdic = new Dictionary<string, string>();
                            logdic.Add("Info", "Error in ExecuteDeleteBoolAsync");
                            MySolarCellsGlobals.ReportErrorToAppCenter(ex, logdic);
                            return new Result<bool>("TmResources.Server_Error" + Environment.NewLine + result.Content.ReadAsStringAsync().Result);
                        }
                       
                    }
                }

            }
            return new Result<bool>("No_Internet");
        }
        catch (Exception ex)
        {
            return new Result<bool>(ex.Message);
        }
    }

    public async Task<Result<T>> ExecuteDeleteAsync<T>(string quary, Dictionary<string, string> parameters = null, bool autoLogInOnUnauthorized = true)
    {
        try
        {
            //This so only run this when needded 
            if (!initIsDone)
                ReInit();
            if (InternetConnectionHelper.InternetAccess())
            {
                if (parameters != null)
                    quary = AddParamters(quary, parameters);

                //Logger
                DateTime logTime = DateTime.Now;


                var result = await this.client.DeleteAsync(quary).ConfigureAwait(false);

                if (this.logTimeToDebugWindows)
                    LogExecuteTime(logTime, quary);

                using (var stream = await result.Content.ReadAsStreamAsync())
                {
                    if (result.IsSuccessStatusCode)
                    {
                        return new Result<T>(await JsonSerializer.DeserializeAsync<T>(stream, jsonOptions));
                    }
                    else if (result.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        if (autoLogInOnUnauthorized)
                        {
                            var generic = ServiceHelper.GetService<IMyRestClientGeneric>();
                            var resultLogin = await generic.SilentLogin(true);
                            if (resultLogin)
                                return await ExecuteDeleteAsync<T>(quary, null, false);
                        }
                        return new Result<T>("User_Need_To_Log_In_Again");
                    }
                    else if (result.StatusCode == HttpStatusCode.NotFound)
                    {
                        return new Result<T>("Current_API_Not_Found_On_Server");
                    }
                    else if (result.StatusCode == HttpStatusCode.InternalServerError)
                    {
                        return new Result<T>("TmResources.Server_Error");
                    }
                    else
                    {
                        try
                        {
                            return new Result<T>(await JsonSerializer.DeserializeAsync<GenericResponse>(stream, jsonOptions));
                        }
                        catch (Exception ex)
                        {
                            var logdic = new Dictionary<string, string>();
                            logdic.Add("Info", "Error in ExecuteDeleteAsync");
                            MySolarCellsGlobals.ReportErrorToAppCenter(ex, logdic);
                            return new Result<T>("TmResources.Server_Error" + Environment.NewLine + result.Content.ReadAsStringAsync().Result);
                        }
                    }
                }


            }
            return new Result<T>("No_Internet");
        }
        catch (Exception ex)
        {
            var logdic = new Dictionary<string, string>();
            logdic.Add("Info", "Error in ExecuteDeleteAsync");
            MySolarCellsGlobals.ReportErrorToAppCenter(ex, logdic);
            return new Result<T>(ex.Message);
        }
    }
    public async Task<Result<T>> ExecutePostAsync<T>(string quary, object data = null, Dictionary<string, string> parameters = null, bool autoLogInOnUnauthorized = true)
    {
        try
        {
            //This so only run this when needded 
            if (!initIsDone)
                ReInit();
            if (InternetConnectionHelper.InternetAccess())
            {
                StringContent content = new StringContent("");
                if (data != null) //, Encoding.UTF8, "application/json")
                    content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");

                if (parameters != null)
                    quary = AddParamters(quary, parameters);

                //Logger
                DateTime logTime = DateTime.Now;


                var result = await this.client.PostAsync(quary, content).ConfigureAwait(false);

                if (this.logTimeToDebugWindows)
                    LogExecuteTime(logTime, quary);

                using (var stream = await result.Content.ReadAsStreamAsync())
                {
                    if (result.IsSuccessStatusCode)
                    {
                        return new Result<T>(await JsonSerializer.DeserializeAsync<T>(stream, jsonOptions));
                    }
                    else if (result.StatusCode == HttpStatusCode.Unauthorized)
                    {
                       
                        if (autoLogInOnUnauthorized)
                        {
                            var generic = ServiceHelper.GetService<IMyRestClientGeneric>();
                            var resultLogin = await generic.SilentLogin(true);
                            if (resultLogin)
                                return await ExecutePostAsync<T>(quary, data, null, false);
                        }
                        return new Result<T>("User_Need_To_Log_In_Again");
                    }
                    else if (result.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return new Result<T>("Current_API_Not_Found_On_Server");
                    }
                    else if (result.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                    {
                        return new Result<T>("TmResources.Server_Error");
                    }
                    else
                    {
                        try
                        {
                            return new Result<T>(await JsonSerializer.DeserializeAsync<GenericResponse>(stream, jsonOptions));
                        }
                        catch (Exception ex)
                        {
                            var logdic = new Dictionary<string, string>();
                            logdic.Add("Info", "Error in ExecutePostAsync");
                            MySolarCellsGlobals.ReportErrorToAppCenter(ex, logdic);
                            return new Result<T>("TmResources.Server_Error" + Environment.NewLine + result.Content.ReadAsStringAsync().Result);
                        }
                        
                    }
                }


            }
            return new Result<T>("No_Internet");
        }
        catch (Exception ex)
        {
            var logdic = new Dictionary<string, string>();
            logdic.Add("Info", "Error in ExecutePostAsync");
            MySolarCellsGlobals.ReportErrorToAppCenter(ex, logdic);
            return new Result<T>(ex.Message);
        }

    }
    public async Task<Result<bool>> ExecutePostBoolAsync(string quary, object data = null, Dictionary<string, string> parameters = null, bool autoLogInOnUnauthorized = true)
    {
        try
        {
            //This so only run this when needded 
            if (!initIsDone)
                ReInit();
            if (InternetConnectionHelper.InternetAccess())
            {
                StringContent content = new StringContent("");
                if (data != null)
                    content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");

                if (parameters != null)
                    quary = AddParamters(quary, parameters);

                //Logger
                DateTime logTime = DateTime.Now;


                var result = await this.client.PostAsync(quary, content).ConfigureAwait(false);

                if (this.logTimeToDebugWindows)
                    LogExecuteTime(logTime, quary);

                using (var stream = await result.Content.ReadAsStreamAsync())
                {
                    if (result.IsSuccessStatusCode)
                    {
                        return new Result<bool>(true);
                    }
                    else if (result.StatusCode == HttpStatusCode.Unauthorized)
                    {
                     
                        if (autoLogInOnUnauthorized)
                        {
                            var generic = ServiceHelper.GetService<IMyRestClientGeneric>();
                            var resultLogin = await generic.SilentLogin(true);
                            if (resultLogin)
                                return await ExecutePostAsync<bool>(quary, data, null, false);
                        }
                        return new Result<bool>("User_Need_To_Log_In_Again");
                    }
                    else if (result.StatusCode == HttpStatusCode.NotFound)
                    {
                        return new Result<bool>("Current_API_Not_Found_On_Server");
                    }
                    else if (result.StatusCode == HttpStatusCode.InternalServerError)
                    {
                        return new Result<bool>("TmResources.Server_Error");
                    }
                    else
                    {
                        try
                        {
                            return new Result<bool>(await JsonSerializer.DeserializeAsync<GenericResponse>(stream, jsonOptions));
                        }
                        catch (Exception ex)
                        {
                            var logdic = new Dictionary<string, string>();
                            logdic.Add("Info", "Error in ExecutePostBoolAsync");
                            MySolarCellsGlobals.ReportErrorToAppCenter(ex, logdic);
                            return new Result<bool>("TmResources.Server_Error" + Environment.NewLine + result.Content.ReadAsStringAsync().Result);
                        }
                        
                    }
                }


            }
            return new Result<bool>("No_Internet");
        }
        catch (Exception ex)
        {
            var logdic = new Dictionary<string, string>();
            logdic.Add("Info", "Error in ExecutePostBoolAsync");
            MySolarCellsGlobals.ReportErrorToAppCenter(ex, logdic);
            return new Result<bool>(ex.Message);
        }

    }
    public async Task<Result<T>> ExecutePatchAsync<T>(string quary, object data = null, Dictionary<string, string> parameters = null, bool autoLogInOnUnauthorized = true)
    {
        try
        {
            //This so only run this when needded 
            if (!initIsDone)
                ReInit();
            if (InternetConnectionHelper.InternetAccess())
            {
                StringContent content = new StringContent("");
                if (data != null)
                    content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");

                if (parameters != null)
                    quary = AddParamters(quary, parameters);

                using (HttpRequestMessage RequestMessage = new HttpRequestMessage(new HttpMethod("PATCH"), quary))
                {
                    RequestMessage.Content = content;

                    //Logger
                    DateTime logTime = DateTime.Now;

                    var result = await this.client.SendAsync(RequestMessage).ConfigureAwait(false);

                    if (this.logTimeToDebugWindows)
                        LogExecuteTime(logTime, quary);

                    using (var stream = await result.Content.ReadAsStreamAsync())
                                       {
                        if (result.IsSuccessStatusCode)
                        {
                            return new Result<T>(await JsonSerializer.DeserializeAsync<T>(stream,jsonOptions));
                        }
                        else if (result.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            if (autoLogInOnUnauthorized)
                            {
                                var generic = ServiceHelper.GetService<IMyRestClientGeneric>();
                                var resultLogin = await generic.SilentLogin(true);
                                if (resultLogin)
                                    return await ExecutePatchAsync<T>(quary, data, null, false);
                            }
                            return new Result<T>("User_Need_To_Log_In_Again");
                        }
                        else if (result.StatusCode == HttpStatusCode.NotFound)
                        {
                            return new Result<T>("Current_API_Not_Found_On_Server");
                        }
                        else if (result.StatusCode == HttpStatusCode.InternalServerError)
                        {
                            return new Result<T>("TmResources.Server_Error");
                        }
                        else
                        {
                            try
                            {
                                return new Result<T>(await JsonSerializer.DeserializeAsync<GenericResponse>(stream, jsonOptions));
                            }
                            catch (Exception ex)
                            {
                                var logdic = new Dictionary<string, string>();
                                logdic.Add("Info", "Error in ExecutePatchAsync");
                                MySolarCellsGlobals.ReportErrorToAppCenter(ex, logdic);
                                return new Result<T>("TmResources.Server_Error" + Environment.NewLine + result.Content.ReadAsStringAsync().Result);
                            }
                            
                        }
                    }
                }

            }
            return new Result<T>("No_Internet");
        }
        catch (Exception ex)
        {
            var logdic = new Dictionary<string, string>();
            logdic.Add("Info", "Error in ExecutePatchAsync");
            MySolarCellsGlobals.ReportErrorToAppCenter(ex, logdic);
            return new Result<T>(ex.Message);
        }

    }
    public bool RemoveTokens()
    {
        this.client.DefaultRequestHeaders.Remove("AUTHENTICATION_TOKEN");
        this.client.DefaultRequestHeaders.Remove("Authorization");
        return true;

    }
    //public bool AddTokens()
    //{
    //    //_client.DefaultRequestHeaders.Add("AUTHENTICATION_TOKEN", _settingsService.AuthAccessToken);
    //    //_client.DefaultRequestHeaders.Add(AppConstants.Authorization, string.Format("Bearer {0}", _settingsService.AuthAccessToken));
    //    return true;
    //}

    
    #endregion
    #region Private
    private string AddParamters(string quary, Dictionary<string, string> parameters)
    {
        bool first = true;

        foreach (var item in parameters)
        {
            if (first)
            {
                var array = item.Value.Split(Convert.ToChar(Environment.NewLine));
                if (array.Count() > 1)
                {
                    foreach (var itemstrig in array)
                    {
                        quary = quary + string.Format(@"?{0}={1}", item.Key, itemstrig);
                    }
                }
                else
                {
                    quary = quary + string.Format(@"?{0}={1}", item.Key, item.Value);
                }

                first = false;
            }
            else
            {
                var array = item.Value.Split(Convert.ToChar(Environment.NewLine));
                if (array.Count() > 1)
                {
                    foreach (var itemstrig in array)
                    {
                        quary = quary + string.Format(@"&{0}={1}", item.Key, itemstrig);
                    }
                }
                else
                {
                    quary = quary + string.Format(@"&{0}={1}", item.Key, item.Value);
                }

            }

        }
        return quary;

    }
    #endregion


}

public class TokenModel
{
    public string Token { get; set; }

    public DateTime TokenExpire { get; set; }
}
public class ApiSettings
{
    //Propertys used for the App Backend
    public string BaseUrl { get; set; }
    public Dictionary<string, string> defaultRequestHeaders { get; set; }
    public string Enviroment { get; set; }
 
}