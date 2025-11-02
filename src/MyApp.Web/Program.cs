using Blazored.Toast;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyApp.Core.Interfaces;
using MyApp.Infrastructure;
using MyApp.Infrastructure.Data;
using MyApp.Infrastructure.Identity;
using MyApp.Infrastructure.Services;
using MyApp.Web.Authentication;
using MyApp.Web.Data;
using MyApp.Web.Helpers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

// ======================================================
// BLAZOR SERVER
// ======================================================
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

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
    options.SignIn.RequireConfirmedEmail = false;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
})
.AddEntityFrameworkStores<AppDbContext>()
.AddSignInManager()
.AddDefaultTokenProviders();

// ======================================================
// COOKIE POLICY
// ======================================================
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/forbidden";
    options.Cookie.Name = "MyApp.Auth";
    options.Cookie.HttpOnly = false;

    if (builder.Environment.IsDevelopment())
    {
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Allow HTTP in dev
        options.Cookie.SameSite = SameSiteMode.Lax;
    }
    else
    {
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // HTTPS only in production
        options.Cookie.SameSite = SameSiteMode.Strict;
    }

    options.ExpireTimeSpan = TimeSpan.FromHours(2);
    options.SlidingExpiration = true;

    options.Events.OnRedirectToLogin = context =>
    {
        // Jangan redirect untuk API calls
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }

        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});


builder.Services.AddScoped<UploadService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddBlazoredToast();
builder.Services.AddHttpClient();


builder.Services.AddSingleton<WeatherForecastService>();


builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
    provider.GetRequiredService<CustomAuthenticationStateProvider>());
builder.Services.AddCascadingAuthenticationState();


builder.Services.AddAuthorization();

builder.Services.AddScoped<ToastService>();
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
        var context = services.GetRequiredService<AppDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
        var config = services.GetRequiredService<IConfiguration>();
        var logger = services.GetRequiredService<ILogger<Program>>();

        context.Database.Migrate();

        // --- Seed Role Admin ---
        const string adminRoleName = "Admin";
        var adminRoleExists = await roleManager.RoleExistsAsync(adminRoleName);
        if (!adminRoleExists)
        {
            await roleManager.CreateAsync(new IdentityRole<int>(adminRoleName));
            logger.LogInformation("Admin role created");
        }

        // --- Seed User Admin ---
        var adminUserName = config["AdminUser:UserName"] ?? throw new Exception("AdminUser:UserName not configured");
        var adminEmail = config["AdminUser:Email"] ?? throw new Exception("AdminUser:Email not configured");
        var adminPassword = config["AdminUser:Password"] ?? throw new Exception("AdminUser:Password not configured");

        var adminUser = await userManager.FindByNameAsync(adminUserName);
        if (adminUser == null)
        {
            var newUser = new ApplicationUser
            {
                UserName = adminUserName,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(newUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(newUser, adminRoleName);
                logger.LogInformation("Admin user created and assigned to Admin role");
            }
            else
            {
                throw new Exception($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

        // --- Seed Menus ---
        await MenuSeeder.SeedMenusAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Database seeding failed");
    }
}


// ======================================================
// MIDDLEWARE CONFIGURATION
// ======================================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// COOKIE POLICY HARUS SEBELUM ROUTING
app.UseCookiePolicy();

app.UseRouting();


// PENTING: Urutan ini harus benar!
app.UseAuthentication();  // Harus SEBELUM Authorization
app.UseAuthorization();

// Tambahkan logging middleware untuk debug
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation($"Request: {context.Request.Method} {context.Request.Path}");
    logger.LogInformation($"IsAuthenticated: {context.User?.Identity?.IsAuthenticated}, User: {context.User?.Identity?.Name}");
    logger.LogInformation($"Cookies: {string.Join(", ", context.Request.Cookies.Keys)}");
    await next();
});

app.MapControllers();
app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
