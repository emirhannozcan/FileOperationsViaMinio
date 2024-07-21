using FileOperationsViaMinio.Models.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace FileOperationsViaMinio.Contracts
{
    public interface IFileService
    {
        Task<Guid> Upload(IFormFile file, bool makeObjectPublic = false);
        Task DeleteFile(Guid id);
        Task<FileContentResult> DownloadFile(Guid id);
        Task<FileStreamResult> DownloadFileStream(Guid id);
        Task<string> GetFileLink(Guid id);
        Task<string> GetPublicFileLink(Guid id);
    }
}
