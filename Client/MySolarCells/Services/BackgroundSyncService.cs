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
    private PeriodicTimer? periodicTimer;
    private CancellationTokenSource? cancellationTokenSource;
    private Task? backgroundTask;
    private bool isRunning;
    private readonly SemaphoreSlim syncLock = new SemaphoreSlim(1, 1);

    public bool IsRunning => isRunning;

    public BackgroundSyncService(
        IDataSyncService dataSyncService,
        ISettingsService settingsService,
        ILogService logService,
        IInternetConnectionService internetConnectionService)
    {
        this.dataSyncService = dataSyncService;
        this.settingsService = settingsService;
        this.logService = logService;
        this.internetConnectionService = internetConnectionService;
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
            if (!await internetConnectionService.IsConnected())
            {
                logService.ConsoleWriteLineDebug("No internet connection, skipping sync");
                return;
            }

            // Perform the sync
            var result = await dataSyncService.Sync(syncGaps: false);

            if (result.WasSuccessful)
            {
                logService.ConsoleWriteLineDebug($"Background sync completed successfully: {result.Value?.Message}");

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

    public void Dispose()
    {
        StopBackgroundSync();
        cancellationTokenSource?.Dispose();
        syncLock?.Dispose();
    }
}
