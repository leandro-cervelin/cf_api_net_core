using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CF.CustomerMngt.Domain.Repositories;
using CF.CustomerMngt.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace CF.CustomerMngt.Infrastructure.Repositories
{
    public class RepositoryBase<TEntity> : IRepositoryBase<TEntity> 
        where TEntity : class
    {
        protected readonly CustomerMngtContext DbContext;
        protected readonly DbSet<TEntity> DbSet;

        public RepositoryBase(CustomerMngtContext context)
        {
            DbContext = context;
            DbSet = DbContext.Set<TEntity>();
        }

        public virtual async Task Add(TEntity entity)
        {
            await DbSet.AddAsync(entity);
        }

        public virtual async Task<TEntity> GetById(long id)
        {
            return await DbSet.FindAsync(id);
        }
        
        public virtual async Task<IList<TEntity>> GetAll()
        {
            return await DbSet.ToListAsync();
        }
        
        public virtual void Update(TEntity entity)
        {
            DbSet.Update(entity);
        }

        public virtual void Remove(long id)
        {
            DbSet.Remove(DbSet.Find(id));
        }

        public async Task<int> SaveChanges()
        {
            return await DbContext.SaveChangesAsync();
        }
        
        public void Dispose()
        {
            DbContext.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
