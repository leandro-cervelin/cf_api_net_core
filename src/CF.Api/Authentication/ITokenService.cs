namespace CF.Api.Authentication;

public interface ITokenService
{
    AccessToken CreateToken(long customerId, string email, string securityStamp, IEnumerable<string> roles);
}

public sealed record AccessToken(string Token, DateTime ExpiresAt);
