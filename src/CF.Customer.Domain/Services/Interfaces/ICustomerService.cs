using CF.Customer.Domain.Models;

namespace CF.Customer.Domain.Services.Interfaces;

public interface ICustomerService
{
    Task<Pagination<Entities.Customer>>
        GetListByFilterAsync(CustomerFilter filter, CancellationToken cancellationToken);

    Task<Entities.Customer?> GetByFilterAsync(CustomerFilter filter, CancellationToken cancellationToken);
    /// <param name="currentPassword">The customer's existing password; required to change it.</param>
    /// <param name="verifyCurrentPassword">False only for admins editing another customer.</param>
    Task UpdateAsync(long id, Entities.Customer customer, string? currentPassword, bool verifyCurrentPassword,
        CancellationToken cancellationToken);
    Task<long> CreateAsync(Entities.Customer customer, CancellationToken cancellationToken);
    Task DeleteAsync(long id, CancellationToken cancellationToken);

    /// <summary>Returns the customer when the email and password match, otherwise null.</summary>
    Task<Entities.Customer?> AuthenticateAsync(string email, string password, CancellationToken cancellationToken);

    /// <summary>Returns the customer's current security stamp, or null when the customer doesn't exist.</summary>
    Task<string?> GetSecurityStampAsync(long id, CancellationToken cancellationToken);
}