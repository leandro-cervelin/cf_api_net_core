using System.Threading.Tasks;
using CF.Customer.Domain.Models;

namespace CF.Customer.Domain.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<Pagination<Entities.Customer>> GetListByFilterAsync(CustomerFilter filter);
        Task<Entities.Customer> GetByFilterAsync(CustomerFilter filter);
        Task UpdateAsync(long id, Entities.Customer customer);
        Task<long> CreateAsync(Entities.Customer customer);
        Task DeleteAsync(long id);
    }
}