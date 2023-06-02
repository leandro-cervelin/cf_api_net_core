namespace CF.Customer.Domain.Repositories;

public interface IRepositoryBase<TEntity> : IDisposable where TEntity : class
{
    /// <summary>
    ///     This method is async only to allow special value generators, such as the one used by
    ///     'Microsoft.EntityFrameworkCore.Metadata.SqlServerValueGenerationStrategy.SequenceHiLo', to access the database
    ///     asynchronously. For all other cases the non async method should be used.
    ///     https://docs.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbset-1?view=efcore-3.1#Microsoft_EntityFrameworkCore_DbSet_1_AddAsync__0_System_Threading_CancellationToken_
    /// </summary>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken);

    void Add(TEntity entity);
    Task<TEntity> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task<IList<TEntity>> GetAllAsync(CancellationToken cancellationToken);
    void Update(TEntity entity);
    void Remove(TEntity entity);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}