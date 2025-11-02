using DocumentFormat.OpenXml.Math;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyApp.Infrastructure.Identity;
using MyApp.Shared.Models;
using System.Security.Claims;

namespace MyApp.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<int>> roleManager,
            ILogger<AuthController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                _logger.LogInformation($"Login attempt for user: {request.UserName}");

                // Validasi input
                if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
                {
                    _logger.LogWarning("Login attempt with empty credentials");
                    return BadRequest(new MyApp.Core.Domain.LoginResponse 
                    { 
                        Success = false, 
                        Message = "Username and password are required" 
                    });
                }

                // Cari user berdasarkan username atau email
                var user = await _userManager.FindByNameAsync(request.UserName) 
                           ?? await _userManager.FindByEmailAsync(request.UserName);

                if (user == null)
                {
                    _logger.LogWarning($"Login attempt with non-existent user: {request.UserName}");
                    return Unauthorized(new MyApp.Core.Domain.LoginResponse 
                    { 
                        Success = false, 
                        Message = "Invalid username or password" 
                    });
                }

                // Cek apakah user aktif
                if (!user.IsActive)
                {
                    _logger.LogWarning($"Login attempt for inactive user: {user.UserName} (ID: {user.Id})");
                    return Unauthorized(new MyApp.Core.Domain.LoginResponse 
                    { 
                        Success = false, 
                        Message = "User account is inactive. Please contact administrator." 
                    });
                }

                // SIGN OUT dulu jika ada session lama
                await _signInManager.SignOutAsync();
                _logger.LogInformation($"Cleared any existing session for user: {user.UserName}");

                // Set explicit authentication properties
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = request.RememberMe,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1)
                };

                // Attempt sign in dengan EXPLICIT authentication properties
                var result = await _signInManager.PasswordSignInAsync(
                    user.UserName!,
                    request.Password,
                    isPersistent: request.RememberMe,
                    lockoutOnFailure: true);

                _logger.LogInformation($"SignIn result for {user.UserName}: Succeeded={result.Succeeded}, IsLockedOut={result.IsLockedOut}, RequiresTwoFactor={result.RequiresTwoFactor}");

                if (result.Succeeded)
                {
                    _logger.LogInformation($"✅ User {user.UserName} logged in successfully");

                    user.UpdatedAt = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);

                    var roles = await _userManager.GetRolesAsync(user);

                    return Ok(new MyApp.Core.Domain.LoginResponse
                    {
                        Success = true,
                        Message = "Login successful",
                        RedirectUrl = "/",
                        UserName = user.UserName,
                        Roles = roles.ToList(),
                        Email = user.Email,
                        UserId = user.Id

                    });
                }
                else if (result.IsLockedOut)
                {
                    _logger.LogWarning($"User {user.UserName} account is locked out");
                    return Unauthorized(new MyApp.Core.Domain.LoginResponse 
                    { 
                        Success = false, 
                        Message = "Account is locked due to multiple failed login attempts. Please try again later." 
                    });
                }
                else if (result.RequiresTwoFactor)
                {
                    _logger.LogInformation($"User {user.UserName} requires two-factor authentication");
                    return Ok(new MyApp.Core.Domain.LoginResponse 
                    { 
                        Success = false, 
                        Message = "Two-factor authentication required",
                        RequiresTwoFactor = true
                    });
                }
                else
                {
                    _logger.LogWarning($"Failed login attempt for user {request.UserName} - Invalid password");
                    return Unauthorized(new MyApp.Core.Domain.LoginResponse 
                    { 
                        Success = false, 
                        Message = "Invalid username or password" 
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Login error for user {request.UserName}: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return StatusCode(500, new MyApp.Core.Domain.LoginResponse 
                { 
                    Success = false, 
                    Message = "An error occurred during login. Please try again later." 
                });
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var userName = User.Identity?.Name ?? "Unknown";

                if (_signInManager.IsSignedIn(User))
                {
                    await _signInManager.SignOutAsync();
                }

                return Ok(new 
                { 
                    Success = true, 
                    Message = "Logout successful" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Logout error: {ex.Message}");
                return StatusCode(500, new 
                { 
                    Success = false, 
                    Message = "An error occurred during logout" 
                });
            }
        }

        [HttpGet("check-auth")]
        public async Task<IActionResult> CheckAuth()
        {
            try
            {
                _logger.LogInformation($"CheckAuth called - IsAuthenticated: {User?.Identity?.IsAuthenticated}, User: {User?.Identity?.Name}");
                
                var isAuthenticated = User?.Identity?.IsAuthenticated ?? false;
                
                if (isAuthenticated && !string.IsNullOrEmpty(User.Identity.Name))
                {
                    var user = await _userManager.FindByNameAsync(User.Identity.Name);
                    if (user != null)
                    {
                        var roles = await _userManager.GetRolesAsync(user);
                        
                        return Ok(new 
                        { 
                            IsAuthenticated = true,
                            UserName = user.UserName,
                            Email = user.Email,
                            Roles = roles
                        });
                    }
                }
                
                return Ok(new 
                { 
                    IsAuthenticated = false
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking authentication status");
                return Ok(new { IsAuthenticated = false });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.UserName) || 
                    string.IsNullOrWhiteSpace(request.Email) || 
                    string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest(new 
                    { 
                        Success = false, 
                        Message = "Username, email, and password are required" 
                    });
                }

                var existingUser = await _userManager.FindByNameAsync(request.UserName);
                if (existingUser != null)
                {
                    return BadRequest(new 
                    { 
                        Success = false, 
                        Message = "Username already exists" 
                    });
                }

                var existingEmail = await _userManager.FindByEmailAsync(request.Email);
                if (existingEmail != null)
                {
                    return BadRequest(new 
                    { 
                        Success = false, 
                        Message = "Email already exists" 
                    });
                }

                var identityUser = new ApplicationUser
                {
                    UserName = request.UserName,
                    Email = request.Email,
                    EmailConfirmed = false,
                    IsActive = true,
                };

                var result = await _userManager.CreateAsync(identityUser, request.Password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return BadRequest(new 
                    { 
                        Success = false, 
                        Message = $"Registration failed: {errors}" 
                    });
                }

                // Assign role
                if (request.RoleId > 0)
                {
                    var role = await _roleManager.FindByIdAsync(request.RoleId.ToString());
                    if (role != null)
                    {
                        await _userManager.AddToRoleAsync(identityUser, role.Name!);
                    }
                }
                else
                {
                    var defaultRole = await _roleManager.FindByNameAsync("User");
                    if (defaultRole != null)
                    {
                        await _userManager.AddToRoleAsync(identityUser, "User");
                    }
                }

                _logger.LogInformation($"New user registered: {request.UserName}");
                
                return Ok(new 
                { 
                    Success = true, 
                    Message = "Registration successful. Please login." 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Registration error: {ex.Message}");
                return StatusCode(500, new 
                { 
                    Success = false, 
                    Message = "An error occurred during registration" 
                });
            }
        }
    }
}
