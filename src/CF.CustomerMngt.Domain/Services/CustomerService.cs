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

        public async Task<Pagination<Customer>> GetListByFilter(CustomerFilter filter)
        {
            if (filter == null)
                throw new ValidationException("Filter is null.");

            if (filter.PageSize > 100)
                throw new ValidationException( "Maximum allowed page size is 100.");

            if (filter.CurrentPage <= 0) filter.PageSize = 1;

            var total = await _customerRepository.CountByFilter(filter);

            if (total == 0) return new Pagination<Customer>();

            var paginateResult = await _customerRepository.GetListByFilter(filter);

            var result = new Pagination<Customer>
            {
                Count = total,
                CurrentPage = filter.CurrentPage,
                PageSize = filter.PageSize,
                Result = paginateResult.ToList()
            };

            return result;
        }

        public async Task<Customer> GetByFilter(CustomerFilter filter)
        {
            if (filter == null)
                throw new ValidationException("Filter is null.");

            return await _customerRepository.GetByFilter(filter);
        }

        public async Task Update(long id, Customer customer)
        {
            if (id <= 0) throw new ValidationException("Id is invalid.");

            if (customer == null)
                throw new ValidationException("Customer is null.");
            
            var entity = await _customerRepository.GetById(id);

            if (entity == null) 
                throw new EntityNotFoundException(id);

            Validate(customer);

            if (entity.Email != customer.Email && !await IsAvailableEmail(customer.Email))
                throw new ValidationException("Email is not available.");

            entity.Email = customer.Email;
            entity.FirstName = customer.FirstName;
            entity.Surname = customer.Surname;

            var password = _passwordHasher.Check(entity.Password, customer.Password);
            if (!password.Verified) entity.Password = _passwordHasher.Hash(customer.Password);

            entity.SetUpdatedDate();
            await _customerRepository.SaveChanges();
        }

        public async Task<long> Create(Customer customer)
        {
            if (customer == null)
                throw new ValidationException("Customer is null.");

            Validate(customer);

            var isAvailableEmail = await IsAvailableEmail(customer.Email);
            if (!isAvailableEmail) throw new ValidationException("Email is not available.");
            
            customer.Password = _passwordHasher.Hash(customer.Password);
            customer.SetCreatedDate();
            await _customerRepository.Add(customer);
            await _customerRepository.SaveChanges();

            return customer.Id;
        }

        public async Task Delete(long id)
        {
            if (id <= 0) throw new ValidationException("Id is invalid.");

            var entity = await _customerRepository.GetById(id);

            if (entity == null) throw new EntityNotFoundException(id);

            _customerRepository.Remove(entity.Id);

            await _customerRepository.SaveChanges();
        }

        public async Task<bool> IsAvailableEmail(string email)
        {
            var existingEmail = await _customerRepository.GetByFilter(new CustomerFilter {Email = email});
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
