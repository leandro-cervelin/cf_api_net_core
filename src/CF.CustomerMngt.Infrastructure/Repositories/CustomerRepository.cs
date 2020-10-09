using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CF.CustomerMngt.Domain.Entities;
using CF.CustomerMngt.Domain.Models;
using CF.CustomerMngt.Domain.Repositories;
using CF.CustomerMngt.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace CF.CustomerMngt.Infrastructure.Repositories
{
    public class CustomerRepository : RepositoryBase<Customer>, ICustomerRepository
    {
        public CustomerRepository(CustomerMngtContext context) : base(context)
        {
        }

        public async Task<int> CountByFilterAsync(CustomerFilter filter)
        {
            var query = DbContext.Customers.AsQueryable();

            query = ApplyFilter(filter, query);

            return await query.CountAsync();
        }

        public async Task<Customer> GetByFilterAsync(CustomerFilter filter)
        {
            var query = DbContext.Customers.AsQueryable();

            query = ApplyFilter(filter, query);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<Customer>> GetListByFilterAsync(CustomerFilter filter)
        {
            var query = DbContext.Customers.AsQueryable();

            query = ApplyFilter(filter, query);

            query = ApplySorting(filter, query);

            if (filter.CurrentPage > 0)
                query = query.Skip((filter.CurrentPage - 1) * filter.PageSize).Take(filter.PageSize);

            return await query.ToListAsync();
        }

        private static IQueryable<Customer> ApplySorting(CustomerFilter filter, IQueryable<Customer> query)
        {
            if (filter?.OrderBy.ToLower() == "firstname")
                query = filter.SortBy.ToLower() == "asc"
                    ? query.OrderBy(x => x.FirstName)
                    : query.OrderByDescending(x => x.FirstName);

            if (filter?.OrderBy.ToLower() == "surname")
                query = filter.SortBy.ToLower() == "asc"
                    ? query.OrderBy(x => x.Surname)
                    : query.OrderByDescending(x => x.Surname);

            if (filter?.OrderBy.ToLower() == "email")
                query = filter.SortBy.ToLower() == "asc"
                    ? query.OrderBy(x => x.Email)
                    : query.OrderByDescending(x => x.Email);

            return query;
        }

        private static IQueryable<Customer> ApplyFilter(CustomerFilter filter, IQueryable<Customer> query)
        {
            if (filter.Id > 0)
                query = query.Where(x => x.Id == filter.Id);

            if (!string.IsNullOrWhiteSpace(filter.FirstName))
                query = query.Where(x => x.FirstName.Contains(filter.FirstName));

            if (!string.IsNullOrWhiteSpace(filter.Surname))
                query = query.Where(x => x.Surname.Contains(filter.Surname));

            if (!string.IsNullOrWhiteSpace(filter.Email))
                query = query.Where(x => x.Email.Contains(filter.Email));

            return query;
        }
    }
}