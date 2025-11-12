using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyApp.Core.Entities;
using MyApp.Core.Interfaces;
using MyApp.Infrastructure.Data;
using MySqlConnector;

namespace MyApp.Infrastructure.Services
{
    public class BackupScheduleService : IBackupScheduleService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<BackupScheduleService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private bool _eventSchedulerAvailable = false;
        private bool _isInitialized = false;

        public BackupScheduleService(
            AppDbContext context, 
            ILogger<BackupScheduleService> logger,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string not found");
        }

        private async Task EnsureInitializedAsync()
        {
            if (_isInitialized) return;

            await CheckEventSchedulerAvailabilityAsync();
            _isInitialized = true;
        }

        private async Task CheckEventSchedulerAvailabilityAsync()
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();
                
                // Check EVENT privilege
                using var cmd = new MySqlCommand("SHOW GRANTS FOR CURRENT_USER()", connection);
                using var reader = await cmd.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    var grant = reader.GetString(0);
                    if (grant.Contains("EVENT", StringComparison.OrdinalIgnoreCase) || 
                        grant.Contains("ALL PRIVILEGES", StringComparison.OrdinalIgnoreCase))
                    {
                        _eventSchedulerAvailable = true;
                        break;
                    }
                }

                reader.Close();

                if (_eventSchedulerAvailable)
                {
                    // Enable event scheduler
                    using var enableCmd = new MySqlCommand("SET GLOBAL event_scheduler = ON", connection);
                    await enableCmd.ExecuteNonQueryAsync();
                    _logger.LogInformation("✅ MySQL Event Scheduler is ENABLED");
                }
                else
                {
                    _logger.LogWarning("⚠️ MySQL Event Scheduler NOT available (EVENT privilege required)");
                    _logger.LogInformation("ℹ️ System will use Background Service fallback for scheduled backups");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Could not enable Event Scheduler - using Background Service fallback");
                _eventSchedulerAvailable = false;
            }
        }

        public async Task<List<BackupSchedule>> GetAllSchedulesAsync()
        {
            return await _context.BackupSchedules
                .AsNoTracking()
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<BackupSchedule?> GetScheduleByIdAsync(int id)
        {
            return await _context.BackupSchedules
                .Include(s => s.BackupHistories)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<BackupSchedule> CreateScheduleAsync(BackupSchedule schedule)
        {
            // Ensure Event Scheduler check is completed
            await EnsureInitializedAsync();

            var strategy = _context.Database.CreateExecutionStrategy();
            
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                
                try
                {
                    // Ensure backup directory exists
                    var fullPath = Path.GetFullPath(schedule.BackupPath);
                    if (!Directory.Exists(fullPath))
                    {
                        Directory.CreateDirectory(fullPath);
                        _logger.LogInformation($"📁 Created backup directory: {fullPath}");
                    }

                    schedule.CreatedAt = DateTime.Now;
                    _context.BackupSchedules.Add(schedule);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation($"✅ Backup schedule created: {schedule.Name}");
                    _logger.LogInformation($"   📅 Frequency: {schedule.Frequency}");
                    _logger.LogInformation($"   🕐 Time: {schedule.ScheduledTime}");
                    _logger.LogInformation($"   🔄 Event Scheduler Available: {_eventSchedulerAvailable}");
                    
                    // Sync to MySQL Event Scheduler if available
                    if (_eventSchedulerAvailable && schedule.IsEnabled)
                    {
                        _logger.LogInformation($"   🔄 Creating MySQL Event...");
                        await SyncScheduleToDatabase(schedule);
                    }
                    else if (!_eventSchedulerAvailable && schedule.IsEnabled)
                    {
                        _logger.LogInformation($"   ⏰ Will use Background Service (fallback mode)");
                    }
                    
                    await transaction.CommitAsync();
                    
                    return schedule;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, $"❌ Error creating backup schedule: {schedule.Name}");
                    throw;
                }
            });
        }

        public async Task<BackupSchedule> UpdateScheduleAsync(BackupSchedule schedule)
        {
            await EnsureInitializedAsync();

            var strategy = _context.Database.CreateExecutionStrategy();
            
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                
                try
                {
                    var fullPath = Path.GetFullPath(schedule.BackupPath);
                    if (!Directory.Exists(fullPath))
                    {
                        Directory.CreateDirectory(fullPath);
                    }

                    schedule.UpdatedAt = DateTime.Now;
                    _context.BackupSchedules.Update(schedule);
                    await _context.SaveChangesAsync();
                    
                    // Update Event Scheduler
                    if (_eventSchedulerAvailable)
                    {
                        await RemoveScheduleFromDatabase(schedule.Id);
                        if (schedule.IsEnabled)
                        {
                            await SyncScheduleToDatabase(schedule);
                        }
                    }
                    
                    await transaction.CommitAsync();
                    _logger.LogInformation($"✅ Backup schedule updated: {schedule.Name}");
                    
                    return schedule;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, $"❌ Error updating backup schedule");
                    throw;
                }
            });
        }

        public async Task DeleteScheduleAsync(int id)
        {
            await EnsureInitializedAsync();

            var strategy = _context.Database.CreateExecutionStrategy();
            
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                
                try
                {
                    var schedule = await _context.BackupSchedules.FindAsync(id);
                    if (schedule != null)
                    {
                        if (_eventSchedulerAvailable)
                        {
                            await RemoveScheduleFromDatabase(id);
                        }
                        
                        _context.BackupSchedules.Remove(schedule);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                        
                        _logger.LogInformation($"✅ Backup schedule deleted: {schedule.Name}");
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, $"❌ Error deleting backup schedule");
                    throw;
                }
            });
        }

        public async Task<bool> ToggleScheduleAsync(int id, bool isEnabled)
        {
            await EnsureInitializedAsync();

            try
            {
                var schedule = await _context.BackupSchedules.FindAsync(id);
                if (schedule == null) return false;

                schedule.IsEnabled = isEnabled;
                schedule.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                
                // Update Event Scheduler
                if (_eventSchedulerAvailable)
                {
                    await RemoveScheduleFromDatabase(id);
                    if (isEnabled)
                    {
                        await SyncScheduleToDatabase(schedule);
                    }
                }
                
                _logger.LogInformation($"✅ Schedule {(isEnabled ? "enabled" : "disabled")}: {schedule.Name}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error toggling schedule {id}");
                return false;
            }
        }

        // Sync schedule to MySQL Event Scheduler
        public async Task SyncScheduleToDatabase(BackupSchedule schedule)
        {
            if (!_eventSchedulerAvailable)
            {
                _logger.LogDebug("Event Scheduler not available, skipping sync");
                return;
            }

            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                var eventName = $"backup_schedule_{schedule.Id}";
                var intervalExpression = GetIntervalExpression(schedule);
                
                _logger.LogInformation($"   📝 Creating MySQL Event: {eventName}");
                _logger.LogInformation($"   📅 Interval: {intervalExpression}");
                
                // Create stored procedure call command
                var procedureCall = $@"CALL sp_execute_backup({schedule.Id}, '{schedule.BackupPath}', {(schedule.IncludeSchema ? "TRUE" : "FALSE")}, {(schedule.IncludeData ? "TRUE" : "FALSE")}, {schedule.CreatedBy})";

                // Create MySQL Event
                var createEventSql = $@"
CREATE EVENT IF NOT EXISTS {eventName}
ON SCHEDULE {intervalExpression}
DO
BEGIN
    {procedureCall};
END;
";

                _logger.LogDebug($"SQL: {createEventSql}");

                using var cmd = new MySqlCommand(createEventSql, connection);
                await cmd.ExecuteNonQueryAsync();

                _logger.LogInformation($"   ✅ MySQL Event created: {eventName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Failed to create MySQL Event for schedule {schedule.Id}");
                throw;
            }
        }

        public async Task RemoveScheduleFromDatabase(int scheduleId)
        {
            if (!_eventSchedulerAvailable) return;

            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                var eventName = $"backup_schedule_{scheduleId}";
                var dropEventSql = $"DROP EVENT IF EXISTS {eventName}";

                using var cmd = new MySqlCommand(dropEventSql, connection);
                await cmd.ExecuteNonQueryAsync();

                _logger.LogInformation($"🗑️ MySQL Event removed: {eventName}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"⚠️ Failed to remove MySQL Event for schedule {scheduleId}");
            }
        }

        private string GetIntervalExpression(BackupSchedule schedule)
        {
            // Asumsikan user input dalam waktu lokal (misal WIB/GMT+7)
            var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); // Windows: "SE Asia Standard Time", Linux: "Asia/Jakarta"
            var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, userTimeZone);

            var time = schedule.ScheduledTime?.ToTimeSpan() ?? TimeSpan.Zero;

            // Waktu target dalam waktu lokal
            var targetLocal = nowLocal.Date.Add(time);
            if (targetLocal <= nowLocal)
                targetLocal = targetLocal.AddDays(1);

            // Konversi ke UTC
            var targetUtc = TimeZoneInfo.ConvertTimeToUtc(targetLocal, userTimeZone);

            switch (schedule.Frequency)
            {
                case "Daily":
                    return $"EVERY 1 DAY STARTS '{targetUtc:yyyy-MM-dd HH:mm:ss}'";
                case "Weekly":
                    var dayOffset = (schedule.DayOfWeek ?? 0) - (int)nowLocal.DayOfWeek;
                    if (dayOffset < 0) dayOffset += 7;
                    var weeklyLocal = nowLocal.Date.AddDays(dayOffset).Add(time);
                    if (weeklyLocal <= nowLocal)
                        weeklyLocal = weeklyLocal.AddDays(7);
                    var weeklyUtc = TimeZoneInfo.ConvertTimeToUtc(weeklyLocal, userTimeZone);
                    return $"EVERY 1 WEEK STARTS '{weeklyUtc:yyyy-MM-dd HH:mm:ss}'";
                case "Monthly":
                    var day = schedule.DayOfMonth ?? 1;
                    var monthlyLocal = new DateTime(nowLocal.Year, nowLocal.Month, Math.Min(day, DateTime.DaysInMonth(nowLocal.Year, nowLocal.Month)), 0, 0, 0)
                        .Add(time);
                    if (monthlyLocal <= nowLocal)
                        monthlyLocal = monthlyLocal.AddMonths(1);
                    var monthlyUtc = TimeZoneInfo.ConvertTimeToUtc(monthlyLocal, userTimeZone);
                    return $"EVERY 1 MONTH STARTS '{monthlyUtc:yyyy-MM-dd HH:mm:ss}'";
                case "Custom":
                    return schedule.CustomCronExpression ?? "EVERY 1 DAY";
                default:
                    return $"EVERY 1 DAY STARTS '{targetUtc:yyyy-MM-dd HH:mm:ss}'";
            }
        }
    }
}