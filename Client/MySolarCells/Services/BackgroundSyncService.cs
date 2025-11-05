namespace MySolarCells.Services;

public interface IBackgroundSyncService
{
    void StartBackgroundSync();
    void StopBackgroundSync();
    bool IsRunning { get; }
}

public class BackgroundSyncService : IBackgroundSyncService, IDisposable
{
    private readonly IDataSyncService dataSyncService;
    private readonly ISettingsService settingsService;
    private readonly ILogService logService;
    private readonly IInternetConnectionService internetConnectionService;
    private readonly ICloudSyncService cloudSyncService;
    private PeriodicTimer? periodicTimer;
    private PeriodicTimer? cloudBackupTimer;
    private CancellationTokenSource? cancellationTokenSource;
    private Task? backgroundTask;
    private Task? cloudBackupTask;
    private bool isRunning;
    private readonly SemaphoreSlim syncLock = new SemaphoreSlim(1, 1);
    private readonly SemaphoreSlim cloudSyncLock = new SemaphoreSlim(1, 1);

    public bool IsRunning => isRunning;

    public BackgroundSyncService(
        IDataSyncService dataSyncService,
        ISettingsService settingsService,
        ILogService logService,
        IInternetConnectionService internetConnectionService,
        ICloudSyncService cloudSyncService)
    {
        this.dataSyncService = dataSyncService;
        this.settingsService = settingsService;
        this.logService = logService;
        this.internetConnectionService = internetConnectionService;
        this.cloudSyncService = cloudSyncService;
    }

    public void StartBackgroundSync()
    {
        // Check if background sync is enabled in settings
        if (!settingsService.BackgroundSyncEnabled)
        {
            logService.ConsoleWriteLineDebug("Background sync is disabled in settings");
            return;
        }

        // Check if already running
        if (isRunning)
        {
            logService.ConsoleWriteLineDebug("Background sync is already running");
            return;
        }

        // Check if app has completed onboarding
        if (settingsService.OnboardingStatus < OnboardingStatusEnum.OnboardingDone)
        {
            logService.ConsoleWriteLineDebug("Background sync skipped - onboarding not complete");
            return;
        }

        try
        {
            isRunning = true;
            cancellationTokenSource = new CancellationTokenSource();

            // Get sync interval in minutes from settings (default: 60 minutes)
            var intervalMinutes = settingsService.BackgroundSyncIntervalMinutes;
            periodicTimer = new PeriodicTimer(TimeSpan.FromMinutes(intervalMinutes));

            // Start the background task
            backgroundTask = Task.Run(async () => await BackgroundSyncLoop(cancellationTokenSource.Token));

            logService.ConsoleWriteLineDebug($"Background sync started with interval: {intervalMinutes} minutes");

            // Start cloud backup timer if enabled
            if (settingsService.AutoCloudBackupEnabled && settingsService.CloudSyncEnabled)
            {
                var cloudIntervalMinutes = settingsService.CloudBackupIntervalMinutes;
                cloudBackupTimer = new PeriodicTimer(TimeSpan.FromMinutes(cloudIntervalMinutes));
                cloudBackupTask = Task.Run(async () => await CloudBackupLoop(cancellationTokenSource.Token));
                logService.ConsoleWriteLineDebug($"Cloud backup started with interval: {cloudIntervalMinutes} minutes");
            }
        }
        catch (Exception ex)
        {
            logService.ConsoleWriteLineDebug($"Failed to start background sync: {ex.Message}");
            isRunning = false;
        }
    }

    public void StopBackgroundSync()
    {
        if (!isRunning)
        {
            return;
        }

        try
        {
            cancellationTokenSource?.Cancel();
            periodicTimer?.Dispose();
            periodicTimer = null;
            cloudBackupTimer?.Dispose();
            cloudBackupTimer = null;
            isRunning = false;

            logService.ConsoleWriteLineDebug("Background sync stopped");
        }
        catch (Exception ex)
        {
            logService.ConsoleWriteLineDebug($"Error stopping background sync: {ex.Message}");
        }
    }

