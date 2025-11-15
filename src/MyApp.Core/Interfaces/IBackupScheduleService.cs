using MyApp.Core.Entities;

namespace MyApp.Core.Interfaces
{
    public interface IBackupScheduleService
    {
        Task<List<BackupSchedule>> GetAllSchedulesAsync();
        Task<BackupSchedule?> GetScheduleByIdAsync(int id);
        Task<BackupSchedule> CreateScheduleAsync(BackupSchedule schedule);
        Task<BackupSchedule> UpdateScheduleAsync(BackupSchedule schedule);
        Task DeleteScheduleAsync(int id);
        Task<bool> ToggleScheduleAsync(int id, bool isEnabled);
        Task SyncScheduleToDatabase(BackupSchedule schedule);
        Task RemoveScheduleFromDatabase(int scheduleId);
    }
}