namespace NotifyAPI.Models.DTOs
{
    public class UserAddRequestDto
    {
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
