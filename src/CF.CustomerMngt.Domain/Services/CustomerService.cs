using System.Linq;
using System.Threading.Tasks;
using CF.CustomerMngt.Domain.Entities;
using CF.CustomerMngt.Domain.Exceptions;
using CF.CustomerMngt.Domain.Helpers.PasswordHasher;
using CF.CustomerMngt.Domain.Models;
using CF.CustomerMngt.Domain.Repositories;
using CF.CustomerMngt.Domain.Services.Interfaces;

namespace CF.CustomerMngt.Domain.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IPasswordHasher _passwordHasher;

        public CustomerService(ICustomerRepository customerRepository, IPasswordHasher passwordHasher)
        {
            _customerRepository = customerRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<Pagination<Customer>> GetListByFilterAsync(CustomerFilter filter)
        {
            if (filter == null)
                throw new ValidationException("Filter is null.");

            if (filter.PageSize > 100)
                throw new ValidationException("Maximum allowed page size is 100.");

            if (filter.CurrentPage <= 0) filter.PageSize = 1;

            var total = await _customerRepository.CountByFilterAsync(filter);

            if (total == 0) return new Pagination<Customer>();

            var paginateResult = await _customerRepository.GetListByFilterAsync(filter);

            var result = new Pagination<Customer>
            {
                Count = total,
                CurrentPage = filter.CurrentPage,
                PageSize = filter.PageSize,
                Result = paginateResult.ToList()
            };

            return result;
        }

        public async Task<Customer> GetByFilterAsync(CustomerFilter filter)
        {
            if (filter == null)
                throw new ValidationException("Filter is null.");

            return await _customerRepository.GetByFilterAsync(filter);
        }

        public async Task UpdateAsync(long id, Customer customer)
        {
            if (id <= 0) throw new ValidationException("Id is invalid.");

            if (customer == null)
                throw new ValidationException("Customer is null.");

            var entity = await _customerRepository.GetByIdAsync(id);

            if (entity == null)
                throw new EntityNotFoundException(id);

            Validate(customer);

            if (entity.Email != customer.Email && !await IsAvailableEmailAsync(customer.Email))
                throw new ValidationException("Email is not available.");

            entity.Email = customer.Email;
            entity.FirstName = customer.FirstName;
            entity.Surname = customer.Surname;

            var (verified, _) = _passwordHasher.Check(entity.Password, customer.Password);
            if (!verified) entity.Password = _passwordHasher.Hash(customer.Password);

            entity.SetUpdatedDate();
            await _customerRepository.SaveChangesAsync();
        }

        public async Task<long> CreateAsync(Customer customer)
        {
            if (customer == null)
                throw new ValidationException("Customer is null.");

            Validate(customer);

            var isAvailableEmail = await IsAvailableEmailAsync(customer.Email);
            if (!isAvailableEmail) throw new ValidationException("Email is not available.");

            customer.Password = _passwordHasher.Hash(customer.Password);
            customer.SetCreatedDate();
            _customerRepository.Add(customer);
            await _customerRepository.SaveChangesAsync();

            return customer.Id;
        }

        public async Task DeleteAsync(long id)
        {
            if (id <= 0) throw new ValidationException("Id is invalid.");

            var entity = await _customerRepository.GetByIdAsync(id);

            if (entity == null) throw new EntityNotFoundException(id);

            _customerRepository.Remove(entity);

            await _customerRepository.SaveChangesAsync();
        }

        public async Task<bool> IsAvailableEmailAsync(string email)
        {
            var existingEmail = await _customerRepository.GetByFilterAsync(new CustomerFilter {Email = email});
            return existingEmail == null;
        }

        private static void Validate(Customer customer)
        {
            customer.ValidateFistName();

            customer.ValidateSurname();

            customer.ValidateEmail();

            customer.ValidatePassword();
        }
    }
}