using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using CF.IntegrationTest.Factories;
using Microsoft.AspNetCore.Hosting;
using Xunit;

namespace CF.IntegrationTest;

/// <summary>The OpenAPI document (and Scalar) are only served in Development.</summary>
public class DevelopmentWebApplicationFactory : CustomWebApplicationFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.UseEnvironment("Development");
    }
}

public class OpenApiIntegrationTest(DevelopmentWebApplicationFactory factory)
    : IClassFixture<DevelopmentWebApplicationFactory>
{
    [Fact]
    public async Task Document_DeclaresBearerScheme_AndMarksOnlyProtectedOperations()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/openapi/v1.json", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(
            TestContext.Current.CancellationToken));
        var root = document.RootElement;

        var scheme = root.GetProperty("components").GetProperty("securitySchemes").GetProperty("Bearer");
        Assert.Equal("http", scheme.GetProperty("type").GetString());
        Assert.Equal("bearer", scheme.GetProperty("scheme").GetString());

        var paths = root.GetProperty("paths");
        Assert.True(RequiresBearer(paths.GetProperty("/api/v1/customer").GetProperty("get")));
        Assert.True(RequiresBearer(paths.GetProperty("/api/v1/customer/{id}").GetProperty("put")));
        Assert.False(RequiresBearer(paths.GetProperty("/api/v1/customer").GetProperty("post")));
        Assert.False(RequiresBearer(paths.GetProperty("/api/v1/auth/token").GetProperty("post")));
    }

    private static bool RequiresBearer(JsonElement operation)
    {
        if (!operation.TryGetProperty("security", out var security)) return false;

        foreach (var requirement in security.EnumerateArray())
            if (requirement.TryGetProperty("Bearer", out _))
                return true;

        return false;
    }
}
