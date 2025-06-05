using System.ComponentModel.DataAnnotations;

namespace NotifyAPI.Models.DTOs
{
    public class UserLoginRequest
    {
        [Required(ErrorMessage = "Username is manditory")]
        [MinLength(3,ErrorMessage ="Invalid entry for username")]
        public string Username { get; set; } = string.Empty;
        [Required(ErrorMessage = "Password is manditory")]
        public string Password { get; set; } = string.Empty;
    }
}