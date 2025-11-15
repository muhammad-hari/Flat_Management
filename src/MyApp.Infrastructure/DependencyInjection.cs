using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyApp.Core.Interfaces;
using MyApp.Infrastructure.Data;
using MyApp.Infrastructure.Redis;
using MyApp.Infrastructure.Repositories;
using MyApp.Infrastructure.Repositories.Services;
using MyApp.Infrastructure.Services;
using StackExchange.Redis;

namespace MyApp.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Get connection string once
            var connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            
            // FIX: Use only AddDbContextFactory instead of both AddDbContext and AddPooledDbContextFactory
            services.AddDbContextFactory<AppDbContext>(options =>
            {
                options.UseMySql(connectionString,
                    ServerVersion.AutoDetect(connectionString),
                    mySqlOptions =>
                    {
                        mySqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(5),
                            errorNumbersToAdd: null);
                    });
            });

            // Also add regular DbContext for services that need it (like repositories)
            services.AddScoped<AppDbContext>(sp =>
            {
                var factory = sp.GetRequiredService<IDbContextFactory<AppDbContext>>();
                return factory.CreateDbContext();
            });

            // Register Redis Connection
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var redisConfig = ConfigurationOptions.Parse(
                    configuration.GetConnectionString("Redis") ?? "localhost:6379"
                );
                return ConnectionMultiplexer.Connect(redisConfig);
            });

            // Redis Cache Service
            services.AddSingleton<IRedisService, RedisService>();

            // ======================================================
            // DEPENDENCY INJECTION REPOSITORIES
            // ======================================================
            services.AddScoped<IPositionRepository, PositionRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IUserTypeRepository, UserTypeRepository>();
            services.AddScoped<IRankRepository, RankRepository>();
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<IRoomRepository, RoomRepository>();
            services.AddScoped<IRoomCategoryRepository, RoomCategoryRepository>();
            services.AddScoped<IRoomStatusRepository, RoomStatusRepository>();
            services.AddScoped<IRoomConditionRepository, RoomConditionRepository>();
            services.AddScoped<IOccupantRepository, OccupantRepository>();
            services.AddScoped<IOccupantHistoryRepository, OccupantHistoryRepository>();
            services.AddScoped<IBuildingRepository, BuildingRepository>();
            services.AddScoped<IVisitorRepository, VisitorRepository>();
            services.AddScoped<IVendorRepository, VendorRepository>();
            services.AddScoped<IMaintenanceRequestRepository, MaintenanceRequestRepository>();
            services.AddScoped<IInventoryTypeRepository, InventoryTypeRepository>();
            services.AddScoped<IRepositoryRepository, RepositoryRepository>();
            services.AddScoped<IInventoryRepository, InventoryRepository>();
            services.AddScoped<IMenuService, MenuService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserRoleService, UserRoleService>();
            services.AddScoped<IMenuPermissionService, MenuPermissionService>();
            services.AddScoped<IIdentityRoleService, IdentityRoleService>();
            services.AddScoped<IWeaponRepository, WeaponRepository>();
            services.AddScoped<IAlsusRepository, AlsusRepository>();
            services.AddScoped<IAreaRepository, AreaRepository>();
            services.AddScoped<ICardRepository, CardRepository>();
            services.AddScoped<IGateDeviceRepository, GateDeviceRepository>();
            services.AddScoped<ISystemSettingService, SystemSettingService>();

            services.AddScoped<IBackupScheduleService, BackupScheduleService>();
            services.AddScoped<IDataCareService, DataCareService>();
            services.AddScoped<IAssignmentWeaponRepository, AssignmentWeaponRepository>();
            services.AddScoped<IAssignmentAlsusRepository, AssignmentAlsusRepository>();
            // Add this to your service registration
            services.AddHostedService<BackupProcessorHostedService>();

            return services;
        }
    }
}
