using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CF.CustomerMngt.Domain.Repositories
{
    public interface IRepositoryBase<TEntity> : IDisposable where TEntity : class
    {
        Task AddAsync(TEntity entity);
        Task<TEntity> GetByIdAsync(long id);
        Task<IList<TEntity>> GetAllAsync();
        void Update(TEntity entity);
        void Remove(long id);
        Task<int> SaveChangesAsync();
    }
}
