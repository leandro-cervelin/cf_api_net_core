using System.Globalization;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace CF.Api.Authentication;

public class TokenService(IOptions<JwtOptions> options) : ITokenService
{
    private readonly JsonWebTokenHandler _handler = new();

    public AccessToken CreateToken(long customerId, string email, string securityStamp, IEnumerable<string> roles)
    {
        var jwt = options.Value;
        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(jwt.ExpiryMinutes);

        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, customerId.ToString(CultureInfo.InvariantCulture)),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(AuthenticationExtensions.SecurityStampClaimType, securityStamp)
        ];
        claims.AddRange(roles.Select(role => new Claim(AuthenticationExtensions.RoleClaimType, role)));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = jwt.Issuer,
            Audience = jwt.Audience,
            Subject = new ClaimsIdentity(claims),
            IssuedAt = now,
            NotBefore = now,
            Expires = expiresAt,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)), SecurityAlgorithms.HmacSha256)
        };

        return new AccessToken(_handler.CreateToken(descriptor), expiresAt);
    }
}
