using System.Globalization;

namespace MySolarCells.Services;

public interface ISettingsService
{
    OnboardingStatusEnum OnboardingStatus { get; set; }
    int SelectedHomeId { get; set; }
    CountryEnum UserCountry { get; set; }
    void SetCurentCultureOnAllThreds(CountryEnum country);
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
            var exist = this.mscDbContext.Preferences.FirstOrDefault(x => x.Name == nameof(OnboardingStatus));
            if (exist == null)
                return OnboardingStatusEnum.Unknown;
            else
                return (OnboardingStatusEnum)exist.IntValue;
        }
        set
        {
            var exist = this.mscDbContext.Preferences.FirstOrDefault(x => x.Name == nameof(OnboardingStatus));
            if (exist == null)
                this.mscDbContext.Preferences.Add(new SQLite.Sqlite.Models.Preferences { Name = nameof(OnboardingStatus), IntValue = (int)value });
            else
                exist.IntValue = (int)value;
            this.mscDbContext.SaveChanges();
        }
    }
    public int SelectedHomeId
    {
        get
        {
            var exist = this.mscDbContext.Preferences.FirstOrDefault(x => x.Name == nameof(SelectedHomeId));
            if (exist == null)
                return 0;
            else
                return exist.IntValue;
        }
        set
        {
            var exist = this.mscDbContext.Preferences.FirstOrDefault(x => x.Name == nameof(SelectedHomeId));
            if (exist == null && value != 0)
            {
                this.mscDbContext.Preferences.Add(new SQLite.Sqlite.Models.Preferences { Name = nameof(SelectedHomeId), IntValue = (int)value });
                this.mscDbContext.SaveChanges();
            }
            else if (exist != null)
            {
                exist.IntValue = (int)value;
                this.mscDbContext.SaveChanges();
            }

            
        }
    }
    public CountryEnum UserCountry
    {
        get
        {
            var exist = this.mscDbContext.Preferences.FirstOrDefault(x => x.Name == nameof(UserCountry));
            if (exist == null)
                return CountryEnum.En_US;
            else
                return (CountryEnum)exist.IntValue;
        }
        set
        {
            var exist = this.mscDbContext.Preferences.FirstOrDefault(x => x.Name == nameof(UserCountry));
            if (exist == null)
                this.mscDbContext.Preferences.Add(new SQLite.Sqlite.Models.Preferences { Name = nameof(UserCountry), IntValue = (int)value });
            else
                exist.IntValue = (int)value;
            this.mscDbContext.SaveChanges();
            SetCurentCultureOnAllThreds(value);
        }
    }

    public void SetCurentCultureOnAllThreds(CountryEnum country)
    {
        switch (country)
        {
            case CountryEnum.Sv_SE:
            case CountryEnum.Undefined:
            default:
                Thread.CurrentThread.CurrentUICulture = new CultureInfo("sv-SE");
                Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture;
                AppResources.Culture = Thread.CurrentThread.CurrentUICulture;
               
                break;
            case CountryEnum.En_US:
           
                Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US"); //en-US
                Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture;
                AppResources.Culture = Thread.CurrentThread.CurrentUICulture;
             
                break;
        }

        //MySolarCellsGlobals..ApplicationTitle = AppResources.Libero_Club;
    }

}