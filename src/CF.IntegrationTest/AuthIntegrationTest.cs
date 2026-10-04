using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using CF.Api.Dtos;
using CF.Customer.Application.Dtos;
using CF.IntegrationTest.Factories;
using CF.IntegrationTest.Models;
using CF.IntegrationTest.Seeds;
using Xunit;

namespace CF.IntegrationTest;

public class AuthIntegrationTest(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private const string CustomerUrl = "api/v1/customer";
    private const string TokenUrl = "api/v1/auth/token";
    private const string Password = "Password1@";

    [Theory]
    [InlineData("api/v1/customer")]
    [InlineData("api/v1/customer/1")]
    public async Task Anonymous_ProtectedEndpoint_ReturnsUnauthorized(string url)
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync(url, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Anonymous_PutAndDelete_ReturnUnauthorized()
    {
        using var client = factory.CreateClient();

        var put = await client.PutAsJsonAsync($"{CustomerUrl}/1", CreateCustomerRequestDto(),
            TestContext.Current.CancellationToken);
        var delete = await client.DeleteAsync($"{CustomerUrl}/1", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, put.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, delete.StatusCode);
    }

    [Fact]
    public async Task Token_InvalidSignature_ReturnsUnauthorized()
    {
        using var client = factory.CreateClientWithToken("eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiIxIiwicm9sZSI6ImFkbWluIn0.invalid");

        var response = await client.GetAsync(CustomerUrl, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsUnauthorized()
    {
        using var client = factory.CreateClient();
        var (_, email) = await SignUpAsync(client);

        var response = await client.PostAsJsonAsync(TokenUrl,
            new LoginRequestDto { Email = email, Password = "Wrong@Password1" }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_UnknownEmail_ReturnsUnauthorized()
    {
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(TokenUrl,
            new LoginRequestDto { Email = "nobody@test.com", Password = Password },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Customer_CanManageOwnRecord_ButNotOthersOrTheList()
    {
        using var anonymous = factory.CreateClient();
        var (id, email) = await SignUpAsync(anonymous);
        using var client = factory.CreateClientWithToken(await LoginAsync(anonymous, email, Password));

        // Own record.
        var getOwn = await client.GetAsync($"{CustomerUrl}/{id}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, getOwn.StatusCode);

        var update = CreateCustomerRequestDto();
        update.Email = email;
        update.FirstName = "Renamed";
        var putOwn = await client.PutAsJsonAsync($"{CustomerUrl}/{id}", update, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, putOwn.StatusCode);

        // Someone else's record (the seed customer) and the full list.
        var otherId = CustomWebApplicationFactory.AdminCustomerId;
        var getOther = await client.GetAsync($"{CustomerUrl}/{otherId}", TestContext.Current.CancellationToken);
        var putOther = await client.PutAsJsonAsync($"{CustomerUrl}/{otherId}", update,
            TestContext.Current.CancellationToken);
        var deleteOther = await client.DeleteAsync($"{CustomerUrl}/{otherId}", TestContext.Current.CancellationToken);
        var list = await client.GetAsync(CustomerUrl, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, getOther.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, putOther.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, deleteOther.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, list.StatusCode);

        // Own delete.
        var deleteOwn = await client.DeleteAsync($"{CustomerUrl}/{id}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, deleteOwn.StatusCode);
    }

    [Fact]
    public async Task ConfiguredAdmin_LoginGrantsListAccess()
    {
        using var anonymous = factory.CreateClient();
        using var admin = factory.CreateClientWithToken(
            await LoginAsync(anonymous, CustomerSeed.Email, CustomerSeed.Password));

        var list = await admin.GetAsync(CustomerUrl, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
    }

    [Fact]
    public async Task PasswordChange_RevokesExistingTokens()
    {
        using var anonymous = factory.CreateClient();
        var (id, email) = await SignUpAsync(anonymous);
        using var client = factory.CreateClientWithToken(await LoginAsync(anonymous, email, Password));

        var update = CreateCustomerRequestDto();
        update.Email = email;
        update.Password = "NewPassword2@";
        update.ConfirmPassword = "NewPassword2@";
        update.CurrentPassword = Password;
        var put = await client.PutAsJsonAsync($"{CustomerUrl}/{id}", update, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, put.StatusCode);

        // The token used for the change is now revoked...
        var withOldToken = await client.GetAsync($"{CustomerUrl}/{id}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, withOldToken.StatusCode);

        // ...and a fresh login with the new password works.
        using var relogged = factory.CreateClientWithToken(await LoginAsync(anonymous, email, "NewPassword2@"));
        var withNewToken = await relogged.GetAsync($"{CustomerUrl}/{id}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, withNewToken.StatusCode);
    }

    [Theory]
    [InlineData(null, "The current password is required to change the password.")]
    [InlineData("Wrong@Password1", "The current password is incorrect.")]
    public async Task PasswordChange_WithoutValidCurrentPassword_IsRejected(string currentPassword,
        string expectedError)
    {
        using var anonymous = factory.CreateClient();
        var (id, email) = await SignUpAsync(anonymous);
        using var client = factory.CreateClientWithToken(await LoginAsync(anonymous, email, Password));

        // A stolen token alone must not be enough to take over the account.
        var update = CreateCustomerRequestDto();
        update.Email = email;
        update.Password = "Hijacked2@";
        update.ConfirmPassword = "Hijacked2@";
        update.CurrentPassword = currentPassword;
        var put = await client.PutAsJsonAsync($"{CustomerUrl}/{id}", update, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, put.StatusCode);
        var problem = await put.Content.ReadFromJsonAsync<ErrorResponse>(TestContext.Current.CancellationToken);
        Assert.Equal(expectedError, Assert.Single(problem!.Errors["Validation"]));

        // The original password still works.
        await LoginAsync(anonymous, email, Password);
    }

    [Fact]
    public async Task EmailChange_WithUnchangedPassword_DoesNotNeedCurrentPassword()
    {
        using var anonymous = factory.CreateClient();
        var (id, email) = await SignUpAsync(anonymous);
        using var client = factory.CreateClientWithToken(await LoginAsync(anonymous, email, Password));

        var update = CreateCustomerRequestDto(); // same Password as at sign-up, so it proves knowledge
        var put = await client.PutAsJsonAsync($"{CustomerUrl}/{id}", update, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, put.StatusCode);

        await LoginAsync(anonymous, update.Email, Password);
    }

    [Fact]
    public async Task NameOnlyChange_KeepsExistingTokensValid()
    {
        using var anonymous = factory.CreateClient();
        var (id, email) = await SignUpAsync(anonymous);
        using var client = factory.CreateClientWithToken(await LoginAsync(anonymous, email, Password));

        var update = CreateCustomerRequestDto();
        update.Email = email;
        update.FirstName = "Renamed";
        var put = await client.PutAsJsonAsync($"{CustomerUrl}/{id}", update, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, put.StatusCode);

        var get = await client.GetAsync($"{CustomerUrl}/{id}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
    }

    [Fact]
    public async Task DeletedCustomer_TokenIsRevoked()
    {
        using var anonymous = factory.CreateClient();
        var (id, email) = await SignUpAsync(anonymous);
        using var client = factory.CreateClientWithToken(await LoginAsync(anonymous, email, Password));

        // Warm the stamp cache, then have an admin delete the account.
        Assert.Equal(HttpStatusCode.OK,
            (await client.GetAsync($"{CustomerUrl}/{id}", TestContext.Current.CancellationToken)).StatusCode);
        using var admin = factory.CreateAdminClient();
        var delete = await admin.DeleteAsync($"{CustomerUrl}/{id}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var afterDelete = await client.GetAsync($"{CustomerUrl}/{id}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, afterDelete.StatusCode);
    }

    [Fact]
    public async Task TokenWithoutSecurityStamp_IsRejected()
    {
        // Simulates a token issued before security stamps existed.
        using var client = factory.CreateClientFor(CustomWebApplicationFactory.AdminCustomerId, CustomerSeed.Email,
            securityStamp: "", ["admin"]);

        var response = await client.GetAsync(CustomerUrl, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private static async Task<(long Id, string Email)> SignUpAsync(HttpClient client)
    {
        var dto = CreateCustomerRequestDto();
        var response = await client.PostAsJsonAsync(CustomerUrl, dto, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        return (body.GetProperty("id").GetInt64(), dto.Email);
    }

    private static async Task<string> LoginAsync(HttpClient client, string email, string password)
    {
        var response = await client.PostAsJsonAsync(TokenUrl, new LoginRequestDto { Email = email, Password = password },
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var token = await response.Content.ReadFromJsonAsync<TokenResponseDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(token);
        return token.AccessToken;
    }

    private static CustomerRequestDto CreateCustomerRequestDto()
    {
        return new CustomerRequestDto
        {
            FirstName = "Auth Test",
            Surname = "Surname",
            Email = $"auth_{Guid.NewGuid():N}@test.com",
            Password = Password,
            ConfirmPassword = Password
        };
    }
}
