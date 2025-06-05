using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.IO;
using System.Threading.Tasks;
using NotifyAPI.Misc;

[Authorize]
[ApiController]
[Route("api/documents")]
public class DocumentController : ControllerBase
{
    private readonly IHubContext<DocumentHub> _hubContext;
    private readonly string _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

    public DocumentController(IHubContext<DocumentHub> hubContext)
    {
        _hubContext = hubContext;

        if (!Directory.Exists(_uploadPath))
            Directory.CreateDirectory(_uploadPath);
    }

    [HttpPost("upload")]
    [Authorize(Roles = "HRAdmin")]
    public async Task<IActionResult> UploadDocument([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Invalid file.");

        var filePath = Path.Combine(_uploadPath, file.FileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        //await _hubContext.Clients.All.SendAsync("DocumentAdded", file.FileName);

        return Ok(new { message = "File uploaded successfully.", fileName = file.FileName });
    }

    [HttpGet("{fileName}")]
    [Authorize]
    public IActionResult GetFile(string fileName)
    {
        var filePath = Path.Combine(_uploadPath, fileName);

        if (!System.IO.File.Exists(filePath))
            return NotFound("File not found.");

        var mimeType = "application/octet-stream"; // Default MIME type
        return PhysicalFile(filePath, mimeType, fileName);
    }
}
