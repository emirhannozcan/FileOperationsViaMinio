using FileOperationsViaMinio.Models.Entities;

namespace FileOperationsViaMinio.Contracts
{
    public interface IFileRepository
    {
        public IQueryable<FileEntity> GetAll();
        public Task<FileEntity> GetByIdAsync(Guid id);
        public Task InsertAsync(FileEntity entity);
        public Task DeleteAsync(Guid id);
        public Task SaveChangesAsync();
    }
}
