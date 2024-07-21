using FileOperationsViaMinio.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace FileOperationsViaMinio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IFileService _fileService;

        public FileController(IFileService fileService)
        {
            _fileService = fileService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file, bool makeObjectPublic = false)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is empty");

            var fileId = await _fileService.Upload(file, makeObjectPublic);
            return Ok(new { Id = fileId });
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteFile([FromBody] Guid id)
        {
            await _fileService.DeleteFile(id);
            return Ok();
        }

        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadFile(Guid id)
        {
            var fileResult = await _fileService.DownloadFile(id);
            return fileResult;
        }

        [HttpGet("download-stream/{id}")]
        public async Task<IActionResult> DownloadFileStream(Guid id)
        {
            var fileResult = await _fileService.DownloadFileStream(id);
            return fileResult;
        }

        [HttpGet("get-link/{id}")]
        public async Task<IActionResult> GetFileLink(Guid id)
        {
            var link = await _fileService.GetFileLink(id);
            return Ok(new { Link = link });
        }

        [HttpGet("get-public-link/{id}")]
        public async Task<IActionResult> GetPublicFileLink(Guid id)
        {
            var link = await _fileService.GetPublicFileLink(id);
            return Ok(new { Link = link });
        }
    }
}
