namespace ChienVHShopOnline.DTOs;

public class OrderReadDto
{
    public int OrderID { get; set; }
    public string? CustomerName { get; set; }
    public DateTime? OrderDate { get; set; }
    public double? TotalAmount { get; set; }
    public string Status { get; set; }
}
