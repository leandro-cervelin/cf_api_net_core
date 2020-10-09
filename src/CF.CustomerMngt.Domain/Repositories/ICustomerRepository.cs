using System.Collections.Generic;
using System.Threading.Tasks;
using CF.CustomerMngt.Domain.Entities;
using CF.CustomerMngt.Domain.Models;

namespace CF.CustomerMngt.Domain.Repositories
{
    public interface ICustomerRepository : IRepositoryBase<Customer>
    {
        Task<int> CountByFilterAsync(CustomerFilter filter);
        Task<Customer> GetByFilterAsync(CustomerFilter filter);
        Task<List<Customer>> GetListByFilterAsync(CustomerFilter filter);
    }
}