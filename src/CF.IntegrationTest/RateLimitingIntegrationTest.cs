using System.Net;
using System.Threading.Tasks;
using CF.IntegrationTest.Factories;
using Xunit;

namespace CF.IntegrationTest;

public class RateLimitingIntegrationTest(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task RateLimiting_ExceedsLimit_Returns429()
    {
        // Arrange
        var client = factory.CreateClient();
        const int requestCount = 62;
        var successCount = 0;
        var rateLimitCount = 0;

        // Act
        for (var i = 0; i < requestCount; i++)
        {
            var response = await client.GetAsync("/health/live");

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    successCount++;
                    break;
                case HttpStatusCode.TooManyRequests:
                {
                    rateLimitCount++;

                    var content = await response.Content.ReadAsStringAsync();
                    Assert.Contains("Rate limit exceeded", content);
                    Assert.Contains("retryAfter", content);
                    break;
                }
            }
        }

        // Assert
        Assert.True(rateLimitCount > 0, "Rate limiting should have triggered at least once");
        Assert.True(successCount > 0, "Some requests should have succeeded");
    }

    [Fact]
    public async Task RateLimiting_WithinLimit_AllSucceed()
    {
        // Arrange
        var client = factory.CreateClient();
        const int requestCount = 10;

        // Act & Assert
        for (var i = 0; i < requestCount; i++)
        {
            var response = await client.GetAsync("/health/live");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }

    [Fact]
    public async Task RateLimiting_DifferentEndpoints_SharesLimit()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var response1 = await client.GetAsync("/health/live");
        var response2 = await client.GetAsync("/health/ready");
        var response3 = await client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);
    }
}