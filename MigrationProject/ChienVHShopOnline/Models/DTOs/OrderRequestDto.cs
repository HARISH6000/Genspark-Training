namespace ChienVHShopOnline.DTOs;

public class OrderRequestDto
{
    public int UserId { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerAddress { get; set; } = string.Empty;
    public List<CartItemDto> Items { get; set; } = new();
}
