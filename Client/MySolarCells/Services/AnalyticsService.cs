namespace MySolarCells.Services;
public interface IAnalyticsService
{
    void LogEvent(string eventId);
    void LogEvent(string eventId, string paramName, string value);
    void LogEvent(string eventName, IDictionary<string, object> parameters);
    void SetUserProperty(string name, string value);
    void SetCurrentScreen(string screenName, string screenClass);

    bool IsTabbedPageViewModel(string viewModelName);

}
public class AnalyticsService : IAnalyticsService
{
    public bool IsTabbedPageViewModel(string viewModelName)
    {
        //throw new NotImplementedException();
        return true;
    }

    public void LogEvent(string eventId)
    {
        //throw new NotImplementedException();
    }

    public void LogEvent(string eventId, string paramName, string value)
    {
        // throw new NotImplementedException();
    }

    public void LogEvent(string eventName, IDictionary<string, object> parameters)
    {
        //throw new NotImplementedException();
    }

    public void SetCurrentScreen(string screenName, string screenClass)
    {
        //throw new NotImplementedException();
    }

    public void SetUserProperty(string name, string value)
    {
        //throw new NotImplementedException();
    }
}