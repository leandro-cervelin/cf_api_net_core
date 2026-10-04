using System.Globalization;
using System.Security.Claims;
using CF.Customer.Application.Facades.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;

namespace CF.Api.Authentication;

/// <summary>
///     Rejects tokens whose security stamp is stale (password/email changed) or whose customer was deleted.
///     Stamps are cached briefly to keep this off the database for most requests. Changes made through this
///     instance invalidate the cache right away; other instances pick them up when their entry expires.
/// </summary>
public class SecurityStampValidator(
    ICustomerFacade customerFacade,
    IMemoryCache cache,
    IOptions<JwtOptions> options) : ISecurityStampValidator
{
    public async Task<bool> IsCurrentAsync(ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var tokenStamp = principal.FindFirst(AuthenticationExtensions.SecurityStampClaimType)?.Value;
        if (string.IsNullOrEmpty(tokenStamp) ||
            !long.TryParse(principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, NumberStyles.None,
                CultureInfo.InvariantCulture, out var customerId))
            return false;

        var currentStamp = await GetCurrentStampAsync(customerId, cancellationToken);

        return currentStamp is not null && string.Equals(currentStamp, tokenStamp, StringComparison.Ordinal);
    }

    public void Invalidate(long customerId)
    {
        cache.Remove(CacheKey(customerId));
    }

    private async Task<string?> GetCurrentStampAsync(long customerId, CancellationToken cancellationToken)
    {
        var cacheDuration = TimeSpan.FromSeconds(options.Value.SecurityStampCacheSeconds);
        if (cacheDuration <= TimeSpan.Zero)
            return await customerFacade.GetSecurityStampAsync(customerId, cancellationToken);

        // A null (deleted customer) is cached too, so a revoked token can't be used to hammer the database.
        return await cache.GetOrCreateAsync(CacheKey(customerId), entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = cacheDuration;
            return customerFacade.GetSecurityStampAsync(customerId, cancellationToken);
        });
    }

    private static string CacheKey(long customerId)
    {
        return $"security-stamp:{customerId}";
    }
}
