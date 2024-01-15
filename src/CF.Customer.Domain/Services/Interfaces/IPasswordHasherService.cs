namespace CF.Customer.Domain.Services.Interfaces;

public interface IPasswordHasherService
{
    Task<string> HashAsync(string password);

    Task<bool> VerifyAsync(string password, string hash);
}