namespace FileOperationsViaMinio.Models.Dtos
{
    public class FileItemDto
    {
        public Guid FileId { get; set; }
        public string FileType { get; set; }
        public string FileName { get; set; }
        public long FileLength { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
