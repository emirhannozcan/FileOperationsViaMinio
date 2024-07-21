namespace FileOperationsViaMinio.Models.Entities
{
    public class FileEntity
    {
        public Guid Id { get; set; }
        public string FileType { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public long FileLength { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
