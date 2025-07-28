using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChienVHShopOnline.Models;

public class Product
{
    [Key]
    public int ProductId { get; set; }

    [Required, MaxLength(255)]
    public string ProductName { get; set; } = string.Empty;

    public string? Image { get; set; }

    public double? Price { get; set; }

    public int? UserId { get; set; }
    public User? User { get; set; }

    public int? CategoryId { get; set; }
    public Category? Category { get; set; }

    public int? ColorId { get; set; }
    public Color? Color { get; set; }

    public int? ModelId { get; set; }
    public Model? Model { get; set; }

    public int? StorageId { get; set; }

    public DateTime? SellStartDate { get; set; }

    public DateTime? SellEndDate { get; set; }

    public int? IsNew { get; set; }

    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
