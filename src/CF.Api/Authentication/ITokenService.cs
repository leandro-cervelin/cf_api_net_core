namespace CF.Api.Authentication;

public interface ITokenService
{
    AccessToken CreateToken(long customerId, string email, IEnumerable<string> roles);
}

public sealed record AccessToken(string Token, DateTime ExpiresAt);
