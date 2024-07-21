using FileOperationsViaMinio.Extensions;
using FileOperationsViaMinio.Models;
using Microsoft.EntityFrameworkCore;

namespace FileOperationsViaMinio
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            #region FileOperationsViaMinio
            builder.Services.Configure<MinioOptions>(builder.Configuration.GetSection("MinioOptions"));
            builder.Services.AddMinio();
            builder.Services.CreateDefaultBucket().Wait();
            builder.Services.AddApplicationServices();
            #endregion

            #region DbContext
            builder.Services.AddDbContext<ApplicationContext>(options =>
                options.UseNpgsql(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationContext).Assembly.FullName)));
            #endregion

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
