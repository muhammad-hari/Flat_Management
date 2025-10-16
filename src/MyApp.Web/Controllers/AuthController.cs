using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyApp.Infrastructure.Identity;
using MyApp.Core.Entities;
using MyApp.Core.Interfaces;

namespace MyApp.Web.Controllers
{
    [ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserRepository _userRepository;

    public AuthController(SignInManager<ApplicationUser> signInManager,
                          UserManager<ApplicationUser> userManager,
                          IUserRepository userRepository)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _userRepository = userRepository;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // 1. Ambil user dari domain repository
        var domainUser = await _userRepository.GetByUserNameAsync(request.UserName);
        if (domainUser == null)
            return Unauthorized(new { Message = "User not exist" });

        // 2. Cari Identity user
        var identityUser = await _userManager.FindByNameAsync(request.UserName);
        if (identityUser == null)
        {
            // Jika belum ada di Identity, otomatis buat
            identityUser = new ApplicationUser
            {
                UserName = domainUser.UserName,
                Email = domainUser.Email
            };
            var identityResult = await _userManager.CreateAsync(identityUser, "DefaultPassword123!"); 
            // bisa ganti dengan domainUser.PasswordHash jika sudah hashed
            if (!identityResult.Succeeded)
                return BadRequest(identityResult.Errors);
        }

        // 3. Login via Identity
        var result = await _signInManager.PasswordSignInAsync(identityUser, request.Password, true, false);
        if (!result.Succeeded)
            return Unauthorized(new { Message = "Password wrong" });

        return Ok(new { Message = "Login success" });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // 1. Cek domain repository dulu
        var exists = await _userRepository.GetByUserNameAsync(request.UserName);
        if (exists != null)
            return BadRequest(new { Message = "Username already exists" });

        // 2. Buat Identity user
        var identityUser = new ApplicationUser
        {
            UserName = request.UserName,
            Email = request.Email
        };
        var result = await _userManager.CreateAsync(identityUser, request.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        // 3. Simpan di domain repository
        var domainUser = new User
        {
            UserName = request.UserName,
            Email = request.Email,
            PasswordHash = identityUser.PasswordHash ?? "",
            RoleId = request.RoleId,
            IsActive = true,
            CreatedBy = "system"
        };
        await _userRepository.AddAsync(domainUser);

        return Ok(new { Message = "Registrasi berhasil" });
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
    public int RoleId { get; set; }
}

}
