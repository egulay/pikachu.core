using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;

namespace Pikachu.Core.Repository.Sql.Interfaces
{
    public interface IUnitOfWork : IUnitOfWorkForService
    {
        void Save();
        Task<int> SaveAsync();
        Task<int> SaveAsync(CancellationToken cancellationToken);
        void Dispose(bool disposing);
        void Dispose();
        IDbContextTransaction CreateNewContextTransaction(IsolationLevel isolationLevel);
    }

    public interface IUnitOfWorkForService
    {
        IRepository<TEntity> Repository<TEntity>() where TEntity : class, new();
    }
}