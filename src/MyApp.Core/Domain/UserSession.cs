using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApp.Core.Domain
{
    public class UserSession
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; } = new();
        public Dictionary<string, string> Claims { get; set; } = new();
        public DateTime LoginTime { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string? PhotoUrl { get; set; }
    }
}
