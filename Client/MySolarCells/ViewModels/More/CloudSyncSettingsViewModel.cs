
namespace MySolarCells.ViewModels.More;

public partial class CloudSyncSettingsViewModel : BaseViewModel
{
    private readonly ICloudSyncService cloudSyncService;

    [ObservableProperty]
    private bool cloudSyncEnabled;

    [ObservableProperty]
    private string? azureConnectionString;

    [ObservableProperty]
    private string? azureContainerName;

    [ObservableProperty]
    private bool autoCloudBackupEnabled;

    [ObservableProperty]
    private int cloudBackupIntervalHours;

    [ObservableProperty]
    private string? deviceId;

    [ObservableProperty]
    private string lastBackupText = AppResources.Never;

    [ObservableProperty]
    private string lastRestoreText = AppResources.Never;

    [ObservableProperty]
    private bool isTestingConnection;

    [ObservableProperty]
    private bool isBackingUp;

    [ObservableProperty]
    private bool isRestoring;

    public CloudSyncSettingsViewModel(
        IDialogService dialogService,
        IAnalyticsService analyticsService,
        IInternetConnectionService internetConnectionService,
        ILogService logService,
        ISettingsService settingsService,
        IHomeService homeService,
        ICloudSyncService cloudSyncService)
        : base(dialogService, analyticsService, internetConnectionService, logService, settingsService, homeService)
    {
        this.cloudSyncService = cloudSyncService;
    }

    public override async Task OnAppearingAsync()
    {
        await base.OnAppearingAsync();
        LoadSettings();
    }


    private void LoadSettings()
    {
        CloudSyncEnabled = SettingsService.CloudSyncEnabled;
        AzureConnectionString = SettingsService.AzureBlobConnectionString;
        AzureContainerName = SettingsService.AzureBlobContainerName;
        AutoCloudBackupEnabled = SettingsService.AutoCloudBackupEnabled;
        CloudBackupIntervalHours = SettingsService.CloudBackupIntervalMinutes / 60;
        DeviceId = SettingsService.DeviceId;

        var lastBackup = SettingsService.LastCloudBackupTime;
        LastBackupText = lastBackup.HasValue
            ? lastBackup.Value.ToString("yyyy-MM-dd HH:mm:ss")
            : AppResources.Never;

        var lastRestore = SettingsService.LastCloudRestoreTime;
        LastRestoreText = lastRestore.HasValue
            ? lastRestore.Value.ToString("yyyy-MM-dd HH:mm:ss")
            : AppResources.Never;
    }

    partial void OnCloudSyncEnabledChanged(bool value)
    {
        SettingsService.CloudSyncEnabled = value;
    }

    partial void OnAzureConnectionStringChanged(string? value)
    {
        SettingsService.AzureBlobConnectionString = value;
    }

    partial void OnAzureContainerNameChanged(string? value)
    {
        SettingsService.AzureBlobContainerName = value;
    }

    partial void OnAutoCloudBackupEnabledChanged(bool value)
    {
        SettingsService.AutoCloudBackupEnabled = value;
    }

    partial void OnCloudBackupIntervalHoursChanged(int value)
    {
        SettingsService.CloudBackupIntervalMinutes = value * 60;
    }

    public ICommand TestConnectionCommand => new Command(async () =>
    {
        try
        {
            await TestConnection();
        }
        catch (Exception ex)
        {
            LogService.ReportErrorToAppCenter(ex, "TestConnectionCommand");
        }
    });
    
    public ICommand BackupNowCommand => new Command(async () =>
    {
        try
        {
            await BackupNow();
        }
        catch (Exception ex)
        {
            LogService.ReportErrorToAppCenter(ex, "BackupNowCommand");
        }
    });
    
    public ICommand RestoreFromCloudCommand => new Command(async () =>
    {
        try
        {
            await RestoreFromCloud();
        }
        catch (Exception ex)
        {
            LogService.ReportErrorToAppCenter(ex, "RestoreFromCloudCommand");
        }
    });

