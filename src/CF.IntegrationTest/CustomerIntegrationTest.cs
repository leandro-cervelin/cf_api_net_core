using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CF.Customer.Application.Dtos;
using CF.IntegrationTest.Factories;
using CF.IntegrationTest.Models;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Xunit;

namespace CF.IntegrationTest;

public class CustomerIntegrationTest : IClassFixture<CustomWebApplicationFactory>
{
    private const string CustomerUrl = "api/v1/customer";
    private readonly CustomWebApplicationFactory _factory;

    public CustomerIntegrationTest(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateCustomerOkTest()
    {
        var dto = new CustomerRequestDto
        {
            FirstName = "Test Name",
            Surname = "Test Surname",
            Email = CreateValidEmail(),
            Password = "Password1@",
            ConfirmPassword = "Password1@"
        };

        var content = await CreateStringContent(dto);
        var client = _factory.CreateClient();
        var response = await client.PostAsync(CustomerUrl, content);
        response.EnsureSuccessStatusCode();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateCustomerInvalidEmailTest()
    {
        var dto = new CustomerRequestDto
        {
            FirstName = "Test Name",
            Surname = "Test Surname",
            Email = CreateInvalidEmail(),
            Password = "Password1@",
            ConfirmPassword = "Password1@"
        };

        var content = await CreateStringContent(dto);
        var client = _factory.CreateClient();
        var response = await client.PostAsync(CustomerUrl, content);
        var errors = await ExtractErrorsFromResponse(response);

        Assert.NotNull(errors);
        Assert.Contains("Email", errors);
        Assert.Single(errors["Email"]);
        Assert.Equal("The Email field is not a valid e-mail address.", errors["Email"][0]);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateCustomerExistingEmailTest()
    {
        var email = CreateValidEmail();

        var dto = new CustomerRequestDto
        {
            FirstName = "Test Name",
            Surname = "Test Surname",
            Email = email,
            Password = "Password1@",
            ConfirmPassword = "Password1@"
        };

        var content = await CreateStringContent(dto);
        var client = _factory.CreateClient();
        var response = await client.PostAsync(CustomerUrl, content);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var clientNotOk = _factory.CreateClient();
        var responseNotOk = await clientNotOk.PostAsync(CustomerUrl, content);
        var errors = await ExtractErrorsFromResponse(responseNotOk);

        Assert.NotNull(errors);
        Assert.Contains("Validation", errors);
        Assert.Single(errors["Validation"]);
        Assert.Equal("Email is not available.", errors["Validation"][0]);
        Assert.Equal(HttpStatusCode.BadRequest, responseNotOk.StatusCode);
    }

    [Fact]
    public async Task CreateCustomerRequiredEmailTest()
    {
        var dto = new CustomerRequestDto
        {
            FirstName = "Test Name",
            Surname = "Test Surname",
            Email = "",
            Password = "Password1@",
            ConfirmPassword = "Password1@"
        };

        var content = await CreateStringContent(dto);
        var client = _factory.CreateClient();
        var response = await client.PostAsync(CustomerUrl, content);
        var errors = await ExtractErrorsFromResponse(response);

        Assert.NotNull(errors);
        Assert.Contains("Email", errors);
        Assert.NotEmpty(errors["Email"]);
        Assert.Equal("The Email field is required.", errors["Email"][0]);
        Assert.Equal("The Email field is not a valid e-mail address.", errors["Email"][1]);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateCustomerRequiredFirstNameTest()
    {
        var dto = new CustomerRequestDto
        {
            FirstName = "",
            Surname = "Test Surname",
            Email = CreateValidEmail(),
            Password = "Password1@",
            ConfirmPassword = "Password1@"
        };

        var content = await CreateStringContent(dto);
        var client = _factory.CreateClient();
        var response = await client.PostAsync(CustomerUrl, content);
        var errors = await ExtractErrorsFromResponse(response);

        Assert.NotNull(errors);
        Assert.Contains("FirstName", errors);
        Assert.NotEmpty(errors["FirstName"]);
        Assert.Equal("The First Name field is required.", errors["FirstName"][0]);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateCustomerRequiredSurnameTest()
    {
        var dto = new CustomerRequestDto
        {
            FirstName = "Test First Name",
            Surname = "",
            Email = CreateValidEmail(),
            Password = "Password1@",
            ConfirmPassword = "Password1@"
        };

        var content = await CreateStringContent(dto);
        var client = _factory.CreateClient();
        var response = await client.PostAsync(CustomerUrl, content);
        var errors = await ExtractErrorsFromResponse(response);

        Assert.NotNull(errors);
        Assert.Contains("Surname", errors);
        Assert.NotEmpty(errors["Surname"]);
        Assert.Equal("The Surname field is required.", errors["Surname"][0]);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateCustomerMaxLengthFirstNameTest()
    {
        var dto = new CustomerRequestDto
        {
            FirstName =
                "Test First Name Test First Name Test First Name Test First Name Test First Name Test First Name Test First Name",
            Surname = "Test Surname",
            Email = CreateValidEmail(),
            Password = "Password1@",
            ConfirmPassword = "Password1@"
        };

        var content = await CreateStringContent(dto);
        var client = _factory.CreateClient();
        var response = await client.PostAsync(CustomerUrl, content);
        var errors = await ExtractErrorsFromResponse(response);

        Assert.NotNull(errors);
        Assert.Contains("FirstName", errors);
        Assert.NotEmpty(errors["FirstName"]);
        Assert.Equal("The field First Name must be a string with a minimum length of 2 and a maximum length of 100.",
            errors["FirstName"][0]);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateCustomerMinLengthFirstNameTest()
    {
        var dto = new CustomerRequestDto
        {
            FirstName = "T",
            Surname = "Test Surname",
            Email = CreateValidEmail(),
            Password = "Password1@",
            ConfirmPassword = "Password1@"
        };

        var content = await CreateStringContent(dto);
        var client = _factory.CreateClient();
        var response = await client.PostAsync(CustomerUrl, content);
        var errors = await ExtractErrorsFromResponse(response);

        Assert.NotNull(errors);
        Assert.Contains("FirstName", errors);
        Assert.NotEmpty(errors["FirstName"]);
        Assert.Equal("The field First Name must be a string with a minimum length of 2 and a maximum length of 100.",
            errors["FirstName"][0]);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateCustomerMaxLengthSurnameTest()
    {
        var dto = new CustomerRequestDto
        {
            FirstName = "Test First",
            Surname =
                "Test Surname Test Surname Test Surname Test Surname Test Surname Test Surname Test Surname Test Surname Test Surname",
            Email = CreateValidEmail(),
            Password = "Password1@",
            ConfirmPassword = "Password1@"
        };

        var content = await CreateStringContent(dto);
        var client = _factory.CreateClient();
        var response = await client.PostAsync(CustomerUrl, content);
        var errors = await ExtractErrorsFromResponse(response);

        Assert.NotNull(errors);
        Assert.Contains("Surname", errors);
        Assert.NotEmpty(errors["Surname"]);
        Assert.Equal("The field Surname must be a string with a minimum length of 2 and a maximum length of 100.",
            errors["Surname"][0]);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateCustomerMinLengthSurnameTest()
    {
        var dto = new CustomerRequestDto
        {
            FirstName = "Test First",
            Surname = "T",
            Email = CreateValidEmail(),
            Password = "Password1@",
            ConfirmPassword = "Password1@"
        };

        var content = await CreateStringContent(dto);
        var client = _factory.CreateClient();
        var response = await client.PostAsync(CustomerUrl, content);
        var errors = await ExtractErrorsFromResponse(response);

        Assert.NotNull(errors);
        Assert.Contains("Surname", errors);
        Assert.NotEmpty(errors["Surname"]);
        Assert.Equal("The field Surname must be a string with a minimum length of 2 and a maximum length of 100.",
            errors["Surname"][0]);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateRequiredPasswordTest()
    {
        var dto = new CustomerRequestDto
        {
            FirstName = "Test First",
            Surname = "Test Surname",
            Email = CreateValidEmail(),
            Password = ""
        };

        var content = await CreateStringContent(dto);
        var client = _factory.CreateClient();
        var response = await client.PostAsync(CustomerUrl, content);
        var errors = await ExtractErrorsFromResponse(response);

        Assert.NotNull(errors);
        Assert.Contains("Password", errors);
        Assert.NotEmpty(errors["Password"]);
        Assert.Equal("The Password field is required.", errors["Password"][0]);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreatePasswordsDoesNotMatchTest()
    {
        var dto = new CustomerRequestDto
        {
            FirstName = "Test First",
            Surname = "Test Surname",
            Email = CreateValidEmail(),
            Password = "Password1@",
            ConfirmPassword = "Password3!"
        };

        var content = await CreateStringContent(dto);
        var client = _factory.CreateClient();
        var response = await client.PostAsync(CustomerUrl, content);

        var errors = await ExtractErrorsFromResponse(response);

        Assert.NotNull(errors);
        Assert.Contains("ConfirmPassword", errors);
        Assert.NotEmpty(errors["ConfirmPassword"]);
        Assert.Equal("The passwords do not match.", errors["ConfirmPassword"][0]);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateMinLengthPasswordTest()
    {
        var dto = new CustomerRequestDto
        {
            FirstName = "Test First",
            Surname = "Test Surname",
            Email = CreateValidEmail(),
            Password = "@123RF",
            ConfirmPassword = "@123RF"
        };

        var content = await CreateStringContent(dto);
        var client = _factory.CreateClient();
        var response = await client.PostAsync(CustomerUrl, content);
        var errors = await ExtractErrorsFromResponse(response);

        Assert.NotNull(errors);
        Assert.Contains("Password", errors);
        Assert.NotEmpty(errors["Password"]);
        Assert.Equal(
            "Passwords must be at least 8 characters and contain at 3 of the following: upper case (A-Z), lower case (a-z), number (0-9) and special character (e.g. !@#$%^&*).",
            errors["Password"][0]);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateInvalidPasswordTest()
    {
        var dto = new CustomerRequestDto
        {
            FirstName = "Test First",
            Surname = "Test Surname",
            Email = CreateValidEmail(),
            Password = "01234567901234",
            ConfirmPassword = "01234567901234"
        };

        var content = await CreateStringContent(dto);
        var client = _factory.CreateClient();
        var response = await client.PostAsync(CustomerUrl, content);
        var errors = await ExtractErrorsFromResponse(response);

        Assert.NotNull(errors);
        Assert.Contains("Password", errors);
        Assert.NotEmpty(errors["Password"]);
        Assert.Equal(
            "Passwords must be at least 8 characters and contain at 3 of the following: upper case (A-Z), lower case (a-z), number (0-9) and special character (e.g. !@#$%^&*).",
            errors["Password"][0]);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCustomerOkTest()
    {
        var email = CreateValidEmail();

        var dto = new CustomerRequestDto
        {
            FirstName = "Test Name",
            Surname = "Test Surname",
            Email = email,
            Password = "Password1@",
            ConfirmPassword = "Password1@"
        };

        var content = await CreateStringContent(dto);
        var client = _factory.CreateClient();
        var createResponse = await client.PostAsync(CustomerUrl, content);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        client = _factory.CreateClient();
        var getResponse = await client.GetAsync(createResponse.Headers.Location?.ToString());
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var customer =
            JsonConvert.DeserializeObject<CustomerResponseDto>(await getResponse.Content.ReadAsStringAsync());

        dto.FirstName = "New Name";
        var contentUpdate = await CreateStringContent(dto);
        var putResponse = await client.PutAsync($"{CustomerUrl}/{customer.Id}", contentUpdate);
        Assert.True(putResponse.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.NoContent, putResponse.StatusCode);
    }

    [Fact]
    public async Task UpdateCustomerExistingEmailTest()
    {
        var customerOneEmail = CreateValidEmail();

        var dto = new CustomerRequestDto
        {
            FirstName = "Test Name",
            Surname = "Test Surname",
            Email = customerOneEmail,
            Password = "Password1@",
            ConfirmPassword = "Password1@"
        };

        var contentCustomerOne = await CreateStringContent(dto);
        var client = _factory.CreateClient();
        var createCustomerOneResponse = await client.PostAsync(CustomerUrl, contentCustomerOne);
        Assert.Equal(HttpStatusCode.Created, createCustomerOneResponse.StatusCode);

        dto.Email = CreateValidEmail();

        var contentCustomerTwo = await CreateStringContent(dto);
        client = _factory.CreateClient();
        var createCustomerTwoResponse = await client.PostAsync(CustomerUrl, contentCustomerTwo);
        Assert.Equal(HttpStatusCode.Created, createCustomerTwoResponse.StatusCode);

        var parameters = new Dictionary<string, string> {{"email", dto.Email}};
        var requestUri = QueryHelpers.AddQueryString(CustomerUrl, parameters);
        client = _factory.CreateClient();
        var getResponse = await client.GetAsync(requestUri);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var customer =
            JsonConvert.DeserializeObject<CustomerResponseDto>(await getResponse.Content.ReadAsStringAsync());

        dto.Email = customerOneEmail;
        var content = await CreateStringContent(dto);
        client = _factory.CreateClient();
        var response = await client.PutAsync($"{CustomerUrl}/{customer.Id}", content);

        var errors = await ExtractErrorsFromResponse(response);

        Assert.NotNull(errors);
        Assert.Contains("Id", errors);
        Assert.NotEmpty(errors["Id"]);
        Assert.Equal("Invalid Id.", errors["Id"][0]);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetCustomerTest()
    {
        var customerOneEmail = CreateValidEmail();

        var dto = new CustomerRequestDto
        {
            FirstName = "Test Name",
            Surname = "Test Surname",
            Email = customerOneEmail,
            Password = "Password1@",
            ConfirmPassword = "Password1@"
        };

        var content = await CreateStringContent(dto);
        var client = _factory.CreateClient();
        var response = await client.PostAsync(CustomerUrl, content);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        client = _factory.CreateClient();
        var getResponse = await client.GetAsync(response.Headers.Location?.ToString());
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }

    [Fact]
    public async Task GetCustomerInvalidIdValueTest()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync($"{CustomerUrl}/l");
        var errors = await ExtractErrorsFromResponse(response);

        Assert.NotNull(errors);
        Assert.Contains("id", errors);
        Assert.NotEmpty(errors["id"]);
        Assert.Equal("The value 'l' is not valid.", errors["id"][0]);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetCustomerInvalidIdNegativeTest()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync($"{CustomerUrl}/-1");
        var errors = await ExtractErrorsFromResponse(response);

        Assert.NotNull(errors);
        Assert.Contains("Id", errors);
        Assert.NotEmpty(errors["Id"]);
        Assert.Equal("Invalid Id.", errors["Id"][0]);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetCustomerListTest()
    {
        var dto = new CustomerRequestDto
        {
            FirstName = "Test Name Get",
            Surname = "Test Surname",
            Email = CreateValidEmail(),
            Password = "Password1@",
            ConfirmPassword = "Password1@"
        };

        var content = await CreateStringContent(dto);
        var client = _factory.CreateClient();
        var response = await client.PostAsync(CustomerUrl, content);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        dto.Email = CreateValidEmail();
        var contentTwo = await CreateStringContent(dto);
        client = _factory.CreateClient();
        var responseTwo = await client.PostAsync(CustomerUrl, contentTwo);
        Assert.Equal(HttpStatusCode.Created, responseTwo.StatusCode);


        var parameters = new Dictionary<string, string>
        {
            {"currentPage", "1"},
            {"pageSize", "1"},
            {"orderBy", dto.FirstName},
            {"sortBy", "asc"}
        };

        var requestUri = QueryHelpers.AddQueryString(CustomerUrl, parameters);

        client = _factory.CreateClient();
        var getResponse = await client.GetAsync(requestUri);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var customers =
            JsonConvert.DeserializeObject<PaginationDto<CustomerResponseDto>>(
                await getResponse.Content.ReadAsStringAsync());
        Assert.True(customers.Count > 1);
        Assert.NotEmpty(customers.Result);
    }

    [Fact]
    public async Task DeleteCustomerOkTest()
    {
        var email = CreateValidEmail();

        var dto = new CustomerRequestDto
        {
            FirstName = "Test Name",
            Surname = "Test Surname",
            Email = email,
            Password = "Password1@",
            ConfirmPassword = "Password1@"
        };

        var content = await CreateStringContent(dto);

        var client = _factory.CreateClient();
        var response = await client.PostAsync(CustomerUrl, content);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        client = _factory.CreateClient();
        var getResponse = await client.GetAsync(response.Headers.Location?.ToString());
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var customer =
            JsonConvert.DeserializeObject<CustomerResponseDto>(await getResponse.Content.ReadAsStringAsync());

        client = _factory.CreateClient();
        var deleteResponse = await client.DeleteAsync($"{CustomerUrl}/{customer.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    private static async Task<StringContent> CreateStringContent(CustomerRequestDto dto)
    {
        var content = new StringContent(await Task.Factory.StartNew(() => JsonConvert.SerializeObject(dto)));
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        return content;
    }

    private static string CreateValidEmail()
    {
        return $"{DateTime.Now:yyyyMMdd_hhmmssfff}@test.com";
    }

    private static string CreateInvalidEmail()
    {
        return $"{DateTime.Now:yyyyMMdd_hhmmssfff}attest.com";
    }

    private static async Task<IDictionary<string, string[]>> ExtractErrorsFromResponse(HttpResponseMessage response)
    {
        var responseContent =
            JsonConvert.DeserializeObject<ErrorResponse>(await response.Content.ReadAsStringAsync(),
                new ExpandoObjectConverter());
        var errors =
            JsonConvert.DeserializeObject<Dictionary<string, string[]>>(responseContent.Errors.ToString()) as
                IDictionary<string, string[]>;
        return errors;
    }
}