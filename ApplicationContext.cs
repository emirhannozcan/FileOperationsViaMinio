using FileOperationsViaMinio.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace FileOperationsViaMinio
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
        }
        public DbSet<FileEntity> Files { get; set; }
    }
}
