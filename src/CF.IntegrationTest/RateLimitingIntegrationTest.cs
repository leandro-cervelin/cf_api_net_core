using CF.IntegrationTest.Factories;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace CF.IntegrationTest;

[Collection("RateLimiting")]
public class RateLimitingIntegrationTest : IAsyncLifetime
{
    // Any rate-limited endpoint works; anonymous calls are rejected with 401 but still count towards the limit.
    private const string LimitedUrl = "/api/v1/customer/1";

    private readonly CustomWebApplicationFactory factory = new();

    public ValueTask InitializeAsync() => factory.InitializeAsync();

    public async ValueTask DisposeAsync()
    {
        await factory.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task RateLimiting_ExceedsLimit_Returns429()
    {
        using var client = factory.CreateClient();
        const int requestCount = 62;

        var allowedCount = 0;
        var rateLimitCount = 0;

        for (var i = 0; i < requestCount; i++)
        {
            using var response = await client.GetAsync(LimitedUrl, TestContext.Current.CancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                allowedCount++;
                continue;
            }

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                rateLimitCount++;

                var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
                Assert.Contains("Rate limit exceeded", content);
                Assert.Contains("retryAfter", content);
            }
        }

        Assert.True(allowedCount > 0, "Some requests should have been let through");
        Assert.True(rateLimitCount > 0, "Rate limiting should have triggered at least once");
    }

    [Fact]
    public async Task RateLimiting_WithinLimit_NoneThrottled()
    {
        using var client = factory.CreateClient();

        for (var i = 0; i < 10; i++)
        {
            using var response = await client.GetAsync(LimitedUrl, TestContext.Current.CancellationToken);
            Assert.NotEqual(HttpStatusCode.TooManyRequests, response.StatusCode);
        }
    }

    [Fact]
    public async Task RateLimiting_HealthProbes_AreNeverThrottled()
    {
        using var client = factory.CreateClient();

        var exhausted = false;
        for (var i = 0; i < 62 && !exhausted; i++)
        {
            using var response = await client.GetAsync(LimitedUrl, TestContext.Current.CancellationToken);
            exhausted = response.StatusCode == HttpStatusCode.TooManyRequests;
        }

        Assert.True(exhausted, "The shared rate limit should be exhausted first");

        using var live = await client.GetAsync("/health/live", TestContext.Current.CancellationToken);
        using var ready = await client.GetAsync("/health/ready", TestContext.Current.CancellationToken);
        using var login = await client.PostAsync("/api/v1/auth/token", null, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, live.StatusCode);
        Assert.Equal(HttpStatusCode.OK, ready.StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, login.StatusCode);
    }
}

/// <summary>Trusts X-Forwarded-For only from 10.0.0.5, like an app behind a single known reverse proxy.</summary>
public class TrustedProxyWebApplicationFactory : CustomWebApplicationFactory
{
    public const string ProxyIp = "10.0.0.5";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.UseSetting("ForwardedHeaders:KnownProxies:0", ProxyIp);
    }
}

[Collection("RateLimiting")]
public class ForwardedHeadersRateLimitingIntegrationTest(TrustedProxyWebApplicationFactory factory)
    : IClassFixture<TrustedProxyWebApplicationFactory>
{
    private const string LimitedUrl = "/api/v1/customer/1";

    [Fact]
    public async Task BehindTrustedProxy_EachClientIpGetsItsOwnLimit()
    {
        Assert.True(await ExhaustAsync(TrustedProxyWebApplicationFactory.ProxyIp, _ => "198.51.100.1"),
            "The first client should hit its limit");

        // Same proxy, different client: a separate bucket, so not throttled.
        var otherClient = await SendAsync(TrustedProxyWebApplicationFactory.ProxyIp, "198.51.100.2");
        Assert.Equal(HttpStatusCode.Unauthorized, otherClient);
    }

    [Fact]
    public async Task FromUntrustedSource_SpoofedForwardedForIsIgnored()
    {
        // A different made-up client IP on every request must not dodge the limit of the real connection.
        Assert.True(await ExhaustAsync("203.0.113.9", i => $"192.0.2.{i % 250 + 1}"),
            "Spoofed X-Forwarded-For values should not create new buckets");
    }

    private async Task<bool> ExhaustAsync(string remoteIp, Func<int, string> forwardedFor)
    {
        for (var i = 0; i < 62; i++)
            if (await SendAsync(remoteIp, forwardedFor(i)) == HttpStatusCode.TooManyRequests)
                return true;

        return false;
    }

    private async Task<HttpStatusCode> SendAsync(string remoteIp, string forwardedFor)
    {
        var context = await factory.Server.SendAsync(c =>
        {
            c.Request.Method = "GET";
            c.Request.Path = LimitedUrl;
            c.Connection.RemoteIpAddress = IPAddress.Parse(remoteIp);
            c.Request.Headers["X-Forwarded-For"] = forwardedFor;
        }, TestContext.Current.CancellationToken);

        return (HttpStatusCode)context.Response.StatusCode;
    }
}
