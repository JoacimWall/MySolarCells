using CommunityToolkit.Mvvm.ComponentModel;

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

    public override async Task OnPageAppearing()
    {
        await base.OnPageAppearing();
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

    public ICommand TestConnectionCommand => new Command(async () => await TestConnection());
    public ICommand BackupNowCommand => new Command(async () => await BackupNow());
    public ICommand RestoreFromCloudCommand => new Command(async () => await RestoreFromCloud());

    private async Task TestConnection()
    {
        if (IsTestingConnection) return;

        try
        {
            IsTestingConnection = true;

            if (string.IsNullOrEmpty(AzureConnectionString))
            {
                await DialogService.AlertAsync(AppResources.Connection_Failed, "Please enter a connection string", AppResources.Ok);
                return;
            }

            var success = await cloudSyncService.TestConnectionAsync();

            if (success)
            {
                await DialogService.AlertAsync(AppResources.Connection_Successful, "", AppResources.Ok);
            }
            else
            {
                await DialogService.AlertAsync(AppResources.Connection_Failed, "", AppResources.Ok);
            }
        }
        catch (Exception ex)
        {
            await DialogService.AlertAsync(AppResources.Connection_Failed, ex.Message, AppResources.Ok);
            await LogService.AddLogAsync(new MySolarCellsSQLite.Sqlite.Models.Log
            {
                LogTitle = "Cloud Connection Test Error",
                LogDetails = ex.Message,
                CreateDate = DateTime.Now,
                LogTyp = LogTypeEnum.Error
            });
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
                await DialogService.AlertAsync("Cloud Sync Disabled", "Please enable cloud sync first", AppResources.Ok);
                return;
            }

            if (string.IsNullOrEmpty(AzureConnectionString))
            {
                await DialogService.AlertAsync("Configuration Required", "Please configure Azure connection string", AppResources.Ok);
                return;
            }

            var progress = new Progress<double>(value =>
            {
                // Update progress if needed
            });

            var result = await cloudSyncService.BackupDatabaseToCloudAsync(progress);

            if (result.Success)
            {
                await DialogService.AlertAsync(AppResources.Backup_Successful, result.Message, AppResources.Ok);
                LoadSettings(); // Refresh the UI
            }
            else
            {
                await DialogService.AlertAsync(string.Format(AppResources.Backup_Failed, result.Message), "", AppResources.Ok);
            }
        }
        catch (Exception ex)
        {
            await DialogService.AlertAsync(string.Format(AppResources.Backup_Failed, ex.Message), "", AppResources.Ok);
            await LogService.AddLogAsync(new MySolarCellsSQLite.Sqlite.Models.Log
            {
                LogTitle = "Cloud Backup Error",
                LogDetails = ex.Message,
                CreateDate = DateTime.Now,
                LogTyp = LogTypeEnum.Error
            });
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
                await DialogService.AlertAsync("Cloud Sync Disabled", "Please enable cloud sync first", AppResources.Ok);
                return;
            }

            if (string.IsNullOrEmpty(AzureConnectionString))
            {
                await DialogService.AlertAsync("Configuration Required", "Please configure Azure connection string", AppResources.Ok);
                return;
            }

            var progress = new Progress<double>(value =>
            {
                // Update progress if needed
            });

            var result = await cloudSyncService.RestoreDatabaseFromCloudAsync(progress);

            if (result.Success)
            {
                await DialogService.AlertAsync(AppResources.Restore_Successful, result.Message, AppResources.Ok);

                // Restart the app
                if (Application.Current != null)
                {
                    Application.Current.MainPage = new AppShell();
                }
            }
            else
            {
                await DialogService.AlertAsync(string.Format(AppResources.Restore_Failed, result.Message), "", AppResources.Ok);
            }
        }
        catch (Exception ex)
        {
            await DialogService.AlertAsync(string.Format(AppResources.Restore_Failed, ex.Message), "", AppResources.Ok);
            await LogService.AddLogAsync(new MySolarCellsSQLite.Sqlite.Models.Log
            {
                LogTitle = "Cloud Restore Error",
                LogDetails = ex.Message,
                CreateDate = DateTime.Now,
                LogTyp = LogTypeEnum.Error
            });
        }
        finally
        {
            IsRestoring = false;
        }
    }
}
