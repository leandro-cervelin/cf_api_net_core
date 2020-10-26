using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CF.Customer.Domain.Models;
using CF.Customer.Domain.Repositories;
using CF.Customer.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace CF.Customer.Infrastructure.Repositories
{
    public class CustomerRepository : RepositoryBase<Domain.Entities.Customer>, ICustomerRepository
    {
        public CustomerRepository(CustomerContext context) : base(context)
        {
        }

        public async Task<int> CountByFilterAsync(CustomerFilter filter)
        {
            var query = DbContext.Customers.AsQueryable();

            query = ApplyFilter(filter, query);

            return await query.CountAsync();
        }

        public async Task<Domain.Entities.Customer> GetByFilterAsync(CustomerFilter filter)
        {
            var query = DbContext.Customers.AsQueryable();

            query = ApplyFilter(filter, query);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<Domain.Entities.Customer>> GetListByFilterAsync(CustomerFilter filter)
        {
            var query = DbContext.Customers.AsQueryable();

            query = ApplyFilter(filter, query);

            query = ApplySorting(filter, query);

            if (filter.CurrentPage > 0)
                query = query.Skip((filter.CurrentPage - 1) * filter.PageSize).Take(filter.PageSize);

            return await query.ToListAsync();
        }

        private static IQueryable<Domain.Entities.Customer> ApplySorting(CustomerFilter filter,
            IQueryable<Domain.Entities.Customer> query)
        {
            query = filter?.OrderBy.ToLower() switch
            {
                "firstname" => filter.SortBy.ToLower() == "asc"
                    ? query.OrderBy(x => x.FirstName)
                    : query.OrderByDescending(x => x.FirstName),
                "surname" => filter.SortBy.ToLower() == "asc"
                    ? query.OrderBy(x => x.Surname)
                    : query.OrderByDescending(x => x.Surname),
                "email" => filter.SortBy.ToLower() == "asc"
                    ? query.OrderBy(x => x.Email)
                    : query.OrderByDescending(x => x.Email),
                _ => query
            };

            return query;
        }

        private static IQueryable<Domain.Entities.Customer> ApplyFilter(CustomerFilter filter,
            IQueryable<Domain.Entities.Customer> query)
        {
            if (filter.Id > 0)
                query = query.Where(x => x.Id == filter.Id);

            if (!string.IsNullOrWhiteSpace(filter.FirstName))
                query = query.Where(x => EF.Functions.Like(x.FirstName, $"%{filter.FirstName}%"));

            if (!string.IsNullOrWhiteSpace(filter.Surname))
                query = query.Where(x => EF.Functions.Like(x.Surname, $"%{filter.Surname}%"));

            if (!string.IsNullOrWhiteSpace(filter.Email))
                query = query.Where(x => x.Email == filter.Email);

            return query;
        }
    }
}