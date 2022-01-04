using CF.Customer.Domain.Models;

namespace CF.Customer.Domain.Repositories;

public interface ICustomerRepository : IRepositoryBase<Entities.Customer>
{
    Task<int> CountByFilterAsync(CustomerFilter filter);
    Task<Entities.Customer> GetByFilterAsync(CustomerFilter filter);
    Task<List<Entities.Customer>> GetListByFilterAsync(CustomerFilter filter);
}