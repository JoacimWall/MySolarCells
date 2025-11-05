using Preferences = MySolarCellsSQLite.Sqlite.Models.Preferences;

namespace MySolarCells.Services;

public interface ISettingsService
{
    OnboardingStatusEnum OnboardingStatus { get; set; }
    int SelectedHomeId { get; set; }
    CountryEnum UserCountry { get; set; }
    bool BackgroundSyncEnabled { get; set; }
    int BackgroundSyncIntervalMinutes { get; set; }
    DateTime? LastBackgroundSyncTime { get; set; }
    AppTheme UserTheme { get; set; }
    void SetCurrentCultureOnAllThreads(CountryEnum country);
}

public class SettingsService : ISettingsService
{
    
    private readonly MscDbContext mscDbContext;
    public SettingsService(MscDbContext mscDbContext)
    {
        this.mscDbContext = mscDbContext;
    }
    
    public OnboardingStatusEnum OnboardingStatus
    {
        get
        {
            var exist = mscDbContext.Preferences.FirstOrDefault(x => x.Name == nameof(OnboardingStatus));
            if (exist == null)
                return OnboardingStatusEnum.Unknown;
            else
                return (OnboardingStatusEnum)exist.IntValue;
        }
        set
        {
            var exist = mscDbContext.Preferences.FirstOrDefault(x => x.Name == nameof(OnboardingStatus));
            if (exist == null)
                mscDbContext.Preferences.Add(new Preferences { Name = nameof(OnboardingStatus), IntValue = (int)value });
            else
                exist.IntValue = (int)value;
            mscDbContext.SaveChanges();
        }
    }
    public int SelectedHomeId
    {
        get
        {
            var exist = mscDbContext.Preferences.FirstOrDefault(x => x.Name == nameof(SelectedHomeId));
            if (exist == null)
                return 0;
            else
                return exist.IntValue;
        }
        set
        {
            var exist = mscDbContext.Preferences.FirstOrDefault(x => x.Name == nameof(SelectedHomeId));
            if (exist == null && value != 0)
            {
                mscDbContext.Preferences.Add(new Preferences { Name = nameof(SelectedHomeId), IntValue = value });
                mscDbContext.SaveChanges();
            }
            else if (exist != null)
            {
                exist.IntValue = value;
                mscDbContext.SaveChanges();
            }

            
        }
    }
    public CountryEnum UserCountry
    {
        get
        {
            var exist = mscDbContext.Preferences.FirstOrDefault(x => x.Name == nameof(UserCountry));
            if (exist == null)
                return CountryEnum.EnUs;
            else
                return (CountryEnum)exist.IntValue;
        }
        set
        {
            var exist = mscDbContext.Preferences.FirstOrDefault(x => x.Name == nameof(UserCountry));
            if (exist == null)
                mscDbContext.Preferences.Add(new Preferences { Name = nameof(UserCountry), IntValue = (int)value });
            else
                exist.IntValue = (int)value;
            mscDbContext.SaveChanges();
            SetCurrentCultureOnAllThreads(value);
        }
    }

    public bool BackgroundSyncEnabled
    {
        get
        {
            var exist = mscDbContext.Preferences.FirstOrDefault(x => x.Name == nameof(BackgroundSyncEnabled));
            if (exist == null)
                return true; // Default: enabled
            else
                return exist.IntValue == 1;
        }
        set
        {
            var exist = mscDbContext.Preferences.FirstOrDefault(x => x.Name == nameof(BackgroundSyncEnabled));
            if (exist == null)
                mscDbContext.Preferences.Add(new Preferences { Name = nameof(BackgroundSyncEnabled), IntValue = value ? 1 : 0 });
            else
                exist.IntValue = value ? 1 : 0;
            mscDbContext.SaveChanges();
        }
    }

    public int BackgroundSyncIntervalMinutes
    {
        get
        {
            var exist = mscDbContext.Preferences.FirstOrDefault(x => x.Name == nameof(BackgroundSyncIntervalMinutes));
            if (exist == null)
                return 60; // Default: 60 minutes (1 hour)
            else
                return exist.IntValue;
        }
        set
        {
            var exist = mscDbContext.Preferences.FirstOrDefault(x => x.Name == nameof(BackgroundSyncIntervalMinutes));
            if (exist == null)
                mscDbContext.Preferences.Add(new Preferences { Name = nameof(BackgroundSyncIntervalMinutes), IntValue = value });
            else
                exist.IntValue = value;
            mscDbContext.SaveChanges();
        }
    }

    public DateTime? LastBackgroundSyncTime
    {
        get
        {
            var exist = mscDbContext.Preferences.FirstOrDefault(x => x.Name == nameof(LastBackgroundSyncTime));
            if (exist == null || string.IsNullOrEmpty(exist.StringValue))
                return null;
            else
                return DateTime.Parse(exist.StringValue);
        }
        set
        {
            var exist = mscDbContext.Preferences.FirstOrDefault(x => x.Name == nameof(LastBackgroundSyncTime));
            if (exist == null)
                mscDbContext.Preferences.Add(new Preferences { Name = nameof(LastBackgroundSyncTime), StringValue = value?.ToString("O") ?? "" });
            else
                exist.StringValue = value?.ToString("O") ?? "";
            mscDbContext.SaveChanges();
        }
    }

    public AppTheme UserTheme
    {
        get
        {
            var exist = mscDbContext.Preferences.FirstOrDefault(x => x.Name == nameof(UserTheme));
            if (exist == null)
                return AppTheme.Unspecified; // Default: follow system theme
            else
                return (AppTheme)exist.IntValue;
        }
        set
        {
            var exist = mscDbContext.Preferences.FirstOrDefault(x => x.Name == nameof(UserTheme));
            if (exist == null)
                mscDbContext.Preferences.Add(new Preferences { Name = nameof(UserTheme), IntValue = (int)value });
            else
                exist.IntValue = (int)value;
            mscDbContext.SaveChanges();
        }
    }

    public void SetCurrentCultureOnAllThreads(CountryEnum country)
    {
        switch (country)
        {
            case CountryEnum.SvSe:
            case CountryEnum.Undefined:
            default:
                Thread.CurrentThread.CurrentUICulture = new CultureInfo("sv-SE");
                Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture;
                AppResources.Culture = Thread.CurrentThread.CurrentUICulture;
               
                break;
            case CountryEnum.EnUs:
           
                Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US"); //en-US
                Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture;
                AppResources.Culture = Thread.CurrentThread.CurrentUICulture;
             
                break;
        }

        //MySolarCellsGlobals..ApplicationTitle = AppResources.Libero_Club;
    }

}