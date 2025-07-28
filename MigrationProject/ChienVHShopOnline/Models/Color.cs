using System.ComponentModel.DataAnnotations;

namespace ChienVHShopOnline.Models;

public class Color
{
    [Key]
    public int ColorId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty; // Renamed from 'Color1'

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
