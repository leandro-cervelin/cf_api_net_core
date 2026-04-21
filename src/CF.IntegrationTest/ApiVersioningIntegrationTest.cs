using CF.Customer.Application.Dtos;
using CF.IntegrationTest.Factories;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;

namespace CF.IntegrationTest;

public class ApiVersioningIntegrationTest : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory factory;

    public ApiVersioningIntegrationTest(CustomWebApplicationFactory factory)
        => this.factory = factory;

    [Fact]
    public async Task ApiVersioning_V1Endpoint_ReturnsSuccess()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/customer?pageSize=10", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ApiVersioning_ResponseHeaders_ContainVersionInfo()
    {
        using var client = factory.CreateClient();

        var url = $"/api/v1/customer?pageSize=10&_t={Guid.NewGuid()}";
        var response = await client.GetAsync(url, TestContext.Current.CancellationToken);

        Assert.True(
            response.Headers.TryGetValues("api-supported-versions", out var values),
            "Response should contain api-supported-versions header");

        Assert.NotEmpty(values);
    }


    [Fact]
    public async Task ApiVersioning_PostReturnsCorrectLocation()
    {
        using var client = factory.CreateClient();

        var dto = new CustomerRequestDto
        {
            FirstName = "Test",
            Surname = "Versioning",
            Email = $"versioning_{DateTime.UtcNow:yyyyMMddHHmmssfff}@test.com",
            Password = "Test@1234",
            ConfirmPassword = "Test@1234"
        };

        using var content = new StringContent(JsonConvert.SerializeObject(dto));
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        var response = await client.PostAsync("/api/v1/customer", content, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Contains("/api/v1/customer/", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task ApiVersioning_InvalidVersion_ReturnsNotFound()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v2/customer", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
