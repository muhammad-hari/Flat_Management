using Blazored.Toast;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyApp.Core.Interfaces;
using MyApp.Infrastructure.Data;
using MyApp.Infrastructure.Identity;
using MyApp.Web.Data;
using MyApp.Web.Helpers;
using System.Text;
using Blazored.Typeahead;

var builder = WebApplication.CreateBuilder(args);

// ----- JWT Settings -----
var key = builder.Configuration["Jwt:Key"];
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
    };
});

// ----- Database -----
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));


// ----- Redis Cache -----
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "MyApp_";
});

// ----- Controllers & JSON Options -----
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // pakai PascalCase
    });

// ----- Authorization -----
builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

// ----- Dependency Injection -----
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

builder.Services.AddSingleton<RedisService>();
builder.Services.AddBlazoredToast();

// ----- Blazor Services -----
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped(sp =>
{
    var nav = sp.GetRequiredService<NavigationManager>();
    return new HttpClient { BaseAddress = new Uri(nav.BaseUri) };
});


var app = builder.Build();

// ----- Auto-migrate & Seed -----
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    // var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    // var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // const string adminRole = "Admin";
    // if (!await roleManager.RoleExistsAsync(adminRole))
    //     await roleManager.CreateAsync(new IdentityRole(adminRole));

    // var adminEmail = builder.Configuration["AdminUser:Email"] ?? "admin@local";
    // var adminUserName = builder.Configuration["AdminUser:UserName"] ?? "admin";
    // var adminPassword = builder.Configuration["AdminUser:Password"] ?? "Admin@123";

    // var admin = await userManager.FindByEmailAsync(adminEmail);
    // if (admin == null)
    // {
    //     admin = new ApplicationUser
    //     {
    //         UserName = adminUserName,
    //         Email = adminEmail,
    //         EmailConfirmed = true
    //     };
    //     var result = await userManager.CreateAsync(admin, adminPassword);
    //     if (result.Succeeded)
    //         await userManager.AddToRoleAsync(admin, adminRole);
    // }
    // else if (!await userManager.IsInRoleAsync(admin, adminRole))
    // {
    //     await userManager.AddToRoleAsync(admin, adminRole);
    // }
}

// ----- Middleware -----
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication(); // harus sebelum UseAuthorization
app.UseAuthorization();

app.MapBlazorHub();
app.MapControllers();
app.MapRazorPages();
app.MapFallbackToPage("/_Host");

app.Run();
