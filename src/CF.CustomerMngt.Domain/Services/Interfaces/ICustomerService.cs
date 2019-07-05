using System.Collections.Generic;
using System.Threading.Tasks;
using CF.CustomerMngt.Domain.Entities;
using CF.CustomerMngt.Domain.Models;

namespace CF.CustomerMngt.Domain.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<Pagination<Customer>> GetListByFilter(CustomerFilter filter);
        Task<Customer> GetByFilter(CustomerFilter filter);
        Task Update(long id, Customer customer);
        Task<long> Create(Customer customer);
        Task Delete(long id);
    }
}
