using CF.Customer.Domain.Models;

namespace CF.Customer.Domain.Services.Interfaces;

public interface ICustomerService
{
    Task<Pagination<Entities.Customer>>
        GetListByFilterAsync(CustomerFilter filter, CancellationToken cancellationToken);
    Task<Entities.Customer> GetByFilterAsync(CustomerFilter filter, CancellationToken cancellationToken);
    Task UpdateAsync(long id, Entities.Customer customer, CancellationToken cancellationToken);
    Task<long> CreateAsync(Entities.Customer customer, CancellationToken cancellationToken);
    Task DeleteAsync(long id, CancellationToken cancellationToken);
}