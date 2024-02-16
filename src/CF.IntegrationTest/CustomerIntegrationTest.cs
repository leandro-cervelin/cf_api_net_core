using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CF.Customer.Application.Dtos;
using CF.IntegrationTest.Factories;
using CF.IntegrationTest.Models;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Xunit;

namespace CF.IntegrationTest
{
    public class CustomerIntegrationTest(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
    {
        private const string CustomerUrl = "api/v1/customer";
        private readonly HttpClient _httpClient = factory.CreateClient();

        [Fact]
        public async Task CreateCustomerOkTestAsync()
        {
            var dto = await CreateCustomerRequestDtoAsync();
            var content = await CreateStringContentAsync(dto);
            var response = await _httpClient.PostAsync(CustomerUrl, content);
            response.EnsureSuccessStatusCode();

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task CreateCustomerInvalidEmailTestAsync()
        {
            var dto = await CreateCustomerRequestDtoAsync();
            dto.Email = "invalid_at_test.com";
            var content = await CreateStringContentAsync(dto);
            var response = await _httpClient.PostAsync(CustomerUrl, content);
            var errors = await ExtractErrorsFromResponse(response);

            Assert.NotNull(errors);
            Assert.Contains("Email", errors);
            Assert.Single(errors["Email"]);
            Assert.Equal("The Email field is not a valid email address.", errors["Email"][0]);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateCustomerExistingEmailTestAsync()
        {
            var dto = await CreateCustomerRequestDtoAsync();
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

        [Fact]
        public async Task CreateCustomerRequiredEmailTestAsync()
        {
            var dto = await CreateCustomerRequestDtoAsync();
            dto.Email = string.Empty;

            var content = await CreateStringContentAsync(dto);
            var response = await _httpClient.PostAsync(CustomerUrl, content);
            var errors = await ExtractErrorsFromResponse(response);

            Assert.NotNull(errors);
            Assert.Contains("Email", errors);
            Assert.NotEmpty(errors["Email"]);
            Assert.Equal("The Email field is required.", errors["Email"][0]);
            Assert.Equal("The Email field is not a valid email address.", errors["Email"][1]);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateCustomerEmailMustNotExceedTestAsync()
        {
            var dto = await CreateCustomerRequestDtoAsync();
            dto.Email = "eeeeeeeeeewedewfdwefweeemmmmmmmmaaaaaaaaaaaaaaiiiiiiiiiiiilllllllllll@teeeeeeesssssssssssssstttttttttttttttt.com";

            var content = await CreateStringContentAsync(dto);
            var response = await _httpClient.PostAsync(CustomerUrl, content);
            var errors = await ExtractErrorsFromResponse(response);

            Assert.NotNull(errors);
            Assert.Contains("Email", errors);
            Assert.NotEmpty(errors["Email"]);
            Assert.Equal("The Email field must not exceed 100 characters.", errors["Email"][0]);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateCustomerRequiredFirstNameTestAsync()
        {
            var dto = await CreateCustomerRequestDtoAsync();
            dto.FirstName = string.Empty;

            var content = await CreateStringContentAsync(dto);
            var response = await _httpClient.PostAsync(CustomerUrl, content);
            var errors = await ExtractErrorsFromResponse(response);

            Assert.NotNull(errors);
            Assert.Contains("FirstName", errors);
            Assert.NotEmpty(errors["FirstName"]);
            Assert.Equal("The First Name field is required.", errors["FirstName"][0]);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateCustomerRequiredSurnameTestAsync()
        {
            var dto = await CreateCustomerRequestDtoAsync();
            dto.Surname = string.Empty;

            var content = await CreateStringContentAsync(dto);
            var response = await _httpClient.PostAsync(CustomerUrl, content);
            var errors = await ExtractErrorsFromResponse(response);

            Assert.NotNull(errors);
            Assert.Contains("Surname", errors);
            Assert.NotEmpty(errors["Surname"]);
            Assert.Equal("The Surname field is required.", errors["Surname"][0]);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateCustomerMaxLengthFirstNameTestAsync()
        {
            var dto = await CreateCustomerRequestDtoAsync();
            dto.FirstName = 
                    "Test First Name Test First Name Test First Name Test First Name Test First Name Test First Name Test First Name";

            var content = await CreateStringContentAsync(dto);
            var response = await _httpClient.PostAsync(CustomerUrl, content);
            var errors = await ExtractErrorsFromResponse(response);

            Assert.NotNull(errors);
            Assert.Contains("FirstName", errors);
            Assert.NotEmpty(errors["FirstName"]);
            Assert.Equal("The First Name field must be between 2 and 100 characters.",
                errors["FirstName"][0]);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateCustomerMinLengthFirstNameTestAsync()
        {
            var dto = await CreateCustomerRequestDtoAsync();
            dto.FirstName = "T";

            var content = await CreateStringContentAsync(dto);
            var response = await _httpClient.PostAsync(CustomerUrl, content);
            var errors = await ExtractErrorsFromResponse(response);

            Assert.NotNull(errors);
            Assert.Contains("FirstName", errors);
            Assert.NotEmpty(errors["FirstName"]);
            Assert.Equal("The First Name field must be between 2 and 100 characters.",
                errors["FirstName"][0]);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateCustomerMaxLengthSurnameTestAsync()
        {
            var dto = await CreateCustomerRequestDtoAsync();
            dto.Surname =
                    "Test Surname Test Surname Test Surname Test Surname Test Surname Test Surname Test Surname Test Surname Test Surname";

            var content = await CreateStringContentAsync(dto);
            var response = await _httpClient.PostAsync(CustomerUrl, content);
            var errors = await ExtractErrorsFromResponse(response);

            Assert.NotNull(errors);
            Assert.Contains("Surname", errors);
            Assert.NotEmpty(errors["Surname"]);
            Assert.Equal("The Surname field must be between 2 and 100 characters.",
                errors["Surname"][0]);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateCustomerMinLengthSurnameTestAsync()
        {
            var dto = await CreateCustomerRequestDtoAsync();
            dto.Surname = "T";

            var content = await CreateStringContentAsync(dto);
            var response = await _httpClient.PostAsync(CustomerUrl, content);
            var errors = await ExtractErrorsFromResponse(response);

            Assert.NotNull(errors);
            Assert.Contains("Surname", errors);
            Assert.NotEmpty(errors["Surname"]);
            Assert.Equal("The Surname field must be between 2 and 100 characters.",
                errors["Surname"][0]);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateRequiredPasswordTestAsync()
        {
            var dto = await CreateCustomerRequestDtoAsync();
            dto.Password = string.Empty;

            var content = await CreateStringContentAsync(dto);
            var response = await _httpClient.PostAsync(CustomerUrl, content);
            var errors = await ExtractErrorsFromResponse(response);

            Assert.NotNull(errors);
            Assert.Contains("Password", errors);
            Assert.NotEmpty(errors["Password"]);
            Assert.Equal("The Password field is required.", errors["Password"][0]);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreatePasswordsDoesNotMatchTestAsync()
        {
            var dto = await CreateCustomerRequestDtoAsync();
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
        public async Task CreateMinLengthPasswordTestAsync()
        {
            var dto = await CreateCustomerRequestDtoAsync();
            dto.Password = "@123RF";
            dto.ConfirmPassword = "@123RF";

            var content = await CreateStringContentAsync(dto);
            var response = await _httpClient.PostAsync(CustomerUrl, content);
            var errors = await ExtractErrorsFromResponse(response);

            Assert.NotNull(errors);
            Assert.Contains("Password", errors);
            Assert.NotEmpty(errors["Password"]);
            Assert.Equal(
                "Passwords must be at least 8 characters and contain at least 3 of the following: upper case (A-Z), lower case (a-z), number (0-9), and special character (e.g. !@#$%^&*).",
                errors["Password"][0]);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateInvalidPasswordTestAsync()
        {
            var dto = await CreateCustomerRequestDtoAsync();
            dto.Password = "01234567901234";
            dto.ConfirmPassword = "01234567901234";

            var content = await CreateStringContentAsync(dto);
            var response = await _httpClient.PostAsync(CustomerUrl, content);
            var errors = await ExtractErrorsFromResponse(response);

            Assert.NotNull(errors);
            Assert.Contains("Password", errors);
            Assert.NotEmpty(errors["Password"]);
            Assert.Equal("Passwords must be at least 8 characters and contain at least 3 of the following: upper case (A-Z), lower case (a-z), number (0-9), and special character (e.g. !@#$%^&*).", errors["Password"][0]);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            }
            [Fact]
        public async Task UpdateCustomerOkTestAsync()
        {
            var dto = await CreateCustomerRequestDtoAsync();

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
            var dto = await CreateCustomerRequestDtoAsync();

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
            var dto = await CreateCustomerRequestDtoAsync();
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
            var dto = await CreateCustomerRequestDtoAsync();

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
            var dto = await CreateCustomerRequestDtoAsync();
            var content = await CreateStringContentAsync(dto);
            var response = await _httpClient.PostAsync(CustomerUrl, content);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            
            dto.Email = $"new_{dto.Email}";
            var contentTwo = await CreateStringContentAsync(dto);
            var responseTwo = await _httpClient.PostAsync(CustomerUrl, contentTwo);
            Assert.Equal(HttpStatusCode.Created, responseTwo.StatusCode);

            var parameters = new Dictionary<string, string>
            {
                {"currentPage", "1"},
                {"pageSize", "1"},
                {"orderBy", dto.FirstName},
                {"sortBy", "asc"}
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
            var dto = await CreateCustomerRequestDtoAsync();

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

        private static async Task<CustomerRequestDto> CreateCustomerRequestDtoAsync()
        {
            return await Task.FromResult(new CustomerRequestDto
            {
                FirstName = "Test Name",
                Surname = "Test Surname",
                Email = $"{DateTime.Now:yyyyMMdd_hhmmssfff}@test.com",
                Password = "Password1@",
                ConfirmPassword = "Password1@"
            });
        }

        private static async Task<IDictionary<string, string[]>> ExtractErrorsFromResponse(HttpResponseMessage response)
        {
            var responseContent =
                JsonConvert.DeserializeObject<ErrorResponse>(await response.Content.ReadAsStringAsync(), new ExpandoObjectConverter());
            var errors =
                (IDictionary<string, string[]>) JsonConvert.DeserializeObject<Dictionary<string, string[]>>(responseContent.Errors.ToString());
            return errors;
        }
    }
}