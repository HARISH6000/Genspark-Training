namespace ChienVHShopOnline.DTOs;

public class ProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Image { get; set; }
    public double? Price { get; set; }
    public string? UserName { get; set; }
    public string? CategoryName { get; set; }
    public string? ColorName { get; set; }
    public string? ModelName { get; set; }
    public DateTime? SellStartDate { get; set; }
    public DateTime? SellEndDate { get; set; }
    public int? IsNew { get; set; }
}
