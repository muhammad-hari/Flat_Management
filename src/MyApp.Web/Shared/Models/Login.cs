namespace MyApp.Shared.Models
{
    public class LoginRequest
    {
        public string UserName { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class LoginResponse
    {
        public string Message { get; set; } = "";
        public string Username { get; set; } = "";
        public string Role { get; set; } = "";
    }
}
