using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace blob_docx_repository_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly FileService _fileService;

        public FilesController(FileService fileService)
        {
            _fileService = fileService;
        }

        // POST api/<FileController>
        [HttpPost]
        public async Task<IActionResult> UploadFileAsync(IFormFile file, string email)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("File not selected or empty");

            }
            string fileName = Path.GetFileName(file.FileName);
            string fileExtension = Path.GetExtension(fileName).ToLower();

            if (fileExtension == ".docx")
            {
                var result = await _fileService.UploadAsync(file, email);
                return Ok(result);
            }
            else
            {
                return BadRequest("Invalid file format");
            }
        }
    }
}
    