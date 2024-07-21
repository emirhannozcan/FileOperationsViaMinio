using FileOperationsViaMinio.Contracts;
using FileOperationsViaMinio.Models;
using FileOperationsViaMinio.Repositories;
using FileOperationsViaMinio.Services;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace FileOperationsViaMinio.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMinio(
            this IServiceCollection services)
        {
            MinioOptions minioOptions = services.BuildServiceProvider().GetRequiredService<IOptions<MinioOptions>>().Value;

            return services.AddMinio(configureClient => configureClient
            .WithEndpoint(minioOptions.Endpoint)
            .WithCredentials(minioOptions.AccessKey, minioOptions.SecretKey)
            .WithSSL(false));
        }

        public static async Task CreateDefaultBucket(this IServiceCollection services)
        {
            try
            {
                MinioOptions minioOptions = services.BuildServiceProvider().GetRequiredService<IOptions<MinioOptions>>().Value;
                var minioClient = services.BuildServiceProvider().GetRequiredService<IMinioClient>();
                BucketExistsArgs args = new BucketExistsArgs().WithBucket(minioOptions.BucketName);

                bool bucketExists = await minioClient.BucketExistsAsync(args);
                if (!bucketExists)
                {
                    MakeBucketArgs makeBucketArgs = new MakeBucketArgs().WithBucket(minioOptions.BucketName);
                    await minioClient.MakeBucketAsync(makeBucketArgs);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void AddApplicationServices(this IServiceCollection services)
        {
            services.AddTransient<IFileRepository, FileRepository>();
            services.AddTransient<IFileService, FileService>();
        }
    }
}
