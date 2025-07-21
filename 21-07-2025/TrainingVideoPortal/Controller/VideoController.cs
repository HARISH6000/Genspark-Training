using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingVideoPortal.Data;
using TrainingVideoPortal.Models;
using TrainingVideoPortal.Services;

namespace TrainingVideoPortal.Controllers
{
    [ApiController]
    [Route("api/videos")]
    public class VideosController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly BlobService _blobService;
        private readonly ILogger<VideosController> _logger;

        public VideosController(AppDbContext db, BlobService blobService, ILogger<VideosController> logger)
        {
            _db = db;
            _blobService = blobService;
            _logger = logger;
        }

        // GET: api/videos
        [HttpGet]
        public async Task<IActionResult> GetVideos()
        {
            _logger.LogInformation("Fetching all training videos.");
            var videos = await _db.TrainingVideos.ToListAsync();
            for (int i = 0; i < videos.Count; i++)
            {
                videos[i].BlobUrl = _blobService.GetSasUrlFromBlobUrl(videos[i].BlobUrl);
            }
            return Ok(videos);
        }

        // POST: api/videos/upload
        [HttpPost("upload")]
        public async Task<IActionResult> UploadVideo([FromForm] VideoUploadRequest request)
        {
            if (request.File == null || request.File.Length == 0)
            {
                return BadRequest(new { error = "No video file uploaded." });
            }

            var blobUrl = await _blobService.UploadFileAsync(request.File);

            var video = new TrainingVideo
            {
                Title = request.Title,
                Description = request.Description,
                UploadDate = DateTime.UtcNow,
                BlobUrl = blobUrl
            };

            _db.TrainingVideos.Add(video);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetVideoById), new { id = video.Id }, video);
        }

        // GET: api/videos/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetVideoById(int id)
        {
            var video = await _db.TrainingVideos.FindAsync(id);
            if (video == null)
                return NotFound();
            var sasUrl = _blobService.GetSasUrlFromBlobUrl(video.BlobUrl);
            video.BlobUrl = sasUrl; // Update BlobUrl with SAS URL
            return Ok(video);
        }

        // GET: api/videos/5/stream
        [HttpGet("{id:int}/stream")]
        public async Task<IActionResult> StreamVideo(int id)
        {
            var video = await _db.TrainingVideos.FindAsync(id);
            if (video == null)
                return NotFound();

            var sasUrl = _blobService.GetSasUrlFromBlobUrl(video.BlobUrl);
            return Ok(new { url = sasUrl });
        }
    }
}
