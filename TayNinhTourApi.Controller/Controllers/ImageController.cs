using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Image;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;

namespace TayNinhTourApi.Controller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IImageService _imageService;

        public ImageController(IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor, IImageService imageService)
        {
            this.webHostEnvironment = webHostEnvironment;
            this.httpContextAccessor = httpContextAccessor;
            _imageService = imageService;
        }

        [HttpPost("Upload")]
        public async Task<IActionResult> UploadImage([FromForm] IFormFileCollection files)
        {
            if (files == null || files.Count == 0)
                return BadRequest("No files were uploaded.");

            var imageDtos = new List<RequestImageUploadDto>();

            foreach (var file in files)
            {
                using (var memoryStream = new MemoryStream())
                {
                    if (file.Length > 0)
                    {
                        await file.CopyToAsync(memoryStream);
                        var fileContent = memoryStream.ToArray();
                        imageDtos.Add(new RequestImageUploadDto
                        {
                            FileName = file.FileName,
                            FileContent = fileContent,
                            FileExtension = Path.GetExtension(file.FileName)
                        });
                    }
                }
            }

            var localRootPath = Path.Combine(webHostEnvironment.ContentRootPath, "Images");
            var urlPath = $"{httpContextAccessor.HttpContext!.Request.Scheme}://{httpContextAccessor.HttpContext.Request.Host}{httpContextAccessor.HttpContext.Request.PathBase}/Images";
            var response = await _imageService.UploadImage(imageDtos, localRootPath, urlPath);
            return StatusCode(response.StatusCode, response);
        }
    }
}
