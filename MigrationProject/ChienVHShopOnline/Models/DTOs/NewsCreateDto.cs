namespace ChienVHShopOnline.DTOs;

public class NewsCreateDto
{
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string? Image { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public bool Status { get; set; }
}