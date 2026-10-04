using System.Security.Claims;

namespace CF.Api.Authentication;

public interface ISecurityStampValidator
{
    /// <summary>
    ///     True when the token's customer still exists and its security stamp matches the current one.
    /// </summary>
    Task<bool> IsCurrentAsync(ClaimsPrincipal principal, CancellationToken cancellationToken);

    /// <summary>Drops the cached stamp so a change takes effect immediately on this instance.</summary>
    void Invalidate(long customerId);
}
