using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChienVHShopOnline.Models;

public class News
{
    [Key]
    public int NewsId { get; set; }

    public int? UserId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string ShortDescription { get; set; } = string.Empty;

    public string? Image { get; set; }

    [Required]
    public string Content { get; set; } = string.Empty;

    public DateTime? CreatedDate { get; set; }

    public int? Status { get; set; }

    public User? User { get; set; }
}