    private async Task TestConnection()
    {
        if (IsTestingConnection) return;

        try
        {
            IsTestingConnection = true;

            if (string.IsNullOrEmpty(AzureConnectionString))
            {
                await DialogService.ShowAlertAsync("Please enter a connection string", AppResources.Connection_Failed, AppResources.Ok);
                return;
            }

            var success = await cloudSyncService.TestConnectionAsync();

            if (success)
            {
                await DialogService.ShowAlertAsync("", AppResources.Connection_Successful, AppResources.Ok);
            }
            else
            {
                await DialogService.ShowAlertAsync("", AppResources.Connection_Failed, AppResources.Ok);
            }
        }
        catch (Exception ex)
        {
            await DialogService.ShowAlertAsync(ex.Message, AppResources.Connection_Failed, AppResources.Ok);
            LogService.ReportErrorToAppCenter(ex, "Cloud Connection Test Error");
        }
        finally
        {
            IsTestingConnection = false;
        }
    }

    private async Task BackupNow()
    {
        if (IsBackingUp) return;

        try
        {
            IsBackingUp = true;

            if (!CloudSyncEnabled)
            {
                await DialogService.ShowAlertAsync("Please enable cloud sync first", "Cloud Sync Disabled", AppResources.Ok);
                return;
            }

            if (string.IsNullOrEmpty(AzureConnectionString))
            {
                await DialogService.ShowAlertAsync("Please configure Azure connection string", "Configuration Required", AppResources.Ok);
                return;
            }

            var progress = new Progress<double>(_ =>
            {
                // Update progress if needed
            });

            var result = await cloudSyncService.BackupDatabaseToCloudAsync(progress);

            if (result.Success)
            {
                await DialogService.ShowAlertAsync(result.Message, AppResources.Backup_Successful, AppResources.Ok);
                LoadSettings(); // Refresh the UI
            }
            else
            {
                await DialogService.ShowAlertAsync("", string.Format(AppResources.Backup_Failed, result.Message), AppResources.Ok);
            }
        }
        catch (Exception ex)
        {
            await DialogService.ShowAlertAsync("", string.Format(AppResources.Backup_Failed, ex.Message), AppResources.Ok);
            LogService.ReportErrorToAppCenter(ex, "Cloud Backup Error");
        }
        finally
        {
            IsBackingUp = false;
        }
    }

    private async Task RestoreFromCloud()
    {
        if (IsRestoring) return;

        try
        {
            // Confirm with user
            var confirm = await DialogService.ConfirmAsync(
                "Restore Database",
                "This will replace your current database with the latest cloud backup. The app will need to restart. Continue?",
                AppResources.Ok,
                AppResources.Cancel);

            if (!confirm)
                return;

            IsRestoring = true;

            if (!CloudSyncEnabled)
            {
                await DialogService.ShowAlertAsync("Please enable cloud sync first", "Cloud Sync Disabled", AppResources.Ok);
                return;
            }

            if (string.IsNullOrEmpty(AzureConnectionString))
            {
                await DialogService.ShowAlertAsync("Please configure Azure connection string", "Configuration Required", AppResources.Ok);
                return;
            }

            var progress = new Progress<double>(_ =>
            {
                // Update progress if needed
            });

            var result = await cloudSyncService.RestoreDatabaseFromCloudAsync(progress);

            if (result.Success)
            {
                await DialogService.ShowAlertAsync(result.Message, AppResources.Restore_Successful, AppResources.Ok);

                // Restart the app by replacing the window's page
                if (Application.Current?.Windows.Count > 0)
                {
                    Application.Current.Windows[0].Page = new AppShell();
                }
            }
            else
            {
                await DialogService.ShowAlertAsync("", string.Format(AppResources.Restore_Failed, result.Message), AppResources.Ok);
            }
        }
        catch (Exception ex)
        {
            await DialogService.ShowAlertAsync("", string.Format(AppResources.Restore_Failed, ex.Message), AppResources.Ok);
            LogService.ReportErrorToAppCenter(ex, "Cloud Restore Error");
        }
        finally
        {
            IsRestoring = false;
        }
    }
}
