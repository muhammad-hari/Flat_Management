namespace MyApp.Core.Domain
{
    public class RegisterRequest
    {
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public int RoleId { get; set; } = 0;
    }
}
