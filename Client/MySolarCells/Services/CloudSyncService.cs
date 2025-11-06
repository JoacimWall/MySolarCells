using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace MySolarCells.Services;

public interface ICloudSyncService
{
    Task<CloudSyncResult> BackupDatabaseToCloudAsync(IProgress<double>? progress = null, CancellationToken cancellationToken = default);
    Task<CloudSyncResult> RestoreDatabaseFromCloudAsync(IProgress<double>? progress = null, CancellationToken cancellationToken = default);
    Task<CloudSyncResult> SyncWithCloudAsync(IProgress<double>? progress = null, CancellationToken cancellationToken = default);
    Task<List<CloudBackupInfo>> ListAvailableBackupsAsync(CancellationToken cancellationToken = default);
    Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);
    Task<CloudBackupInfo?> GetLatestBackupInfoAsync(CancellationToken cancellationToken = default);
}

public class CloudSyncService : ICloudSyncService
{
    private readonly ISettingsService settingsService;
    private readonly MscDbContext dbContext;
    private readonly ILogService logService;
    private readonly IInternetConnectionService internetConnectionService;
    private readonly SemaphoreSlim syncSemaphore = new(1, 1);

    public CloudSyncService(
        ISettingsService settingsService,
        MscDbContext dbContext,
        ILogService logService,
        IInternetConnectionService internetConnectionService)
    {
        this.settingsService = settingsService;
        this.dbContext = dbContext;
        this.logService = logService;
        this.internetConnectionService = internetConnectionService;
    }

