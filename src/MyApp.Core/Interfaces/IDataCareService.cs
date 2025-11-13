using MyApp.Core.Entities;

namespace MyApp.Core.Interfaces
{
    public interface IDataCareService
    {
        Task<BackupHistory> ExecuteBackupAsync(int? scheduleId, int? userId, string backupType = "Manual", int? backupHistoryId = null);
        Task<List<BackupHistory>> GetBackupHistoryAsync(int pageNumber = 1, int pageSize = 20);
        Task<BackupHistory?> GetBackupHistoryByIdAsync(int id);
        Task<List<RestoreHistory>> GetRestoreHistoryAsync(int pageNumber = 1, int pageSize = 20);
        Task<bool> RestoreBackupAsync(int historyId, int userId);
        Task<bool> RestoreBackupFromFileAsync(string filePath, int userId);
        Task<bool> DeleteBackupFileAsync(int historyId);
        Task CleanupOldBackupsAsync();
        
        // FIX: Return download URL string instead of file path
        Task<string> DownloadBackupAsync(int historyId);
    }
}