    private async Task BackgroundSyncLoop(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested && periodicTimer != null)
            {
                try
                {
                    // Wait for the next tick
                    await periodicTimer.WaitForNextTickAsync(cancellationToken);

                    // Check if sync is still enabled (user might have disabled it)
                    if (!settingsService.BackgroundSyncEnabled)
                    {
                        logService.ConsoleWriteLineDebug("Background sync disabled by user, stopping...");
                        StopBackgroundSync();
                        break;
                    }

                    // Execute sync
                    await ExecuteSyncAsync(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // Expected when stopping the service
                    break;
                }
                catch (Exception ex)
                {
                    logService.ConsoleWriteLineDebug($"Error in background sync loop: {ex.Message}");
                    // Continue the loop even if sync fails
                }
            }
        }
        catch (Exception ex)
        {
            logService.ConsoleWriteLineDebug($"Background sync loop terminated: {ex.Message}");
        }
        finally
        {
            isRunning = false;
        }
    }

    private async Task ExecuteSyncAsync(CancellationToken cancellationToken)
    {
        // Use semaphore to prevent concurrent sync operations
        if (!await syncLock.WaitAsync(0, cancellationToken))
        {
            logService.ConsoleWriteLineDebug("Sync already in progress, skipping this cycle");
            return;
        }

        try
        {
            logService.ConsoleWriteLineDebug("Background sync started");

            // Check internet connection
            if (!internetConnectionService.InternetAccess())
            {
                logService.ConsoleWriteLineDebug("No internet connection, skipping sync");
                return;
            }

            // Perform the sync
            var result = await dataSyncService.Sync(syncGaps: false);

            if (result.WasSuccessful)
            {
                logService.ConsoleWriteLineDebug($"Background sync completed successfully: {result.ErrorMessage}");

                // Update last sync time in settings
                settingsService.LastBackgroundSyncTime = DateTime.Now;
            }
            else
            {
                logService.ConsoleWriteLineDebug($"Background sync failed: {result.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            logService.ConsoleWriteLineDebug($"Background sync error: {ex.Message}");
        }
        finally
        {
            syncLock.Release();
        }
    }

    private async Task CloudBackupLoop(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested && cloudBackupTimer != null)
            {
                try
                {
                    // Wait for the next tick
                    await cloudBackupTimer.WaitForNextTickAsync(cancellationToken);

                    // Check if cloud backup is still enabled
                    if (!settingsService.AutoCloudBackupEnabled || !settingsService.CloudSyncEnabled)
                    {
                        logService.ConsoleWriteLineDebug("Cloud backup disabled by user, stopping...");
                        break;
                    }

                    // Execute cloud backup
                    await ExecuteCloudBackupAsync(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // Expected when stopping the service
                    break;
                }
                catch (Exception ex)
                {
                    logService.ConsoleWriteLineDebug($"Error in cloud backup loop: {ex.Message}");
                    // Continue the loop even if backup fails
                }
            }
        }
        catch (Exception ex)
        {
            logService.ConsoleWriteLineDebug($"Cloud backup loop terminated: {ex.Message}");
        }
    }

    private async Task ExecuteCloudBackupAsync(CancellationToken cancellationToken)
    {
        // Use semaphore to prevent concurrent cloud sync operations
        if (!await cloudSyncLock.WaitAsync(0, cancellationToken))
        {
            logService.ConsoleWriteLineDebug("Cloud backup already in progress, skipping this cycle");
            return;
        }

        try
        {
            logService.ConsoleWriteLineDebug("Background cloud backup started");

            // Check internet connection
            if (!internetConnectionService.InternetAccess())
            {
                logService.ConsoleWriteLineDebug("No internet connection, skipping cloud backup");
                return;
            }

            // Perform cloud backup
            var result = await cloudSyncService.BackupDatabaseToCloudAsync(cancellationToken: cancellationToken);

            if (result.Success)
            {
                logService.ConsoleWriteLineDebug($"Background cloud backup completed successfully: {result.Message}");
            }
            else
            {
                logService.ConsoleWriteLineDebug($"Background cloud backup failed: {result.Message}");
            }
        }
        catch (Exception ex)
        {
            logService.ConsoleWriteLineDebug($"Background cloud backup error: {ex.Message}");
        }
        finally
        {
            cloudSyncLock.Release();
        }
    }

    public void Dispose()
    {
        StopBackgroundSync();
        cancellationTokenSource?.Dispose();
        syncLock?.Dispose();
        cloudSyncLock?.Dispose();
    }
}
