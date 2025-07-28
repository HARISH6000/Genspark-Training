using System.ComponentModel.DataAnnotations;

namespace ChienVHShopOnline.Models;

public class User
{
    [Key]
    public int UserId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Password { get; set; } = string.Empty;

    public ICollection<News> News { get; set; } = new List<News>();

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
