
using System.ComponentModel.DataAnnotations;

namespace portfolio.Models
{
    public class AppUser
    {
        public long UserId { get; set; }

        [Required]
        public string FullName { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public string PasswordHash { get; set; } = "";

        public string Role { get; set; } = "Viewer";

        public DateTime CreatedAt { get; set; }
    }
}
