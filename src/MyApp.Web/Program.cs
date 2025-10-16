using Blazored.Toast;
using Blazored.Typeahead;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyApp.Core.Interfaces;
using MyApp.Infrastructure.Data;
using MyApp.Infrastructure.Identity;
using MyApp.Infrastructure.Repositories.Services;
using MyApp.Web.Data;
using MyApp.Web.Helpers;
using MyApp.Web.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// ======================================================
// DATABASE CONTEXTS
// ======================================================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));

// Identity pakai SQL Server (boleh juga MySQL, asal konsisten)

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));

// // Add Identity
// builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
// {
//     options.Password.RequiredLength = 6;
//     options.Password.RequireDigit = false;
//     options.Password.RequireUppercase = false;
//     options.Password.RequireNonAlphanumeric = false;
//     options.User.RequireUniqueEmail = true;
// })
// .AddEntityFrameworkStores<ApplicationDbContext>()
// .AddDefaultTokenProviders();
// ======================================================
// ASP.NET CORE IDENTITY
// ======================================================
builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;

    options.User.RequireUniqueEmail = true;

    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/forbidden";
    options.Cookie.Name = "MyApp.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.None;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
});

builder.Services.AddAuthorization();

// ======================================================
// REDIS (optional, tetap untuk caching umum, bukan auth)
// ======================================================
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "MyApp_";
});

// ======================================================
// DEPENDENCY INJECTION REPOSITORIES
// ======================================================
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPositionRepository, PositionRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IUserTypeRepository, UserTypeRepository>();
builder.Services.AddScoped<IRankRepository, RankRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IRoomCategoryRepository, RoomCategoryRepository>();
builder.Services.AddScoped<IRoomStatusRepository, RoomStatusRepository>();
builder.Services.AddScoped<IRoomConditionRepository, RoomConditionRepository>();
builder.Services.AddScoped<IOccupantRepository, OccupantRepository>();
builder.Services.AddScoped<IOccupantHistoryRepository, OccupantHistoryRepository>();
builder.Services.AddScoped<IBuildingRepository, BuildingRepository>();
builder.Services.AddScoped<IVisitorRepository, VisitorRepository>();
builder.Services.AddScoped<UploadService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IVendorRepository, VendorRepository>();
builder.Services.AddScoped<IMaintenanceRequestRepository, MaintenanceRequestRepository>();
builder.Services.AddScoped<IInventoryTypeRepository, InventoryTypeRepository>();
builder.Services.AddScoped<IRepositoryRepository, RepositoryRepository>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IInventoryRequestRepository, InventoryRequestRepository>();
builder.Services.AddScoped<IInventoryHistoryRepository, InventoryHistoryRepository>();
builder.Services.AddScoped<IWeaponRepository, WeaponRepository>();
builder.Services.AddScoped<IAlsusRepository, AlsusRepository>();

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddBlazoredToast();
builder.Services.AddHttpClient();

// ======================================================
// BLAZOR SERVER
// ======================================================
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

builder.Services.AddScoped<ToastService>();
builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<CustomAuthenticationStateProvider>());

builder.Services.AddScoped(sp =>
{
    var nav = sp.GetRequiredService<NavigationManager>();
    return new HttpClient { BaseAddress = new Uri(nav.BaseUri) };
});

builder.Services.AddSingleton(new TTLockClient(
    clientId: "3c9fffb0685f4653bb2f0aeef43157e7",
    clientSecret: "b70df03c403d1c5f743d80acb2024a94",
    username: "arinababan123@gmail.com",
    password: "ar1aja123"
));

builder.Services.AddScoped<DeviceService>();

// ======================================================
// CONTROLLERS
// ======================================================
builder.Services.AddControllers();

// ======================================================
// BUILD APP
// ======================================================
var app = builder.Build();

// ======================================================
// DATABASE MIGRATION & SEEDING
// ======================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Context utama aplikasi
        var appDbContext = services.GetRequiredService<AppDbContext>();
        appDbContext.Database.Migrate();

        // Context Identity
        var identityContext = services.GetRequiredService<ApplicationDbContext>();
        identityContext.Database.Migrate();

        // --- SEEDING ROLE & USER ADMIN ---
        var userRepo = services.GetRequiredService<IUserRepository>();
        var roleRepo = services.GetRequiredService<IRoleRepository>();
        var config = services.GetRequiredService<IConfiguration>();

        const string adminRoleName = "Admin";
        var adminRole = await roleRepo.GetByNameAsync(adminRoleName);
        if (adminRole == null)
        {
            adminRole = new MyApp.Core.Entities.Role { RoleName = adminRoleName };
            await roleRepo.AddAsync(adminRole);
        }

        var adminUserName = config["AdminUser:UserName"];
        var adminUser = await userRepo.GetByUserNameAsync(adminUserName);
        if (adminUser == null)
        {
            var newUser = new MyApp.Core.Entities.User
            {
                UserName = adminUserName,
                Email = config["AdminUser:Email"],
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(config["AdminUser:Password"]),
                RoleId = adminRole.Id,
                IsActive = true,
                CreatedBy = "System",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await userRepo.AddAsync(newUser);
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Database seeding failed");
    }
}


// ======================================================
// MIDDLEWARE PIPELINE
// ======================================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}


app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// ======================================================
// ENDPOINTS
// ======================================================
app.MapControllers();
app.MapBlazorHub();
app.MapRazorPages();
app.MapFallbackToPage("/_Host");

app.Run();
