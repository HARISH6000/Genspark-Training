using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TrainingVideoPortal.Models
{
    public class VideoUploadRequest
    {
        [FromForm]
        public IFormFile File { get; set; }

        [FromForm]
        public string Title { get; set; }

        [FromForm]
        public string Description { get; set; }
    }
}
