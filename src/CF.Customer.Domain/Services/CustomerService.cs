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

        if (!CustomerFilter.SortableFields.Contains(filter.OrderBy, StringComparer.OrdinalIgnoreCase))
            throw new ValidationException(
                $"OrderBy must be one of: {string.Join(", ", CustomerFilter.SortableFields)}.");

        if (!CustomerFilter.SortDirections.Contains(filter.SortBy, StringComparer.OrdinalIgnoreCase))
            throw new ValidationException($"SortBy must be one of: {string.Join(", ", CustomerFilter.SortDirections)}.");

        if (filter.PageSize <= 0) filter.PageSize = 10;

        if (filter.CurrentPage <= 0) filter.CurrentPage = 1;

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

    public async Task<Entities.Customer?> GetByFilterAsync(CustomerFilter filter, CancellationToken cancellationToken)
    {
        if (filter is null)
            throw new ValidationException("Filter is null.");

        return await customerRepository.GetByFilterAsync(filter, cancellationToken);
    }

    public async Task UpdateAsync(long id, Entities.Customer customer, string? currentPassword,
        bool verifyCurrentPassword, CancellationToken cancellationToken)
    {
        if (id <= 0) throw new ValidationException("Id is invalid.");

        if (customer is null)
            throw new ValidationException("Customer is null.");

        var entity = await customerRepository.GetByIdAsync(id, cancellationToken) ??
                     throw new EntityNotFoundException(id);

        Validate(customer);

        // Case-insensitive to match the database collation: a case-only change of the customer's own
        // email would otherwise find their own row and be rejected as unavailable.
        var emailChanged = !string.Equals(entity.Email, customer.Email, StringComparison.OrdinalIgnoreCase);
        if (emailChanged && !await IsAvailableEmailAsync(customer.Email, cancellationToken))
            throw new ValidationException("Email is not available.");

        entity.Email = customer.Email;
        entity.FirstName = customer.FirstName;
        entity.Surname = customer.Surname;

        var passwordChanged = !passwordHasherService.Verify(customer.Password, entity.Password);
        if (passwordChanged)
        {
            // An update that keeps the password has already proven the caller knows it (it was sent and matched).
            // Changing it is the only way to edit without knowing it, so that is where proof is required. This also
            // covers email changes: without the current password, a token holder can't keep the password unchanged.
            if (verifyCurrentPassword) VerifyCurrentPassword(currentPassword, entity.Password);

            entity.Password = passwordHasherService.Hash(customer.Password);
        }

        // Credentials changed: revoke every token issued so far.
        if (emailChanged || passwordChanged)
            entity.RotateSecurityStamp();

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

        customer.Password = passwordHasherService.Hash(customer.Password);
        customer.RotateSecurityStamp();
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

    public async Task<Entities.Customer?> AuthenticateAsync(string email, string password,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password)) return null;

        var customer = await customerRepository.GetByFilterAsync(new CustomerFilter { Email = email },
            cancellationToken);

        if (customer is null)
        {
            // Spend the same bcrypt work as a real check so response time doesn't reveal whether the email exists.
            passwordHasherService.Hash(password);
            return null;
        }

        return passwordHasherService.Verify(password, customer.Password) ? customer : null;
    }

    public async Task<string?> GetSecurityStampAsync(long id, CancellationToken cancellationToken)
    {
        if (id <= 0) return null;

        return await customerRepository.GetSecurityStampAsync(id, cancellationToken);
    }

    public async Task<bool> IsAvailableEmailAsync(string email, CancellationToken cancellationToken)
    {
        var filter = new CustomerFilter { Email = email };
        var existingCustomer = await customerRepository.GetByFilterAsync(filter, cancellationToken);
        return existingCustomer is null;
    }

    private void VerifyCurrentPassword(string? currentPassword, string passwordHash)
    {
        if (string.IsNullOrEmpty(currentPassword))
            throw new ValidationException("The current password is required to change the password.");

        if (!passwordHasherService.Verify(currentPassword, passwordHash))
            throw new ValidationException("The current password is incorrect.");
    }

    private static void Validate(Entities.Customer customer)
    {
        customer.ValidateFirstName();

        customer.ValidateSurname();

        customer.ValidateEmail();

        customer.ValidatePassword();
    }
}