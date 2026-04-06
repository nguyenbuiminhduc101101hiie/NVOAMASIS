using Microsoft.AspNetCore.Mvc;

namespace NVOAMASIS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public UploadController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost]
        public async Task<IActionResult> Upload(List<IFormFile> files)
        {
            var uploadPath = Path.Combine(_env.WebRootPath, "Uploads");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var filePath = Path.Combine(uploadPath, Path.GetFileName(file.FileName));
                    using var stream = System.IO.File.Create(filePath);
                    await file.CopyToAsync(stream);
                }
            }

            return Ok(new { message = "Upload thành công" });
        }
    }
}
