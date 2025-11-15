using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MyApp.Core.Entities;
using MyApp.Core.Interfaces;
using MyApp.Infrastructure.Data;

namespace MyApp.Infrastructure.Services
{
    public class SystemSettingService : ISystemSettingService
    {
        private readonly AppDbContext _context;
        
        // Static event untuk broadcast ke semua instance
        private static event Action? _onSettingsChanged;
        public event Action? OnSettingsChanged
        {
            add => _onSettingsChanged += value;
            remove => _onSettingsChanged -= value;
        }

        private const string DefaultSiteName = "Flat Management System";
        private const string DefaultLogoPath = "/assets/images/default-avatar.png";

        public SystemSettingService(AppDbContext context)
        {
            _context = context;
        }

        public void NotifySettingsChanged() 
        {
            _onSettingsChanged?.Invoke();
        }

        public async Task<List<SystemSetting>> GetAllSettingsAsync()
        {
            return await _context.SystemSettings
                .AsNoTracking()
                .OrderBy(s => s.Key)
                .ToListAsync();
        }

        public async Task<SystemSetting?> GetSettingByKeyAsync(string key)
        {
            return await _context.SystemSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Key == key);
        }

        public async Task<string?> GetSettingValueAsync(string key)
        {
            var setting = await _context.SystemSettings
                .AsNoTracking()
                .Where(s => s.Key == key)
                .Select(s => s.Value)
                .FirstOrDefaultAsync();
            
            if (setting == null)
            {
                return key switch
                {
                    "SiteName" => DefaultSiteName,
                    _ => null
                };
            }
            
            return setting;
        }

        public async Task UpdateSettingAsync(string key, string value)
        {
            // Use ExecutionStrategy for retry-compatible transactions
            var strategy = _context.Database.CreateExecutionStrategy();
            
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                
                try
                {
                    var setting = await _context.SystemSettings
                        .FirstOrDefaultAsync(s => s.Key == key);
                        
                    if (setting != null)
                    {
                        setting.Value = value;
                        setting.UpdatedAt = DateTime.Now;
                    }
                    else
                    {
                        setting = new SystemSetting
                        {
                            Key = key,
                            Value = value,
                            Description = GetDefaultDescription(key),
                            CreatedAt = DateTime.Now
                        };
                        _context.SystemSettings.Add(setting);
                    }
                    
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    
                    // Trigger event after successful commit
                    NotifySettingsChanged();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task UpdateSettingsAsync(Dictionary<string, string> settings)
        {
            if (settings == null || !settings.Any())
                return;

            // Use ExecutionStrategy for retry-compatible transactions
            var strategy = _context.Database.CreateExecutionStrategy();
            
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                
                try
                {
                    // Get all keys to update
                    var keys = settings.Keys.ToList();
                    
                    // Load existing settings
                    var existingSettings = await _context.SystemSettings
                        .Where(s => keys.Contains(s.Key))
                        .ToListAsync();

                    var now = DateTime.Now;

                    foreach (var kvp in settings)
                    {
                        var existing = existingSettings.FirstOrDefault(s => s.Key == kvp.Key);
                        
                        if (existing != null)
                        {
                            // Update existing
                            existing.Value = kvp.Value;
                            existing.UpdatedAt = now;
                        }
                        else
                        {
                            // Create new
                            var newSetting = new SystemSetting
                            {
                                Key = kvp.Key,
                                Value = kvp.Value,
                                Description = GetDefaultDescription(kvp.Key),
                                CreatedAt = now
                            };
                            _context.SystemSettings.Add(newSetting);
                        }
                    }
                    
                    // Save all changes
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    
                    // Trigger event after successful commit
                    NotifySettingsChanged();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task CreateSettingAsync(SystemSetting setting)
        {
            // Use ExecutionStrategy for retry-compatible transactions
            var strategy = _context.Database.CreateExecutionStrategy();
            
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                
                try
                {
                    setting.CreatedAt = DateTime.Now;
                    _context.SystemSettings.Add(setting);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    
                    NotifySettingsChanged();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task DeleteSettingAsync(int id)
        {
            // Use ExecutionStrategy for retry-compatible transactions
            var strategy = _context.Database.CreateExecutionStrategy();
            
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                
                try
                {
                    var setting = await _context.SystemSettings.FindAsync(id);
                    if (setting != null)
                    {
                        _context.SystemSettings.Remove(setting);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                        
                        NotifySettingsChanged();
                    }
                    else
                    {
                        await transaction.RollbackAsync();
                    }
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        private string GetDefaultDescription(string key)
        {
            return key switch
            {
                "SiteName" => "The name of the application displayed in the header and title",
                "SiteLogo" => "The logo image displayed in the sidebar (base64 encoded)",
                _ => string.Empty
            };
        }
    }
}