using CF.Customer.Domain.Models;

namespace CF.Customer.Domain.Repositories;

public interface ICustomerRepository : IRepositoryBase<Entities.Customer>
{
    Task<int> CountByFilterAsync(CustomerFilter filter, CancellationToken cancellationToken);
    Task<Entities.Customer> GetByFilterAsync(CustomerFilter filter, CancellationToken cancellationToken);
    Task<List<Entities.Customer>> GetListByFilterAsync(CustomerFilter filter, CancellationToken cancellationToken);
}