using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyApp.Core.Entities;
using MyApp.Core.Interfaces;
using MyApp.Infrastructure.Data;
using MySql.Data.MySqlClient;
using MySqlConnector;
using System.IO.Compression;

namespace MyApp.Infrastructure.Services
{
    public class BackupService : IBackupService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<BackupService> _logger;
        private readonly string _connectionString;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public BackupService(
            IConfiguration configuration,
            ILogger<BackupService> logger,
            IDbContextFactory<AppDbContext> contextFactory)
        {
            _configuration = configuration;
            _logger = logger;
            _contextFactory = contextFactory;
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        public async Task<BackupHistory> ExecuteBackupAsync(int? scheduleId, int? userId, string backupType = "Manual", int? backupHistoryId = null)
        {
            BackupSchedule? schedule = null;
            int backupId;

            try
            {
                // Get schedule configuration
                if (scheduleId.HasValue)
                {
                    await using var readContext = await _contextFactory.CreateDbContextAsync();
                    schedule = await readContext.BackupSchedules
                        .AsNoTracking()
                        .FirstOrDefaultAsync(s => s.Id == scheduleId.Value);
                }

                var backupPath = schedule?.BackupPath ?? _configuration["Backup:DefaultPath"] ?? "./backups";
                var includeSchema = schedule?.IncludeSchema ?? true;
                var includeData = schedule?.IncludeData ?? true;
                var useCompression = bool.Parse(_configuration["Backup:UseCompression"] ?? "false");

                // Ensure backup directory exists
                var fullBackupPath = Path.GetFullPath(backupPath);
                if (!Directory.Exists(fullBackupPath))
                {
                    Directory.CreateDirectory(fullBackupPath);
                    _logger.LogInformation($"✅ Created backup directory: {fullBackupPath}");
                }

                var startTime = DateTime.Now;
                var fileName = $"backup_{DateTime.Now:yyyyMMdd_HHmmss}.sql";
                var filePath = Path.Combine(fullBackupPath, fileName);

                _logger.LogInformation($"🔄 Starting backup using MySqlBackup.NET");
                _logger.LogInformation($"   📊 Include Schema: {includeSchema}");
                _logger.LogInformation($"   📊 Include Data: {includeData}");
                _logger.LogInformation($"   🗜️  Compression: {useCompression}");
                _logger.LogInformation($"   📁 Path: {filePath}");

                // Step 1: Use existing backup history if backupHistoryId is provided
                if (backupHistoryId.HasValue)
                {
                    backupId = backupHistoryId.Value;
                    await using (var context = await _contextFactory.CreateDbContextAsync())
                    {
                        var history = await context.BackupHistories.FindAsync(backupId);
                        if (history != null)
                        {
                            history.Status = "In Progress";
                            history.StartedAt = startTime;
                            history.FileName = fileName;
                            history.FilePath = filePath;
                            await context.SaveChangesAsync();
                        }
                    }
                }
                else
                {
                    // Create new backup history record (manual/fallback mode)
                    await using (var context = await _contextFactory.CreateDbContextAsync())
                    {
                        var history = new BackupHistory
                        {
                            BackupScheduleId = scheduleId,
                            BackupType = backupType,
                            FileName = fileName,
                            FilePath = filePath,
                            FileSize = 0,
                            Status = "In Progress",
                            StartedAt = startTime,
                            CreatedBy = userId
                        };

                        context.BackupHistories.Add(history);
                        await context.SaveChangesAsync();
                        backupId = history.Id;
                    }
                }

                try
                {
                    // Step 2: Perform backup using MySqlBackup.NET
                    await CreateBackupUsingMySqlBackupNet(
                        filePath, 
                        includeSchema, 
                        includeData,
                        backupId);

                    // Step 3: Optional compression
                    if (useCompression)
                    {
                        filePath = await CompressBackupAsync(filePath);
                        fileName = Path.GetFileName(filePath);
                    }

                    // Step 4: Get file size
                    var fileInfo = new FileInfo(filePath);
                    var fileSize = fileInfo.Length;
                    var duration = (int)(DateTime.Now - startTime).TotalSeconds;

                    // Step 5: Update backup history as success
                    await using (var context = await _contextFactory.CreateDbContextAsync())
                    {
                        var history = await context.BackupHistories.FindAsync(backupId);
                        if (history != null)
                        {
                            history.Status = "Success";
                            history.FileName = fileName;
                            history.FilePath = filePath;
                            history.FileSize = fileSize;
                            history.CompletedAt = DateTime.Now;
                            history.DurationSeconds = duration;
                            await context.SaveChangesAsync();
                        }
                    }

                    _logger.LogInformation($"✅ Backup completed successfully!");
                    _logger.LogInformation($"   📁 File: {fileName}");
                    _logger.LogInformation($"   📏 Size: {FormatBytes(fileSize)}");
                    _logger.LogInformation($"   ⏱️  Duration: {duration}s");

                    // Step 6: Return updated history
                    await using (var context = await _contextFactory.CreateDbContextAsync())
                    {
                        var result = await context.BackupHistories
                            .Include(h => h.BackupSchedule)
                            .AsNoTracking()
                            .FirstOrDefaultAsync(h => h.Id == backupId);

                        return result ?? throw new Exception("Backup history not found after creation");
                    }
                }
                catch (Exception ex)
                {
                    // Update backup history as failed
                    await using (var context = await _contextFactory.CreateDbContextAsync())
                    {
                        var history = await context.BackupHistories.FindAsync(backupId);
                        if (history != null)
                        {
                            history.Status = "Failed";
                            history.ErrorMessage = ex.Message;
                            history.CompletedAt = DateTime.Now;
                            history.DurationSeconds = (int)(DateTime.Now - startTime).TotalSeconds;
                            await context.SaveChangesAsync();
                        }
                    }

                    // Delete partial file if exists
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }

                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Backup failed");
                throw new Exception($"Backup failed: {ex.Message}", ex);
            }
        }

        private async Task CreateBackupUsingMySqlBackupNet(
            string filePath, 
            bool includeSchema, 
            bool includeData,
            int backupId)
        {
            await Task.Run(() =>
            {
                // CORRECT: Use MySqlCommand, not MySqlConnection directly
                using var conn = new MySql.Data.MySqlClient.MySqlConnection(_connectionString);
                using var cmd = conn.CreateCommand();
                using var mb = new MySqlBackup(cmd);

                conn.Open();

                // Configure backup options
                mb.ExportInfo.AddCreateDatabase = true;
                mb.ExportInfo.AddDropDatabase = false;
                mb.ExportInfo.AddDropTable = includeSchema;
                mb.ExportInfo.ExportTableStructure = includeSchema;
                mb.ExportInfo.ExportRows = includeData;
                mb.ExportInfo.ResetAutoIncrement = true;
                mb.ExportInfo.RecordDumpTime = true;
                
                // Progress reporting
                mb.ExportProgressChanged += (sender, e) =>
                {
                    var percent = e.TotalRowsInAllTables > 0 
                        ? (int)(e.CurrentRowIndexInAllTables * 100.0 / e.TotalRowsInAllTables) 
                        : 0;
                    
                    if (percent % 10 == 0 || e.CurrentRowIndexInAllTables == 0)
                    {
                        _logger.LogInformation($"   📊 Progress: {percent}% - {e.CurrentTableName} ({e.CurrentRowIndexInCurrentTable}/{e.TotalRowsInCurrentTable})");
                    }
                };

                mb.ExportCompleted += (sender, e) =>
                {
                    _logger.LogInformation($"   ✅ MySqlBackup.NET export completed in {e.TimeUsed.TotalSeconds:F2}s");
                };

                // Perform backup
                _logger.LogInformation($"   🔄 Exporting database using MySqlBackup.NET...");
                mb.ExportToFile(filePath);
            });
        }

        private async Task<string> CompressBackupAsync(string filePath)
        {
            var compressedPath = filePath + ".gz";
            
            _logger.LogInformation($"   🗜️  Compressing backup...");
            
            await using (var sourceStream = File.OpenRead(filePath))
            await using (var destStream = File.Create(compressedPath))
            await using (var gzipStream = new GZipStream(destStream, CompressionLevel.Optimal))
            {
                await sourceStream.CopyToAsync(gzipStream);
            }
            
            var originalSize = new FileInfo(filePath).Length;
            var compressedSize = new FileInfo(compressedPath).Length;
            var ratio = (1 - (double)compressedSize / originalSize) * 100;
            
            _logger.LogInformation($"   ✅ Compression: {ratio:F1}% reduction");
            _logger.LogInformation($"   📦 {FormatBytes(originalSize)} → {FormatBytes(compressedSize)}");
            
            // Delete uncompressed file
            File.Delete(filePath);
            
            return compressedPath;
        }

        public async Task<bool> RestoreBackupAsync(int historyId, int userId)
        {
            try
            {
                BackupHistory? history;
                
                await using (var context = await _contextFactory.CreateDbContextAsync())
                {
                    history = await context.BackupHistories
                        .AsNoTracking()
                        .FirstOrDefaultAsync(h => h.Id == historyId);
                }

                if (history == null)
                {
                    _logger.LogError($"Backup history not found: {historyId}");
                    return false;
                }

                if (!File.Exists(history.FilePath))
                {
                    _logger.LogError($"Backup file not found: {history.FilePath}");
                    return false;
                }

                _logger.LogInformation($"🔄 Starting restore from: {history.FileName}");

                var filePath = history.FilePath;
                var isCompressed = filePath.EndsWith(".gz");

                // Decompress if needed
                if (isCompressed)
                {
                    _logger.LogInformation($"   🗜️  Decompressing backup...");
                    filePath = await DecompressBackupAsync(filePath);
                }

                await RestoreBackupUsingMySqlBackupNet(filePath);

                // Clean up decompressed file
                if (isCompressed && File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                _logger.LogInformation($"✅ Database restored successfully from: {history.FileName}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Restore failed");
                return false;
            }
        }

        private async Task<string> DecompressBackupAsync(string compressedPath)
        {
            var decompressedPath = compressedPath.Replace(".gz", "");
            
            await using (var sourceStream = File.OpenRead(compressedPath))
            await using (var gzipStream = new GZipStream(sourceStream, CompressionMode.Decompress))
            await using (var destStream = File.Create(decompressedPath))
            {
                await gzipStream.CopyToAsync(destStream);
            }
            
            _logger.LogInformation($"   ✅ Decompressed to: {FormatBytes(new FileInfo(decompressedPath).Length)}");
            
            return decompressedPath;
        }

        private async Task RestoreBackupUsingMySqlBackupNet(string filePath)
        {
            await Task.Run(() =>
            {
                // CORRECT: Use MySqlCommand, not MySqlConnection directly
                using var conn = new MySql.Data.MySqlClient.MySqlConnection(_connectionString);
                using var cmd = conn.CreateCommand();
                using var mb = new MySqlBackup(cmd);

                conn.Open();

                // Progress reporting
                mb.ImportProgressChanged += (sender, e) =>
                {
                    var percent = e.TotalBytes > 0 
                        ? (int)(e.CurrentBytes * 100.0 / e.TotalBytes) 
                        : 0;
                    
                    if (percent % 10 == 0)
                    {
                        _logger.LogInformation($"   📊 Restore progress: {percent}%");
                    }
                };

                mb.ImportCompleted += (sender, e) =>
                {
                    _logger.LogInformation($"   ✅ MySqlBackup.NET import completed in {e.TimeUsed.TotalSeconds:F2}s");
                };

                _logger.LogInformation($"   🔄 Importing database using MySqlBackup.NET...");
                mb.ImportFromFile(filePath);
            });
        }

        public async Task<List<BackupHistory>> GetBackupHistoryAsync(int pageNumber = 1, int pageSize = 20)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.BackupHistories
                .Include(h => h.BackupSchedule)
                .AsNoTracking()
                .OrderByDescending(h => h.StartedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<BackupHistory?> GetBackupHistoryByIdAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.BackupHistories
                .Include(h => h.BackupSchedule)
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.Id == id);
        }

        public async Task<bool> DeleteBackupFileAsync(int historyId)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                
                var history = await context.BackupHistories.FindAsync(historyId);
                if (history == null)
                {
                    _logger.LogWarning($"Backup history not found: {historyId}");
                    return false;
                }

                if (File.Exists(history.FilePath))
                {
                    File.Delete(history.FilePath);
                    _logger.LogInformation($"🗑️  Deleted backup file: {history.FilePath}");
                }

                context.BackupHistories.Remove(history);
                await context.SaveChangesAsync();

                _logger.LogInformation($"✅ Backup deleted: {history.FileName}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Delete backup failed");
                return false;
            }
        }

        public async Task CleanupOldBackupsAsync()
        {
            try
            {
                var retentionDays = int.Parse(_configuration["Backup:RetentionDays"] ?? "30");
                var cutoffDate = DateTime.Now.AddDays(-retentionDays);

                _logger.LogInformation($"🧹 Cleaning up backups older than {retentionDays} days...");

                await using var context = await _contextFactory.CreateDbContextAsync();
                
                var oldBackups = await context.BackupHistories
                    .Where(h => h.StartedAt < cutoffDate && h.Status == "Success")
                    .ToListAsync();

                foreach (var backup in oldBackups)
                {
                    if (File.Exists(backup.FilePath))
                    {
                        File.Delete(backup.FilePath);
                    }
                    context.BackupHistories.Remove(backup);
                }

                await context.SaveChangesAsync();

                _logger.LogInformation($"✅ Cleaned up {oldBackups.Count} old backups");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Cleanup failed");
            }
        }

        public async Task<string> DownloadBackupAsync(int historyId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            var history = await context.BackupHistories
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.Id == historyId);

            if (history == null || !File.Exists(history.FilePath))
            {
                throw new FileNotFoundException("Backup file not found");
            }

            var fileName = Path.GetFileName(history.FilePath);
            return $"/backups/{fileName}";
        }

        private string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}