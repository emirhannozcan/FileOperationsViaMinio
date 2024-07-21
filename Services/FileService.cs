using FileOperationsViaMinio.Contracts;
using FileOperationsViaMinio.Models;
using FileOperationsViaMinio.Models.Entities;
using FileOperationsViaMinio.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using System.Text.Json;

namespace FileOperationsViaMinio.Services
{
    public class FileService : IFileService
    {
        private readonly MinioOptions _minioOptions;
        private readonly IMinioClient _minioClient;
        private readonly IFileRepository _fileRepository;

        public FileService(IOptions<MinioOptions> minioOptions, IMinioClient minioClient, IFileRepository fileRepository)
        {
            _minioOptions = minioOptions.Value;
            _minioClient = minioClient;
            _fileRepository = fileRepository;
        }

        public async Task DeleteFile(Guid id)
        {
            try
            {
                var file = await _fileRepository.GetByIdAsync(id);
                RemoveObjectArgs rmArgs = new RemoveObjectArgs().WithBucket(_minioOptions.BucketName).WithObject(file.FilePath);
                await _minioClient.RemoveObjectAsync(rmArgs);
                await _fileRepository.DeleteAsync(id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<FileContentResult> DownloadFile(Guid id)
        {
            try
            {
                var file = await _fileRepository.GetByIdAsync(id);
                using (var memoryStream = new MemoryStream())
                {
                    var getArgs = new GetObjectArgs()
                    .WithObject(file.FilePath)
                    .WithBucket(_minioOptions.BucketName)
                    .WithCallbackStream((stream) =>
                    {
                        stream.CopyTo(memoryStream);
                    });

                    var getObjectResponse = await _minioClient.GetObjectAsync(getArgs);
                    memoryStream.Position = 0;

                    return new FileContentResult(memoryStream.ToByteArray(), getObjectResponse.ContentType)
                    {
                        FileDownloadName = file.FileName
                    };
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<FileStreamResult> DownloadFileStream(Guid id)
        {
            try
            {
                var file = await _fileRepository.GetByIdAsync(id);
                var statArgs = new StatObjectArgs().WithObject(file.FilePath).WithBucket(_minioOptions.BucketName);
                var stat = await _minioClient.StatObjectAsync(statArgs);

                var memoryStream = new MemoryStream();

                var getArgs = new GetObjectArgs()
                    .WithObject(file.FilePath)
                    .WithBucket(_minioOptions.BucketName)
                    .WithCallbackStream(async (stream) =>
                    {
                        await stream.CopyToAsync(memoryStream);
                    });

                var getObjectResponse = await _minioClient.GetObjectAsync(getArgs);
                memoryStream.Position = 0;

                return new FileStreamResult(memoryStream, stat.ContentType)
                {
                    FileDownloadName = file.FileName
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        //Returns the link signed by minio 
        public async Task<string> GetFileLink(Guid id)
        {
            try
            {
                var entity = await _fileRepository.GetByIdAsync(id);
                PresignedGetObjectArgs args = new PresignedGetObjectArgs().WithBucket(_minioOptions.BucketName)
                                                                          .WithObject(entity.FilePath)
                                                                          .WithExpiry(60 * 60 * 24);

                return await _minioClient.PresignedGetObjectAsync(args);
            }
            catch (Exception)
            {
                throw;
            }
        }

        // Generate and return the public link of the file by setting the necessary policy in Minio.
        public async Task<string> GetPublicFileLink(Guid id)
        {
            FileEntity entity = await _fileRepository.GetByIdAsync(id);
            return $"{_minioOptions.Endpoint}/{_minioOptions.BucketName}/{entity.FilePath}";
        }

        public async Task<Guid> Upload(IFormFile file, bool makeObjectPublic)
        {
            try
            {
                // Create a new FileEntity object with the file's metadata
                FileEntity entity = new FileEntity
                {
                    FileType = file.ContentType,
                    FileName = file.FileName,
                    FileLength = file.Length,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                // Insert the new entity into the repository
                await _fileRepository.InsertAsync(entity);

                // Generate the file path for Minio storage
                entity.FilePath = CreateMinioPath(entity.Id, entity.FileName);

                // Prepare the arguments for uploading the file to Minio
                var putObjectArg = new PutObjectArgs().WithBucket(_minioOptions.BucketName).WithObject(entity.FilePath).WithStreamData(file.OpenReadStream()).WithObjectSize(file.Length).WithContentType(file.ContentType);
                
                // Upload the file to Minio
                await _minioClient.PutObjectAsync(putObjectArg);

                // Save changes to the repository
                await _fileRepository.SaveChangesAsync();

                // Make the file public if the flag is set
                if (makeObjectPublic)
                    await MakeObjectPublic(entity.FilePath);

                // Return the ID of the newly created file entity
                return entity.Id;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string CreateMinioPath(Guid entityId, string fileName)
        {
            // Crate File Path Inside Minio
            return MinioUtil.Join([DateTime.Now.ToString("yyyyMMdd"), entityId.ToString(), fileName]);
        }

        private async Task MakeObjectPublic(string objectName)
        {
            // Get Existing Policy
            var existingPolicy = await GetExistingPolicyAsync();

            // Add New Resource
            var newResource = $"arn:aws:s3:::{_minioOptions.BucketName}/{objectName}";
            var policy = AddResourceToPolicy(existingPolicy, newResource);

            SetPolicyArgs args = new SetPolicyArgs().WithBucket(_minioOptions.BucketName).WithPolicy(policy);
            await _minioClient.SetPolicyAsync(args).ConfigureAwait(false);
        }

        private async Task<string> GetExistingPolicyAsync()
        {
            try
            {
                // Get the current policy for the bucket
                var args = new GetPolicyArgs().WithBucket(_minioOptions.BucketName);
                var policy = await _minioClient.GetPolicyAsync(args).ConfigureAwait(false);

                // No existing policy, return a default policy template
                if (policy == null)
                    return @"{""Version"": ""2012-10-17"", ""Statement"": []}";

                return policy;
            }
            catch (MinioException)
            {
                // No existing policy, return a default policy template
                return @"{""Version"": ""2012-10-17"", ""Statement"": []}";
            }
        }

        private string AddResourceToPolicy(string existingPolicy, string newResource)
        {
            var policyDocument = JsonDocument.Parse(existingPolicy);
            var root = policyDocument.RootElement.Clone();

            var statements = root.GetProperty("Statement").EnumerateArray().ToList();
            var allowStatement = statements.FirstOrDefault(st => st.GetProperty("Effect").GetString() == "Allow" &&
                                                                  st.GetProperty("Action").EnumerateArray().Any(a => a.GetString() == "s3:GetObject"));

            if (allowStatement.ValueKind == JsonValueKind.Undefined)
            {
                // Create a new statement if none exists
                var newStatement = JsonDocument.Parse($@"{{
                    ""Effect"": ""Allow"",
                    ""Principal"": {{""AWS"":[""*""]}},
                    ""Action"": [""s3:GetObject""],
                    ""Resource"": [""{newResource}""]
                }}").RootElement.Clone();

                statements.Add(newStatement);
            }
            else
            {
                // Add the new resource to the existing statement
                var resources = allowStatement.GetProperty("Resource").EnumerateArray().Select(r => r.GetString()).ToList();
                if (!resources.Contains(newResource))
                {
                    resources.Add(newResource);
                    allowStatement = JsonDocument.Parse($@"{{
                        ""Effect"": ""Allow"",
                        ""Principal"": {{""AWS"":[""*""]}},
                        ""Action"": [""s3:GetObject""],
                        ""Resource"": [{string.Join(",", resources.Select(r => $"\"{r}\""))}]
                    }}").RootElement.Clone();

                    var index = statements.FindIndex(st => st.GetProperty("Effect").GetString() == "Allow" &&
                                                           st.GetProperty("Action").EnumerateArray().Any(a => a.GetString() == "s3:GetObject"));
                    statements[index] = allowStatement;
                }
            }

            var newRoot = JsonDocument.Parse($@"{{""Version"": ""2012-10-17"", ""Statement"": [{string.Join(",", statements)}]}}").RootElement.Clone();
            return newRoot.GetRawText();
        }
    }
}
