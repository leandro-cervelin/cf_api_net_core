﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CF.Customer.Domain.Repositories;
using CF.Customer.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace CF.Customer.Infrastructure.Repositories
{
    public class RepositoryBase<TEntity> : IRepositoryBase<TEntity>
        where TEntity : class
    {
        protected readonly CustomerContext DbContext;
        protected readonly DbSet<TEntity> DbSet;

        public RepositoryBase(CustomerContext context)
        {
            DbContext = context;
            DbSet = DbContext.Set<TEntity>();
        }

        public void Add(TEntity entity)
        {
            DbSet.Add(entity);
        }

        /// <summary>
        ///     This method is async only to allow special value generators, such as the one used by
        ///     'Microsoft.EntityFrameworkCore.Metadata.SqlServerValueGenerationStrategy.SequenceHiLo', to access the database
        ///     asynchronously. For all other cases the non async method should be used.
        ///     https://docs.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbset-1?view=efcore-3.1#Microsoft_EntityFrameworkCore_DbSet_1_AddAsync__0_System_Threading_CancellationToken_
        /// </summary>
        public virtual async Task AddAsync(TEntity entity)
        {
            await DbSet.AddAsync(entity);
        }

        public virtual async Task<TEntity> GetByIdAsync(long id)
        {
            return await DbSet.FindAsync(id);
        }

        public virtual async Task<IList<TEntity>> GetAllAsync()
        {
            return await DbSet.ToListAsync();
        }

        public virtual void Update(TEntity entity)
        {
            DbSet.Update(entity);
        }

        public virtual void Remove(TEntity entity)
        {
            DbSet.Remove(entity);
        }

        public async Task<int> SaveChangesAsync()
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