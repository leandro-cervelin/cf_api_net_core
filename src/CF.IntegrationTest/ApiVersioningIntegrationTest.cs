using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CF.Customer.Application.Dtos;
using CF.IntegrationTest.Factories;
using Newtonsoft.Json;
using Xunit;

namespace CF.IntegrationTest;

public class ApiVersioningIntegrationTest(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task ApiVersioning_V1Endpoint_ReturnsSuccess()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/v1/customer?pageSize=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ApiVersioning_ResponseHeaders_ContainVersionInfo()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/v1/customer?pageSize=10");

        // Assert
        Assert.True(response.Headers.Contains("api-supported-versions"),
            "Response should contain api-supported-versions header");
    }

    [Fact]
    public async Task ApiVersioning_PostReturnsCorrectLocation()
    {
        // Arrange
        var client = factory.CreateClient();
        var dto = new CustomerRequestDto
        {
            FirstName = "Test",
            Surname = "Versioning",
            Email = $"versioning_{DateTime.Now:yyyyMMddHHmmss}@test.com",
            Password = "Test@1234",
            ConfirmPassword = "Test@1234"
        };

        var content = new StringContent(JsonConvert.SerializeObject(dto));
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        // Act
        var response = await client.PostAsync("/api/v1/customer", content);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Contains("/api/v1/customer/", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task ApiVersioning_InvalidVersion_ReturnsNotFound()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/v2/customer");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}