namespace MyApp.Core.Domain
{
    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public string RedirectUrl { get; set; } = "/";
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public List<string>? Roles { get; set; }
        public bool RequiresTwoFactor { get; set; }
    }
}
