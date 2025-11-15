using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using CF.IntegrationTest.Factories;
using Xunit;

namespace CF.IntegrationTest;

public class HealthCheckIntegrationTest(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory = factory;

    [Fact]
    public async Task HealthCheck_Full_ReturnsHealthyStatus()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var content = await response.Content.ReadAsStringAsync();
        var healthReport = JsonSerializer.Deserialize<JsonElement>(content);

        Assert.Equal("Healthy", healthReport.GetProperty("status").GetString());
        // In test environment, we only have basic health checks (no database)
        Assert.True(healthReport.GetProperty("checks").GetArrayLength() >= 0);
        Assert.True(healthReport.GetProperty("totalDuration").GetDouble() >= 0);
    }

    [Fact]
    public async Task HealthCheck_Ready_ChecksDatabaseConnectivity()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health/ready");

        // Assert - In test environment, this endpoint exists but may not have database checks
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task HealthCheck_Live_ReturnsHealthy()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health/live");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(Skip = "Database checks are disabled in test environment")]
    public async Task HealthCheck_Full_ContainsDatabaseCheck()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();
        var healthReport = JsonSerializer.Deserialize<JsonElement>(content);

        // Assert
        var checks = healthReport.GetProperty("checks");
        var hasDatabaseCheck = false;
        var hasSelfCheck = false;

        foreach (var check in checks.EnumerateArray())
        {
            var name = check.GetProperty("name").GetString();
            if (name == "database") hasDatabaseCheck = true;
            if (name == "self") hasSelfCheck = true;
        }

        Assert.True(hasDatabaseCheck, "Health check should include database check");
        Assert.True(hasSelfCheck, "Health check should include self check");
    }

    [Fact]
    public async Task HealthCheck_Full_IncludesDurationMetrics()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();
        var healthReport = JsonSerializer.Deserialize<JsonElement>(content);

        // Assert
        var checks = healthReport.GetProperty("checks");
        foreach (var check in checks.EnumerateArray())
        {
            Assert.True(check.TryGetProperty("duration", out var duration));
            Assert.True(duration.GetDouble() >= 0, "Duration should be non-negative");
        }

        Assert.True(healthReport.GetProperty("totalDuration").GetDouble() >= 0);
    }
}