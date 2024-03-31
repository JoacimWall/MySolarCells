using Preferences = MySolarCellsSQLite.Sqlite.Models.Preferences;

namespace MySolarCells.Services;

public interface ISettingsService
{
    OnboardingStatusEnum OnboardingStatus { get; set; }
    int SelectedHomeId { get; set; }
    CountryEnum UserCountry { get; set; }
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