namespace FileOperationsViaMinio.Models
{
    public class MinioOptions
    {
        public string Endpoint { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string BucketName { get; set; }
    }
}
