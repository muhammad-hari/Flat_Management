using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyApp.Core.Interfaces;
using MyApp.Infrastructure.Data;

namespace MyApp.Infrastructure.Services
{
    public class BackupProcessorHostedService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BackupProcessorHostedService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

        public BackupProcessorHostedService(
            IServiceProvider serviceProvider,
            ILogger<BackupProcessorHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🚀 Backup Processor Service started (MySqlBackup.NET + Event Scheduler)");
            _logger.LogInformation("   Mode: Hybrid (Event Scheduler + Fallback Polling)");
            
            // Wait 10 seconds for app to fully start
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Process pending backups created by Event Scheduler
                    await ProcessPendingBackupsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error processing pending backups");
                }

                // Wait before next check
                await Task.Delay(_checkInterval, stoppingToken);
            }
            
            _logger.LogInformation("⏹️ Backup Processor Service stopped");
        }

        private async Task ProcessPendingBackupsAsync(CancellationToken cancellationToken)
       {
            using var scope = _serviceProvider.CreateScope();
            var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
            var backupService = scope.ServiceProvider.GetRequiredService<IDataCareService>();

            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

            // Get pending backups
            var pendingBackups = await context.BackupHistories
                .Include(h => h.BackupSchedule)
                .Where(h => h.Status == "Pending")
                .OrderBy(h => h.CreatedAt)
                .Take(5) // Process max 5 at a time
                .ToListAsync(cancellationToken);

            if (!pendingBackups.Any())
                return;

            _logger.LogInformation($"📋 Processing {pendingBackups.Count} pending backup(s)");

            foreach (var backup in pendingBackups)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    _logger.LogInformation($"🔄 Processing backup #{backup.Id}: {backup.FileName}");

                    // Mark as processing
                    backup.Status = "In Progress";
                    await context.SaveChangesAsync(cancellationToken);

                    // Execute backup using MySqlBackup.NET
                    var result = await backupService.ExecuteBackupAsync(
                        scheduleId: backup.BackupScheduleId,
                        userId: backup.CreatedBy,
                        backupType: backup.BackupType,
                        backupHistoryId: backup.Id // <-- tambahkan ini!
                    );

                    _logger.LogInformation($"✅ Backup #{backup.Id} completed: {result.FileName}");
                    _logger.LogInformation($"   📏 Size: {FormatBytes(result.FileSize)}");
                    _logger.LogInformation($"   ⏱️  Duration: {result.DurationSeconds}s");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"❌ Backup #{backup.Id} failed: {ex.Message}");
                    
                    // Mark as failed
                    backup.Status = "Failed";
                    backup.ErrorMessage = ex.Message;
                    backup.CompletedAt = DateTime.Now;
                    await context.SaveChangesAsync(cancellationToken);
                }
            }
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