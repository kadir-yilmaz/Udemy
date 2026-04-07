using Microsoft.AspNetCore.Mvc;

namespace Udemy.PhotoStock.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PhotosController : ControllerBase
    {
        private readonly string _wwwrootPath;

        public PhotosController(IWebHostEnvironment environment)
        {
            // WebRootPath can be null if wwwroot doesn't exist, fallback to ContentRootPath
            _wwwrootPath = environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");
        }

        [HttpPost]
        public async Task<IActionResult> PhotoSave([FromForm] IFormFile photo, CancellationToken cancellationToken)
        {
            if (photo == null || photo.Length == 0)
                return BadRequest("Photo is empty");

            var photosFolder = Path.Combine(_wwwrootPath, "photos");

            if (!Directory.Exists(photosFolder))
            {
                Directory.CreateDirectory(photosFolder);
            }

            // Use the filename provided by the client (sanitized)
            var fileName = Path.GetFileName(photo.FileName);
            // Fallback to GUID if filename is missing (should not happen)
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = Guid.NewGuid() + Path.GetExtension(photo.FileName);
            }
            var filePath = Path.Combine(photosFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await photo.CopyToAsync(stream, cancellationToken);

            var photoUrl = $"photos/{fileName}";

            return Ok(new
            {
                url = photoUrl
            });
        }

        [HttpDelete("{fileName}")]
        public IActionResult PhotoDelete(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest("fileName is required");
            }

            var filePath = Path.Combine(_wwwrootPath, "photos", fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Photo not found");
            }

            System.IO.File.Delete(filePath);

            return NoContent();
        }

    }
}

