using CF.IntegrationTest.Factories;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace CF.IntegrationTest;

[Collection("RateLimiting")]
public class RateLimitingIntegrationTest
{
    private readonly CustomWebApplicationFactory factory;

    public RateLimitingIntegrationTest()
    {
        factory = new CustomWebApplicationFactory();
    }

    [Fact]
    public async Task RateLimiting_ExceedsLimit_Returns429()
    {
        using var client = factory.CreateClient();
        const int requestCount = 62;

        var successCount = 0;
        var rateLimitCount = 0;

        for (var i = 0; i < requestCount; i++)
        {
            using var response = await client.GetAsync("/health/live", TestContext.Current.CancellationToken);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                successCount++;
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

        Assert.True(successCount > 0, "Some requests should have succeeded");
        Assert.True(rateLimitCount > 0, "Rate limiting should have triggered at least once");
    }

    [Fact]
    public async Task RateLimiting_WithinLimit_AllSucceed()
    {
        using var client = factory.CreateClient();
        const int requestCount = 10;

        for (var i = 0; i < requestCount; i++)
        {
            using var response = await client.GetAsync("/health/live", TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }

    [Fact]
    public async Task RateLimiting_DifferentEndpoints_SharesLimit()
    {
        using var client = factory.CreateClient();

        var exhausted = false;

        for (var i = 0; i < 62; i++)
        {
            using var response = await client.GetAsync("/health/live", TestContext.Current.CancellationToken);

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                exhausted = true;
                break;
            }
        }

        Assert.True(exhausted, "The shared rate limit should be exhausted first");

        using var response1 = await client.GetAsync("/health/ready", TestContext.Current.CancellationToken);
        using var response2 = await client.GetAsync("/health", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.TooManyRequests, response1.StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, response2.StatusCode);
    }
}
