namespace ChienVHShopOnline.DTOs;

public class ProductCreateDto
{
    public string ProductName { get; set; } = string.Empty;
    public double? Price { get; set; }
    public string? Description { get; set; }
    public int? Quantity { get; set; }
    public int? CategoryId { get; set; }
    public int? ModelId { get; set; }
    public int? ColorId { get; set; }
    public int? UserId { get; set; }
}