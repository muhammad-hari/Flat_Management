using MyApp.Core.Entities;

namespace MyApp.Core.Interfaces
{
    public interface ISystemSettingService
    {
        Task<List<SystemSetting>> GetAllSettingsAsync();
        Task<SystemSetting?> GetSettingByKeyAsync(string key);
        Task<string?> GetSettingValueAsync(string key);
        Task UpdateSettingAsync(string key, string value);
        Task UpdateSettingsAsync(Dictionary<string, string> settings); // NEW: Batch update
        Task CreateSettingAsync(SystemSetting setting);
        Task DeleteSettingAsync(int id);
        
        event Action? OnSettingsChanged;
        void NotifySettingsChanged();
    }
}