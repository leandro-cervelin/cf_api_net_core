using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CF.Api.Authentication;
using CF.Customer.Application.Facades.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Moq;
using Xunit;

namespace CF.Api.UnitTest;

public class SecurityStampValidatorTest
{
    private readonly MemoryCache _cache = new(new MemoryCacheOptions());
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly Mock<ICustomerFacade> _customerFacade = new();

    [Fact]
    public async Task IsCurrentAsync_MatchingStamp_ReturnsTrue()
    {
        SetupStamp(1, "current");

        Assert.True(await CreateValidator().IsCurrentAsync(CreatePrincipal("1", "current"),
            _cancellationTokenSource.Token));
    }

    [Fact]
    public async Task IsCurrentAsync_StaleStamp_ReturnsFalse()
    {
        SetupStamp(1, "rotated");

        Assert.False(await CreateValidator().IsCurrentAsync(CreatePrincipal("1", "current"),
            _cancellationTokenSource.Token));
    }

    [Fact]
    public async Task IsCurrentAsync_DeletedCustomer_ReturnsFalse()
    {
        SetupStamp(1, null);

        Assert.False(await CreateValidator().IsCurrentAsync(CreatePrincipal("1", "current"),
            _cancellationTokenSource.Token));
    }

    [Theory]
    [InlineData("1", null)] // token issued before stamps existed
    [InlineData("not-a-number", "current")]
    [InlineData(null, "current")]
    public async Task IsCurrentAsync_MissingOrMalformedClaims_ReturnsFalseWithoutLookup(string? sub, string? stamp)
    {
        Assert.False(await CreateValidator().IsCurrentAsync(CreatePrincipal(sub, stamp),
            _cancellationTokenSource.Token));
        _customerFacade.Verify(x => x.GetSecurityStampAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task IsCurrentAsync_CachesStampBetweenCalls()
    {
        SetupStamp(1, "current");
        var principal = CreatePrincipal("1", "current");

        await CreateValidator().IsCurrentAsync(principal, _cancellationTokenSource.Token);
        await CreateValidator().IsCurrentAsync(principal, _cancellationTokenSource.Token);

        _customerFacade.Verify(x => x.GetSecurityStampAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Invalidate_ForcesFreshLookup_SoRotationTakesEffectImmediately()
    {
        SetupStamp(1, "current");
        var principal = CreatePrincipal("1", "current");
        var validator = CreateValidator();
        Assert.True(await validator.IsCurrentAsync(principal, _cancellationTokenSource.Token));

        SetupStamp(1, "rotated");
        validator.Invalidate(1);

        Assert.False(await validator.IsCurrentAsync(principal, _cancellationTokenSource.Token));
    }

    private void SetupStamp(long customerId, string? stamp)
    {
        _customerFacade.Setup(x => x.GetSecurityStampAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stamp);
    }

    private SecurityStampValidator CreateValidator()
    {
        return new SecurityStampValidator(_customerFacade.Object, _cache,
            Options.Create(new JwtOptions { SecurityStampCacheSeconds = 30 }));
    }

    private static ClaimsPrincipal CreatePrincipal(string? sub, string? stamp)
    {
        var claims = new List<Claim>();
        if (sub is not null) claims.Add(new Claim(JwtRegisteredClaimNames.Sub, sub));
        if (stamp is not null) claims.Add(new Claim(AuthenticationExtensions.SecurityStampClaimType, stamp));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
    }
}