    public async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!internetConnectionService.InternetAccess())
            {
                return false;
            }

            var connectionString = settingsService.AzureBlobConnectionString;
            var containerName = settingsService.AzureBlobContainerName;

            if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(containerName))
            {
                return false;
            }

            var containerClient = new BlobContainerClient(connectionString, containerName);
            var exists = await containerClient.ExistsAsync(cancellationToken);

            if (!exists)
            {
                await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
            }

            return true;
        }
        catch (Exception ex)
        {
            logService.ReportErrorToAppCenter(ex, "Cloud Sync Connection Test Failed");
            return false;
        }
    }

    public async Task<CloudBackupInfo?> GetLatestBackupInfoAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var backups = await ListAvailableBackupsAsync(cancellationToken);
            return backups.OrderByDescending(b => b.Timestamp).FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<CloudBackupInfo>> ListAvailableBackupsAsync(CancellationToken cancellationToken = default)
    {
        var backups = new List<CloudBackupInfo>();

        try
        {
            if (!internetConnectionService.InternetAccess())
            {
                throw new InvalidOperationException("No internet connection available");
            }

            var connectionString = settingsService.AzureBlobConnectionString;
            var containerName = settingsService.AzureBlobContainerName;

            if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(containerName))
            {
                throw new InvalidOperationException("Cloud sync not configured");
            }

            var containerClient = new BlobContainerClient(connectionString, containerName);

            await foreach (var blobItem in containerClient.GetBlobsAsync(cancellationToken: cancellationToken))
            {
                if (blobItem.Name.EndsWith(".db3"))
                {
                    var parts = blobItem.Name.Split('_');
                    var deviceId = parts.Length > 1 ? parts[0] : "unknown";

                    backups.Add(new CloudBackupInfo
                    {
                        BlobName = blobItem.Name,
                        DeviceId = deviceId,
                        Timestamp = blobItem.Properties.LastModified?.UtcDateTime ?? DateTime.MinValue,
                        SizeBytes = blobItem.Properties.ContentLength ?? 0
                    });
                }
            }
        }
        catch (Exception ex)
        {
            logService.ReportErrorToAppCenter(ex, "Failed to List Cloud Backups");
        }

        return backups.OrderByDescending(b => b.Timestamp).ToList();
    }

    public async Task<CloudSyncResult> BackupDatabaseToCloudAsync(IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        // Ensure only one sync operation at a time
        if (!await syncSemaphore.WaitAsync(0, cancellationToken))
        {
            return new CloudSyncResult
            {
                Success = false,
                Message = "Another sync operation is already in progress"
            };
        }

        try
        {
            progress?.Report(0.1);

            if (!settingsService.CloudSyncEnabled)
            {
                return new CloudSyncResult
                {
                    Success = false,
                    Message = "Cloud sync is not enabled"
                };
            }

            if (!internetConnectionService.InternetAccess())
            {
                return new CloudSyncResult
                {
                    Success = false,
                    Message = "No internet connection available"
                };
            }

            progress?.Report(0.2);

            var connectionString = settingsService.AzureBlobConnectionString;
            var containerName = settingsService.AzureBlobContainerName;
            var deviceId = settingsService.DeviceId;

            if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(containerName))
            {
                return new CloudSyncResult
                {
                    Success = false,
                    Message = "Cloud sync not configured. Please set Azure connection string and container name."
                };
            }

            progress?.Report(0.3);

            // Get the database file path
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "Db_v_2.db3");

            if (!File.Exists(dbPath))
            {
                return new CloudSyncResult
                {
                    Success = false,
                    Message = "Database file not found"
                };
            }

            progress?.Report(0.4);

            // Create blob client
            var containerClient = new BlobContainerClient(connectionString, containerName);
            await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

            progress?.Report(0.5);

            // Create a unique blob name with device ID and timestamp
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var blobName = $"{deviceId}_{timestamp}.db3";
            var blobClient = containerClient.GetBlobClient(blobName);

            progress?.Report(0.6);

            // Upload the database file
            using (var fileStream = File.OpenRead(dbPath))
            {
                await blobClient.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = "application/x-sqlite3" }, cancellationToken: cancellationToken);
            }

            progress?.Report(0.8);

            // Update metadata with device info and timestamp
            var metadata = new Dictionary<string, string>
            {
                { "deviceId", deviceId ?? "unknown" },
                { "backupTime", DateTime.UtcNow.ToString("O") },
                { "appVersion", AppInfo.VersionString },
                { "platform", DeviceInfo.Platform.ToString() }
            };
            await blobClient.SetMetadataAsync(metadata, cancellationToken: cancellationToken);

            progress?.Report(0.9);

            // Clean up old backups - keep last 10 per device
            await CleanupOldBackupsAsync(containerClient, deviceId, cancellationToken);

            // Update last backup time
            settingsService.LastCloudBackupTime = DateTime.Now;

            progress?.Report(1.0);

            logService.ConsoleWriteLineDebug($"Cloud Backup Successful: Database backed up to cloud: {blobName}");

            return new CloudSyncResult
            {
                Success = true,
                Message = "Database successfully backed up to cloud",
                BackupInfo = new CloudBackupInfo
                {
                    BlobName = blobName,
                    DeviceId = deviceId ?? "unknown",
                    Timestamp = DateTime.UtcNow,
                    SizeBytes = new FileInfo(dbPath).Length
                }
            };
        }
        catch (Exception ex)
        {
            logService.ReportErrorToAppCenter(ex, "Cloud Backup Failed");

            return new CloudSyncResult
            {
                Success = false,
                Message = $"Backup failed: {ex.Message}"
            };
        }
        finally
        {
            syncSemaphore.Release();
        }
    }

    public async Task<CloudSyncResult> RestoreDatabaseFromCloudAsync(IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        if (!await syncSemaphore.WaitAsync(0, cancellationToken))
        {
            return new CloudSyncResult
            {
                Success = false,
                Message = "Another sync operation is already in progress"
            };
        }

        try
        {
            progress?.Report(0.1);

            if (!settingsService.CloudSyncEnabled)
            {
                return new CloudSyncResult
                {
                    Success = false,
                    Message = "Cloud sync is not enabled"
                };
            }

            if (!internetConnectionService.InternetAccess())
            {
                return new CloudSyncResult
                {
                    Success = false,
                    Message = "No internet connection available"
                };
            }

            progress?.Report(0.2);

            var connectionString = settingsService.AzureBlobConnectionString;
            var containerName = settingsService.AzureBlobContainerName;

            if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(containerName))
            {
                return new CloudSyncResult
                {
                    Success = false,
                    Message = "Cloud sync not configured"
                };
            }

            progress?.Report(0.3);

            // Get list of all backups
            var backups = await ListAvailableBackupsAsync(cancellationToken);
            if (backups.Count == 0)
            {
                return new CloudSyncResult
                {
                    Success = false,
                    Message = "No backups found in cloud"
                };
            }

            progress?.Report(0.4);

            // Get the latest backup (from any device)
            var latestBackup = backups.OrderByDescending(b => b.Timestamp).First();

            var containerClient = new BlobContainerClient(connectionString, containerName);
            var blobClient = containerClient.GetBlobClient(latestBackup.BlobName);

            progress?.Report(0.5);

            // Get the database file path
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "Db_v_2.db3");
            var backupPath = Path.Combine(FileSystem.AppDataDirectory, $"Db_v_2_backup_{DateTime.Now:yyyyMMdd_HHmmss}.db3");

            // Backup current database
            if (File.Exists(dbPath))
            {
                File.Copy(dbPath, backupPath, true);
            }

            progress?.Report(0.6);

            // Close all database connections
            await dbContext.Database.CloseConnectionAsync();

            progress?.Report(0.7);

            // Download and replace the database
            using (var fileStream = File.Create(dbPath))
            {
                await blobClient.DownloadToAsync(fileStream, cancellationToken);
            }

            progress?.Report(0.9);

            // Update last restore time
            settingsService.LastCloudRestoreTime = DateTime.Now;

            progress?.Report(1.0);

            logService.ConsoleWriteLineDebug($"Cloud Restore Successful: Database restored from cloud: {latestBackup.BlobName}");

            return new CloudSyncResult
            {
                Success = true,
                Message = $"Database successfully restored from cloud backup (from device: {latestBackup.DeviceId})",
                BackupInfo = latestBackup
            };
        }
        catch (Exception ex)
        {
            logService.ReportErrorToAppCenter(ex, "Cloud Restore Failed");

            return new CloudSyncResult
            {
                Success = false,
                Message = $"Restore failed: {ex.Message}"
            };
        }
        finally
        {
            syncSemaphore.Release();
        }
    }

    public async Task<CloudSyncResult> SyncWithCloudAsync(IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            progress?.Report(0.1);

            // Get latest cloud backup info
            var latestCloudBackup = await GetLatestBackupInfoAsync(cancellationToken);

            // Get local database last modified time
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "Db_v_2.db3");
            var localLastModified = File.GetLastWriteTimeUtc(dbPath);

            progress?.Report(0.5);

            // Decision logic: Which is newer?
            if (latestCloudBackup == null)
            {
                // No cloud backup exists, upload current database
                progress?.Report(0.6);
                return await BackupDatabaseToCloudAsync(progress, cancellationToken);
            }
            else if (latestCloudBackup.Timestamp > localLastModified)
            {
                // Cloud is newer, download from cloud
                progress?.Report(0.6);
                return await RestoreDatabaseFromCloudAsync(progress, cancellationToken);
            }
            else
            {
                // Local is newer or same, upload to cloud
                progress?.Report(0.6);
                return await BackupDatabaseToCloudAsync(progress, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logService.ReportErrorToAppCenter(ex, "Cloud Sync Failed");

            return new CloudSyncResult
            {
                Success = false,
                Message = $"Sync failed: {ex.Message}"
            };
        }
    }

    private async Task CleanupOldBackupsAsync(BlobContainerClient containerClient, string? deviceId, CancellationToken cancellationToken)
    {
        try
        {
            var allBackups = new List<BlobItem>();

            await foreach (var blobItem in containerClient.GetBlobsAsync(cancellationToken: cancellationToken))
            {
                if (blobItem.Name.StartsWith($"{deviceId}_") && blobItem.Name.EndsWith(".db3"))
                {
                    allBackups.Add(blobItem);
                }
            }

            // Keep only the latest 10 backups per device
            var backupsToDelete = allBackups
                .OrderByDescending(b => b.Properties.LastModified)
                .Skip(10)
                .ToList();

            foreach (var backup in backupsToDelete)
            {
                var blobClient = containerClient.GetBlobClient(backup.Name);
                await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
            }
        }
        catch (Exception ex)
        {
            // Log but don't fail the backup operation if cleanup fails
            logService.ConsoleWriteLineDebug($"Cloud Backup Cleanup Warning: Failed to clean up old backups: {ex.Message}");
        }
    }
}

public class CloudSyncResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public CloudBackupInfo? BackupInfo { get; set; }
}

public class CloudBackupInfo
{
    public string BlobName { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public long SizeBytes { get; set; }

    public string FormattedSize
    {
        get
        {
            if (SizeBytes < 1024)
                return $"{SizeBytes} B";
            else if (SizeBytes < 1024 * 1024)
                return $"{SizeBytes / 1024.0:F2} KB";
            else
                return $"{SizeBytes / (1024.0 * 1024.0):F2} MB";
        }
    }

    public string FormattedTimestamp => Timestamp.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
}

