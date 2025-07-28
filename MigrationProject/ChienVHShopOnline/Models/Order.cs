using System.ComponentModel.DataAnnotations;

namespace ChienVHShopOnline.Models;

public class Order
{
    [Key]
    public int OrderID { get; set; }

    [Required]
    [MaxLength(100)]
    public string OrderName { get; set; } = string.Empty;

    public DateTime? OrderDate { get; set; }

    [MaxLength(50)]
    public string PaymentType { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Status { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string CustomerPhone { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [EmailAddress]
    public string CustomerEmail { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string CustomerAddress { get; set; } = string.Empty;

    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
