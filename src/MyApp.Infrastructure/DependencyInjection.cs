using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyApp.Core.Interfaces;
using MyApp.Infrastructure.Data;
using MyApp.Infrastructure.Redis;
using MyApp.Infrastructure.Services;
using StackExchange.Redis;

namespace MyApp.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Register MySQL
            services.AddDbContext<AppDbContext>(options =>
                options.UseMySql(configuration.GetConnectionString("DefaultConnection"),
                    ServerVersion.AutoDetect(configuration.GetConnectionString("DefaultConnection"))));

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

            return services;
        }
    }
}
