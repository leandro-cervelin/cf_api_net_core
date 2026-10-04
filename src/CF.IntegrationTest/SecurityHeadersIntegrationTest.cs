using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CF.IntegrationTest.Factories;
using Xunit;

namespace CF.IntegrationTest;

public class SecurityHeadersIntegrationTest(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task ApiResponse_HasSecurityHeaders_AndIsNotCacheable()
    {
        using var client = factory.CreateAdminClient();

        var response = await client.GetAsync("/api/v1/customer", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertSecurityHeaders(response);
        Assert.True(response.Headers.CacheControl?.NoStore, "Customer data must be marked no-store");
    }

    [Fact]
    public async Task ErrorResponse_AlsoHasSecurityHeaders()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/customer", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        AssertSecurityHeaders(response);
    }

    [Fact]
    public async Task HealthProbe_HasSecurityHeaders()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health/live", TestContext.Current.CancellationToken);

        AssertSecurityHeaders(response);
    }

    private static void AssertSecurityHeaders(HttpResponseMessage response)
    {
        Assert.Equal("nosniff", Header(response, "X-Content-Type-Options"));
        Assert.Equal("DENY", Header(response, "X-Frame-Options"));
        Assert.Equal("no-referrer", Header(response, "Referrer-Policy"));
        Assert.Equal("default-src 'none'; frame-ancestors 'none'", Header(response, "Content-Security-Policy"));
    }

    private static string Header(HttpResponseMessage response, string name)
    {
        return response.Headers.TryGetValues(name, out var values) ||
               response.Content.Headers.TryGetValues(name, out values)
            ? values.Single()
            : null;
    }
}
