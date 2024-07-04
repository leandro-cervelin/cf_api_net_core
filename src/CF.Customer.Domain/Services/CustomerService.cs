using CF.Customer.Domain.Entities;
using CF.Customer.Domain.Exceptions;
using CF.Customer.Domain.Models;
using CF.Customer.Domain.Repositories;
using CF.Customer.Domain.Services.Interfaces;

namespace CF.Customer.Domain.Services;

public class CustomerService(ICustomerRepository customerRepository, IPasswordHasherService passwordHasherService)
    : ICustomerService
{
    public async Task<Pagination<Entities.Customer>> GetListByFilterAsync(CustomerFilter filter,
        CancellationToken cancellationToken)
    {
        if (filter is null)
            throw new ValidationException("Filter is null.");

        if (filter.PageSize > 100)
            throw new ValidationException("Maximum allowed page size is 100.");

        if (filter.CurrentPage <= 0) filter.PageSize = 1;

        var total = await customerRepository.CountByFilterAsync(filter, cancellationToken);

        if (total == 0) return new Pagination<Entities.Customer>();

        var paginateResult = await customerRepository.GetListByFilterAsync(filter, cancellationToken);

        var result = new Pagination<Entities.Customer>
        {
            Count = total,
            CurrentPage = filter.CurrentPage,
            PageSize = filter.PageSize,
            Result = [.. paginateResult]
        };

        return result;
    }

    public async Task<Entities.Customer> GetByFilterAsync(CustomerFilter filter, CancellationToken cancellationToken)
    {
        if (filter is null)
            throw new ValidationException("Filter is null.");

        return await customerRepository.GetByFilterAsync(filter, cancellationToken);
    }

    public async Task UpdateAsync(long id, Entities.Customer customer, CancellationToken cancellationToken)
    {
        if (id <= 0) throw new ValidationException("Id is invalid.");

        if (customer is null)
            throw new ValidationException("Customer is null.");

        var entity = await customerRepository.GetByIdAsync(id, cancellationToken) ??
                     throw new EntityNotFoundException(id);

        Validate(customer);

        if (entity.Email != customer.Email && !await IsAvailableEmailAsync(customer.Email, cancellationToken))
            throw new ValidationException("Email is not available.");

        entity.Email = customer.Email;
        entity.FirstName = customer.FirstName;
        entity.Surname = customer.Surname;

        if (!await passwordHasherService.VerifyAsync(customer.Password, entity.Password))
            entity.Password = await passwordHasherService.HashAsync(customer.Password);

        entity.SetUpdatedDate();
        await customerRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<long> CreateAsync(Entities.Customer customer, CancellationToken cancellationToken)
    {
        if (customer is null)
            throw new ValidationException("Customer is null.");

        Validate(customer);

        var isAvailableEmail = await IsAvailableEmailAsync(customer.Email, cancellationToken);
        if (!isAvailableEmail) throw new ValidationException("Email is not available.");

        customer.Password = await passwordHasherService.HashAsync(customer.Password);
        customer.SetCreatedDate();
        customerRepository.Add(customer);
        await customerRepository.SaveChangesAsync(cancellationToken);

        return customer.Id;
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken)
    {
        if (id <= 0) throw new ValidationException("Id is invalid.");

        var entity = await customerRepository.GetByIdAsync(id, cancellationToken) ??
                     throw new EntityNotFoundException(id);
        customerRepository.Remove(entity);
        await customerRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> IsAvailableEmailAsync(string email, CancellationToken cancellationToken)
    {
        var filter = new CustomerFilter { Email = email };
        var existingEmail = await customerRepository.GetByFilterAsync(filter, cancellationToken);
        return existingEmail is null;
    }

    private static void Validate(Entities.Customer customer)
    {
        customer.ValidateFirstName();

        customer.ValidateSurname();

        customer.ValidateEmail();

        customer.ValidatePassword();
    }
}