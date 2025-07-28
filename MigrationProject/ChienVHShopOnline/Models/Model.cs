using System.ComponentModel.DataAnnotations;

namespace ChienVHShopOnline.Models;

public class Model
{
    [Key]
    public int ModelId { get; set; }

    [Required]
    [MaxLength(100)]
    public string ModelName { get; set; } = string.Empty; // Renamed from 'Model1'

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
