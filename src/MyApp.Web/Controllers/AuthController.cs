using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyApp.Infrastructure.Identity;
using MyApp.Core.Entities;
using MyApp.Core.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace MyApp.Web.Controllers
{
    [ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;

    public AuthController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IUserRepository userRepository,
        IRoleRepository roleRepository)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var exists = await _userRepository.GetByUserNameAsync(request.UserName);
        if (exists != null)
            return BadRequest(new { Message = "Username already exists" });

        var role = await _roleRepository.GetByIdAsync(request.RoleId);
        if (role == null)
        {
            role = await _roleRepository.GetByNameAsync("User");
            if (role == null)
                return BadRequest(new { Message = "Default role 'User' not found" });
        }

        var identityUser = new ApplicationUser
        {
            UserName = request.UserName,
            Email = request.Email
        };

        var result = await _userManager.CreateAsync(identityUser, request.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        // Tambahkan role claim ke Identity
        await _userManager.AddClaimAsync(identityUser, new Claim(ClaimTypes.Role, role.RoleName));

        // Simpan di domain repository
        var domainUser = new User
        {
            UserName = request.UserName,
            Email = request.Email,
            RoleId = role.Id,
            IsActive = true,
            CreatedBy = "system"
        };
        await _userRepository.AddAsync(domainUser);

        return Ok(new { Message = "Registrasi berhasil", Role = role.RoleName });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var domainUser = await _userRepository.GetByUserNameAsync(request.UserName);
        if (domainUser == null)
            return Unauthorized(new { Message = "User not exist" });

        var identityUser = await _userManager.FindByNameAsync(request.UserName);
        if (identityUser == null)
        {
            identityUser = new ApplicationUser
            {
                UserName = domainUser.UserName,
                Email = domainUser.Email,
            };
            var identityResult = await _userManager.CreateAsync(identityUser, "DefaultPassword123!");
            if (!identityResult.Succeeded)
                return BadRequest(identityResult.Errors);
        }

        var passwordValid = await _userManager.CheckPasswordAsync(identityUser, request.Password);
        if (!passwordValid)
            return Unauthorized(new { Message = "Password wrong" });

        // Pastikan role claim ada
        var claims = await _userManager.GetClaimsAsync(identityUser);
            if (!claims.Any(c => c.Type == ClaimTypes.Role))
            {
                await _userManager.AddClaimAsync(identityUser, new Claim(ClaimTypes.Role, domainUser.Role?.RoleName ?? "User"));
            }

        // Login via SignInManager agar cookie menyimpan claim
        var identity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
       await _signInManager.Context.SignInAsync(
            IdentityConstants.ApplicationScheme,
            new ClaimsPrincipal(identity));

        return Ok(new
        {
            Message = "Login success",
            UserName = domainUser.UserName,
            Role = domainUser.Role?.RoleName ?? "User"
        });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok(new { Message = "Logout success" });
    }
}


    public class LoginRequest
    {
        public string UserName { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class RegisterRequest
    {
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public int RoleId { get; set; } // optional: bisa default ke "User"
    }
}
