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

public class CustomerIntegrationTest(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private const string CustomerUrl = "api/v1/customer";
    private readonly HttpClient _httpClient = factory.CreateClient();

    [Fact]
    public async Task CreateCustomerOkTestAsync()
    {
        var dto = CreateCustomerRequestDto();
        var content = await CreateStringContentAsync(dto);
        var response = await _httpClient.PostAsync(CustomerUrl, content);
        response.EnsureSuccessStatusCode();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }


    [Fact]
    public async Task CreateCustomerExistingEmailTestAsync()
    {
        var dto = CreateCustomerRequestDto();
        var content = await CreateStringContentAsync(dto);
        var response = await _httpClient.PostAsync(CustomerUrl, content);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        content = await CreateStringContentAsync(dto);
        var responseNotOk = await _httpClient.PostAsync(CustomerUrl, content);
        var errors = await ExtractErrorsFromResponse(responseNotOk);

        Assert.NotNull(errors);
        Assert.Contains("Validation", errors);
        Assert.Single(errors["Validation"]);
        Assert.Equal("Email is not available.", errors["Validation"][0]);
        Assert.Equal(HttpStatusCode.BadRequest, responseNotOk.StatusCode);
    }

    [Theory]
    [InlineData("invalid_email_at_test.com", "The Email field is not a valid email address.")]
    [InlineData("", "The Email field is required.")]
    [InlineData(
        "eeeeeeeeeewedewfdwefweeemmmmmmmmaaaaaaaaaaaaaaiiiiiiiiiiiilllllllllll@teeeeeeesssssssssssssstttttttttttttttt.com",
        "The Email field must not exceed 100 characters.")]
    public async Task CreateCustomerAsync_Email_Validation_Test(string email, string errorMessage)
    {
        var dto = CreateCustomerRequestDto();
        dto.Email = email;

        var content = await CreateStringContentAsync(dto);
        var response = await _httpClient.PostAsync(CustomerUrl, content);
        var errors = await ExtractErrorsFromResponse(response);

        Assert.NotNull(errors);
        Assert.Contains("Email", errors);
        Assert.NotEmpty(errors["Email"]);
        Assert.Equal(errorMessage, errors["Email"][0]);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("", "The First Name field is required.")]
    [InlineData(
        "Test First Name Test First Name Test First Name Test First Name Test First Name Test First Name Test First Name",
        "The First Name field must be between 2 and 100 characters.")]
    [InlineData("T", "The First Name field must be between 2 and 100 characters.")]
    public async Task CreateCustomerAsync_FirstName_Validation_Test(string firstName, string errorMessage)
    {
        var dto = CreateCustomerRequestDto();
        dto.FirstName = firstName;
        var content = await CreateStringContentAsync(dto);
        var response = await _httpClient.PostAsync(CustomerUrl, content);
        var errors = await ExtractErrorsFromResponse(response);

        Assert.NotNull(errors);
        Assert.Contains("FirstName", errors);
        Assert.NotEmpty(errors["FirstName"]);
        Assert.Equal(errorMessage, errors["FirstName"][0]);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("", "The Surname field is required.")]
    [InlineData(
        "Test Surname Test Surname Test Surname Test Surname Test Surname Test Surname Test Surname Test Surname Test Surname",
        "The Surname field must be between 2 and 100 characters.")]
    [InlineData("T", "The Surname field must be between 2 and 100 characters.")]
    public async Task CreateCustomerAsync_Surname_Validation_Test(string surname, string errorMessage)
    {
        var dto = CreateCustomerRequestDto();
        dto.Surname = surname;
        var content = await CreateStringContentAsync(dto);
        var response = await _httpClient.PostAsync(CustomerUrl, content);
        var errors = await ExtractErrorsFromResponse(response);

        Assert.NotNull(errors);
        Assert.Contains("Surname", errors);
        Assert.NotEmpty(errors["Surname"]);
        Assert.Equal(errorMessage, errors["Surname"][0]);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("", "The Password field is required.")]
    [InlineData("@123RF",
        "Passwords must be at least 8 characters and contain at least 3 of the following: upper case (A-Z), lower case (a-z), number (0-9), and special character (e.g. !@#$%^&*).")]
    [InlineData("01234567901234",
        "Passwords must be at least 8 characters and contain at least 3 of the following: upper case (A-Z), lower case (a-z), number (0-9), and special character (e.g. !@#$%^&*).")]
    public async Task CreateCustomerAsync_Password_Validation_Test(string password, string errorMessage)
    {
        var dto = CreateCustomerRequestDto();
        dto.Password = password;

        var content = await CreateStringContentAsync(dto);
        var response = await _httpClient.PostAsync(CustomerUrl, content);
        var errors = await ExtractErrorsFromResponse(response);

        Assert.NotNull(errors);
        Assert.Contains("Password", errors);
        Assert.NotEmpty(errors["Password"]);
        Assert.Equal(errorMessage, errors["Password"][0]);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateCustomerAsync_ConfirmPassword_Validation_Test()
    {
        var dto = CreateCustomerRequestDto();
        dto.ConfirmPassword = "Password3!";

        var content = await CreateStringContentAsync(dto);
        var response = await _httpClient.PostAsync(CustomerUrl, content);

        var errors = await ExtractErrorsFromResponse(response);

        Assert.NotNull(errors);
        Assert.Contains("ConfirmPassword", errors);
        Assert.NotEmpty(errors["ConfirmPassword"]);
        Assert.Equal("The passwords do not match.", errors["ConfirmPassword"][0]);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCustomerOkTestAsync()
    {
        var dto = CreateCustomerRequestDto();

        var content = await CreateStringContentAsync(dto);
        var createResponse = await _httpClient.PostAsync(CustomerUrl, content);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var getResponse = await _httpClient.GetAsync(createResponse.Headers.Location?.ToString());
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var customer =
            JsonConvert.DeserializeObject<CustomerResponseDto>(await getResponse.Content.ReadAsStringAsync());

        dto.FirstName = "New Name";
        var contentUpdate = await CreateStringContentAsync(dto);
        var putResponse = await _httpClient.PutAsync($"{CustomerUrl}/{customer.Id}", contentUpdate);
        Assert.True(putResponse.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.NoContent, putResponse.StatusCode);
    }

    [Fact]
    public async Task UpdateCustomerIncludingPasswordOkTestAsync()
    {
        var dto = CreateCustomerRequestDto();

        var content = await CreateStringContentAsync(dto);
        var createResponse = await _httpClient.PostAsync(CustomerUrl, content);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var getResponse = await _httpClient.GetAsync(createResponse.Headers.Location?.ToString());
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var customer =
            JsonConvert.DeserializeObject<CustomerResponseDto>(await getResponse.Content.ReadAsStringAsync());

        dto.FirstName = "New Name";
        dto.Password = "NewPassword3@";
        dto.ConfirmPassword = "NewPassword3@";
        var contentUpdate = await CreateStringContentAsync(dto);
        var putResponse = await _httpClient.PutAsync($"{CustomerUrl}/{customer.Id}", contentUpdate);
        Assert.True(putResponse.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.NoContent, putResponse.StatusCode);
    }

    [Fact]
    public async Task UpdateCustomerExistingEmailTestAsync()
    {
        var dto = CreateCustomerRequestDto();
        var customerOneEmail = dto.Email;

        var contentCustomerOne = await CreateStringContentAsync(dto);
        var createCustomerOneResponse = await _httpClient.PostAsync(CustomerUrl, contentCustomerOne);
        Assert.Equal(HttpStatusCode.Created, createCustomerOneResponse.StatusCode);

        dto.Email = $"new_{customerOneEmail}";

        var contentCustomerTwo = await CreateStringContentAsync(dto);
        var createCustomerTwoResponse = await _httpClient.PostAsync(CustomerUrl, contentCustomerTwo);
        Assert.Equal(HttpStatusCode.Created, createCustomerTwoResponse.StatusCode);

        var parameters = new Dictionary<string, string> { { "email", dto.Email } };
        var requestUri = QueryHelpers.AddQueryString(CustomerUrl, parameters);
        var getResponse = await _httpClient.GetAsync(requestUri);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var customer =
            JsonConvert.DeserializeObject<CustomerResponseDto>(await getResponse.Content.ReadAsStringAsync());

        dto.Email = customerOneEmail;
        var content = await CreateStringContentAsync(dto);
        var response = await _httpClient.PutAsync($"{CustomerUrl}/{customer.Id}", content);

        var errors = await ExtractErrorsFromResponse(response);

        Assert.NotNull(errors);
        Assert.Contains("Id", errors);
        Assert.NotEmpty(errors["Id"]);
        Assert.Equal("Invalid Id.", errors["Id"][0]);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetCustomerTestAsync()
    {
        var dto = CreateCustomerRequestDto();

        var content = await CreateStringContentAsync(dto);
        var response = await _httpClient.PostAsync(CustomerUrl, content);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var getResponse = await _httpClient.GetAsync(response.Headers.Location?.ToString());
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }

    [Fact]
    public async Task GetCustomerInvalidIdValueTestAsync()
    {
        var response = await _httpClient.GetAsync($"{CustomerUrl}/l");
        var errors = await ExtractErrorsFromResponse(response);

        Assert.NotNull(errors);
        Assert.Contains("id", errors);
        Assert.NotEmpty(errors["id"]);
        Assert.Equal("The value 'l' is not valid.", errors["id"][0]);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetCustomerInvalidIdNegativeTestAsync()
    {
        var response = await _httpClient.GetAsync($"{CustomerUrl}/-1");
        var errors = await ExtractErrorsFromResponse(response);

        Assert.NotNull(errors);
        Assert.Contains("Id", errors);
        Assert.NotEmpty(errors["Id"]);
        Assert.Equal("Invalid Id.", errors["Id"][0]);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetCustomerListTestAsync()
    {
        var dto = CreateCustomerRequestDto();
        var content = await CreateStringContentAsync(dto);
        var response = await _httpClient.PostAsync(CustomerUrl, content);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        dto.Email = $"new_{dto.Email}";
        var contentTwo = await CreateStringContentAsync(dto);
        var responseTwo = await _httpClient.PostAsync(CustomerUrl, contentTwo);
        Assert.Equal(HttpStatusCode.Created, responseTwo.StatusCode);

        var parameters = new Dictionary<string, string>
        {
            { "currentPage", "1" },
            { "pageSize", "1" },
            { "orderBy", dto.FirstName },
            { "sortBy", "asc" }
        };

        var requestUri = QueryHelpers.AddQueryString(CustomerUrl, parameters);

        var getResponse = await _httpClient.GetAsync(requestUri);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var customers =
            JsonConvert.DeserializeObject<PaginationDto<CustomerResponseDto>>(
                await getResponse.Content.ReadAsStringAsync());
        Assert.True(customers.Count > 1);
        Assert.NotEmpty(customers.Result);
    }

    [Fact]
    public async Task DeleteCustomerOkTestAsync()
    {
        var dto = CreateCustomerRequestDto();

        var content = await CreateStringContentAsync(dto);

        var response = await _httpClient.PostAsync(CustomerUrl, content);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var getResponse = await _httpClient.GetAsync(response.Headers.Location?.ToString());
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var customer =
            JsonConvert.DeserializeObject<CustomerResponseDto>(await getResponse.Content.ReadAsStringAsync());

        var deleteResponse = await _httpClient.DeleteAsync($"{CustomerUrl}/{customer.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    private static async Task<StringContent> CreateStringContentAsync(CustomerRequestDto dto)
    {
        var content = await Task.FromResult(new StringContent(JsonConvert.SerializeObject(dto)));
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        return content;
    }

    private static CustomerRequestDto CreateCustomerRequestDto()
    {
        return new CustomerRequestDto
        {
            FirstName = "Test Name",
            Surname = "Test Surname",
            Email = $"{DateTime.Now:yyyyMMdd_hhmmssfff}@test.com",
            Password = "Password1@",
            ConfirmPassword = "Password1@"
        };
    }

    private static async Task<IDictionary<string, string[]>> ExtractErrorsFromResponse(HttpResponseMessage response)
    {
        var responseContent =
            JsonConvert.DeserializeObject<ErrorResponse>(await response.Content.ReadAsStringAsync(),
                new ExpandoObjectConverter());
        var errors =
            (IDictionary<string, string[]>)JsonConvert.DeserializeObject<Dictionary<string, string[]>>(
                responseContent.Errors.ToString());
        return errors;
    }
}