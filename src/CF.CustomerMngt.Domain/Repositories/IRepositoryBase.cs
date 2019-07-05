using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CF.CustomerMngt.Domain.Repositories
{
    public interface IRepositoryBase<TEntity> : IDisposable where TEntity : class
    {
        Task Add(TEntity entity);
        Task<TEntity> GetById(long id);
        Task<IList<TEntity>> GetAll();
        void Update(TEntity entity);
        void Remove(long id);
        Task<int> SaveChanges();
    }
}
