namespace FirstAPI.Models.DTOs
{
    public class FileUploadReturnDTO
    {
        public string? Inserted { get; set; }

        public string[]? Errors { get; set; }
    }
}