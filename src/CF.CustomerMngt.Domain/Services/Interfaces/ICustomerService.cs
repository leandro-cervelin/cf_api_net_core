using System.Threading.Tasks;
using CF.CustomerMngt.Domain.Entities;
using CF.CustomerMngt.Domain.Models;

namespace CF.CustomerMngt.Domain.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<Pagination<Customer>> GetListByFilterAsync(CustomerFilter filter);
        Task<Customer> GetByFilterAsync(CustomerFilter filter);
        Task UpdateAsync(long id, Customer customer);
        Task<long> CreateAsync(Customer customer);
        Task DeleteAsync(long id);
    }
}
