using FileOperationsViaMinio.Contracts;
using FileOperationsViaMinio.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace FileOperationsViaMinio.Repositories
{
    public class FileRepository : IFileRepository
    {
        protected readonly ApplicationContext _context;
        protected readonly DbSet<FileEntity> _dbSet;

        public FileRepository(ApplicationContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<FileEntity>();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _dbSet.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public IQueryable<FileEntity> GetAll()
        {
            return _dbSet.AsNoTracking();
        }

        public async Task<FileEntity> GetByIdAsync(Guid id)
        {
            return await _dbSet.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task InsertAsync(FileEntity entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
