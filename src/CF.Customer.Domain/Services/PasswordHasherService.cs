using CF.Customer.Domain.Services.Interfaces;
using Crypt = BCrypt.Net.BCrypt;

namespace CF.Customer.Domain.Services;

public class PasswordHasherService : IPasswordHasherService
{
    public async Task<string> HashAsync(string password) => await Task.FromResult(Crypt.HashPassword(password));

    public async Task<bool> VerifyAsync(string password, string hash) => await Task.FromResult(Crypt.Verify(password, hash));
}