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

    // Cloud Sync Settings
    bool CloudSyncEnabled { get; set; }
    string? AzureBlobConnectionString { get; set; }
    string? AzureBlobContainerName { get; set; }
    DateTime? LastCloudBackupTime { get; set; }
    DateTime? LastCloudRestoreTime { get; set; }
    string? DeviceId { get; set; }
    bool AutoCloudBackupEnabled { get; set; }
    int CloudBackupIntervalMinutes { get; set; }

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

    // Cloud Sync Settings Implementation
    public bool CloudSyncEnabled
    {
        get
        {
            var exist = mscDbContext.Preferences.FirstOrDefault(x => x.Name == nameof(CloudSyncEnabled));
            if (exist == null)
                return false; // Default: disabled until configured
            else
                return exist.IntValue == 1;
        }
        set
        {
            var exist = mscDbContext.Preferences.FirstOrDefault(x => x.Name == nameof(CloudSyncEnabled));
            if (exist == null)
                mscDbContext.Preferences.Add(new Preferences { Name = nameof(CloudSyncEnabled), IntValue = value ? 1 : 0 });
            else
                exist.IntValue = value ? 1 : 0;
            mscDbContext.SaveChanges();
        }
    }

    public string? AzureBlobConnectionString
    {
        get
        {
            var exist = mscDbContext.Preferences.FirstOrDefault(x => x.Name == nameof(AzureBlobConnectionString));
            return exist?.StringValue;
        }
        set
        {
            var exist = mscDbContext.Preferences.FirstOrDefault(x => x.Name == nameof(AzureBlobConnectionString));
            if (exist == null)
                mscDbContext.Preferences.Add(new Preferences { Name = nameof(AzureBlobConnectionString), StringValue = value ?? "" });
            else
                exist.StringValue = value ?? "";
            mscDbContext.SaveChanges();
        }
    }

    public string? AzureBlobContainerName
    {
        get
        {
            var exist = mscDbContext.Preferences.FirstOrDefault(x => x.Name == nameof(AzureBlobContainerName));
            if (exist == null || string.IsNullOrEmpty(exist.StringValue))
                return "mysolarcells-backup"; // Default container name
            return exist.StringValue;
        }
        set
        {
            var exist = mscDbContext.Preferences.FirstOrDefault(x => x.Name == nameof(AzureBlobContainerName));
            if (exist == null)
                mscDbContext.Preferences.Add(new Preferences { Name = nameof(AzureBlobContainerName), StringValue = value ?? "mysolarcells-backup" });
            else
                exist.StringValue = value ?? "mysolarcells-backup";
            mscDbContext.SaveChanges();
        }
    }

    public DateTime? LastCloudBackupTime
    {
        get
        {
            var exist = mscDbContext.Preferences.FirstOrDefault(x => x.Name == nameof(LastCloudBackupTime));
            if (exist == null || string.IsNullOrEmpty(exist.StringValue))
                return null;
            else
                return DateTime.Parse(exist.StringValue);
        }
        set
        {
            var exist = mscDbContext.Preferences.FirstOrDefault(x => x.Name == nameof(LastCloudBackupTime));
            if (exist == null)
                mscDbContext.Preferences.Add(new Preferences { Name = nameof(LastCloudBackupTime), StringValue = value?.ToString("O") ?? "" });
            else
                exist.StringValue = value?.ToString("O") ?? "";
            mscDbContext.SaveChanges();
        }
    }

    public DateTime? LastCloudRestoreTime
    {
        get
        {
            var exist = mscDbContext.Preferences.FirstOrDefault(x => x.Name == nameof(LastCloudRestoreTime));
            if (exist == null || string.IsNullOrEmpty(exist.StringValue))
                return null;
            else
                return DateTime.Parse(exist.StringValue);
        }
        set
        {
            var exist = mscDbContext.Preferences.FirstOrDefault(x => x.Name == nameof(LastCloudRestoreTime));
            if (exist == null)
                mscDbContext.Preferences.Add(new Preferences { Name = nameof(LastCloudRestoreTime), StringValue = value?.ToString("O") ?? "" });
            else
                exist.StringValue = value?.ToString("O") ?? "";
            mscDbContext.SaveChanges();
        }
    }

    public string? DeviceId
    {
        get
        {
            var exist = mscDbContext.Preferences.FirstOrDefault(x => x.Name == nameof(DeviceId));
            if (exist == null || string.IsNullOrEmpty(exist.StringValue))
            {
                // Generate a new device ID if not exists
                var newDeviceId = Guid.NewGuid().ToString();
                DeviceId = newDeviceId;
                return newDeviceId;
            }
            return exist.StringValue;
        }
        set
        {
            var exist = mscDbContext.Preferences.FirstOrDefault(x => x.Name == nameof(DeviceId));
            if (exist == null)
                mscDbContext.Preferences.Add(new Preferences { Name = nameof(DeviceId), StringValue = value ?? "" });
            else
                exist.StringValue = value ?? "";
            mscDbContext.SaveChanges();
        }
    }

    public bool AutoCloudBackupEnabled
    {
        get
        {
            var exist = mscDbContext.Preferences.FirstOrDefault(x => x.Name == nameof(AutoCloudBackupEnabled));
            if (exist == null)
                return false; // Default: disabled
            else
                return exist.IntValue == 1;
        }
        set
        {
            var exist = mscDbContext.Preferences.FirstOrDefault(x => x.Name == nameof(AutoCloudBackupEnabled));
            if (exist == null)
                mscDbContext.Preferences.Add(new Preferences { Name = nameof(AutoCloudBackupEnabled), IntValue = value ? 1 : 0 });
            else
                exist.IntValue = value ? 1 : 0;
            mscDbContext.SaveChanges();
        }
    }

    public int CloudBackupIntervalMinutes
    {
        get
        {
            var exist = mscDbContext.Preferences.FirstOrDefault(x => x.Name == nameof(CloudBackupIntervalMinutes));
            if (exist == null)
                return 360; // Default: 6 hours
            else
                return exist.IntValue;
        }
        set
        {
            var exist = mscDbContext.Preferences.FirstOrDefault(x => x.Name == nameof(CloudBackupIntervalMinutes));
            if (exist == null)
                mscDbContext.Preferences.Add(new Preferences { Name = nameof(CloudBackupIntervalMinutes), IntValue = value });
            else
                exist.IntValue = value;
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