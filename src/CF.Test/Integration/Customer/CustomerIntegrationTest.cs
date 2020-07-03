using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CF.Api;
using CF.CustomerMngt.Application.Dtos;
using CF.Test.Integration.Factories;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using Xunit;

namespace CF.Test.Integration.Customer
{
    public class CustomerIntegrationTest : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly CustomWebApplicationFactory<Startup> _factory;

        public CustomerIntegrationTest(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        private const string CustomerUrl = "api/v1/customer";

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

            var content = new StringContent(JsonConvert.SerializeObject(dto));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var client = _factory.CreateClient();
            var response = await client.PostAsync(CustomerUrl, content);
            response.EnsureSuccessStatusCode();

            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task CreateCustomerInvalidEmailTest()
        {
            var dto = new CustomerRequestDto
            {
                FirstName = "Test Name",
                Surname = "Test Surname",
                Email = CreateInvalidEmail(),
                Password = "Password1@"
            };

            var content = new StringContent(JsonConvert.SerializeObject(dto));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var client = _factory.CreateClient();
            var response = await client.PostAsync(CustomerUrl, content);

            Assert.False(response.IsSuccessStatusCode);
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

            var content = new StringContent(JsonConvert.SerializeObject(dto));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var client = _factory.CreateClient();
            var response = await client.PostAsync(CustomerUrl, content);
            Assert.True(response.IsSuccessStatusCode);

            var clientNotOk = _factory.CreateClient();
            var responseNotOk = await clientNotOk.PostAsync(CustomerUrl, content);
            Assert.False(responseNotOk.IsSuccessStatusCode);
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

            var content = new StringContent(JsonConvert.SerializeObject(dto));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var client = _factory.CreateClient();
            var response = await client.PostAsync(CustomerUrl, content);

            Assert.False(response.IsSuccessStatusCode);
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

            var content = new StringContent(JsonConvert.SerializeObject(dto));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var client = _factory.CreateClient();
            var response = await client.PostAsync(CustomerUrl, content);

            Assert.False(response.IsSuccessStatusCode);
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

            var content = new StringContent(JsonConvert.SerializeObject(dto));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var client = _factory.CreateClient();
            var response = await client.PostAsync(CustomerUrl, content);

            Assert.False(response.IsSuccessStatusCode);
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

            var content = new StringContent(JsonConvert.SerializeObject(dto));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var client = _factory.CreateClient();
            var response = await client.PostAsync(CustomerUrl, content);

            Assert.False(response.IsSuccessStatusCode);
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

            var content = new StringContent(JsonConvert.SerializeObject(dto));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var client = _factory.CreateClient();
            var response = await client.PostAsync(CustomerUrl, content);

            Assert.False(response.IsSuccessStatusCode);
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

            var content = new StringContent(JsonConvert.SerializeObject(dto));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var client = _factory.CreateClient();
            var response = await client.PostAsync(CustomerUrl, content);

            Assert.False(response.IsSuccessStatusCode);
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

            var content = new StringContent(JsonConvert.SerializeObject(dto));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var client = _factory.CreateClient();
            var response = await client.PostAsync(CustomerUrl, content);

            Assert.False(response.IsSuccessStatusCode);
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

            var content = new StringContent(JsonConvert.SerializeObject(dto));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var client = _factory.CreateClient();
            var response = await client.PostAsync(CustomerUrl, content);

            Assert.False(response.IsSuccessStatusCode);
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

            var content = new StringContent(JsonConvert.SerializeObject(dto));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var client = _factory.CreateClient();
            var response = await client.PostAsync(CustomerUrl, content);

            Assert.False(response.IsSuccessStatusCode);
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

            var content = new StringContent(JsonConvert.SerializeObject(dto));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var client = _factory.CreateClient();
            var response = await client.PostAsync(CustomerUrl, content);

            Assert.False(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task CreateMaxLengthPasswordTest()
        {
            var dto = new CustomerRequestDto
            {
                FirstName = "Test First",
                Surname = "Test Surname",
                Email = CreateValidEmail(),
                Password = "01234567901234567901234@Df",
                ConfirmPassword = "01234567901234567901234@Df"
            };

            var content = new StringContent(JsonConvert.SerializeObject(dto));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var client = _factory.CreateClient();
            var response = await client.PostAsync(CustomerUrl, content);

            Assert.False(response.IsSuccessStatusCode);
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

            var content = new StringContent(JsonConvert.SerializeObject(dto));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var client = _factory.CreateClient();
            var response = await client.PostAsync(CustomerUrl, content);

            Assert.False(response.IsSuccessStatusCode);
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

            var content = new StringContent(JsonConvert.SerializeObject(dto));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var client = _factory.CreateClient();
            var createResponse = await client.PostAsync(CustomerUrl, content);
            Assert.True(createResponse.IsSuccessStatusCode);

            client = _factory.CreateClient();
            var getResponse = await client.GetAsync(createResponse.Headers.Location.ToString());
            var customer = JsonConvert.DeserializeObject<CustomerResponseDto>(await getResponse.Content.ReadAsStringAsync());

            dto.FirstName = "New Name";
            var contentUpdate = new StringContent(JsonConvert.SerializeObject(dto));
            contentUpdate.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var putResponse = await client.PutAsync($"{CustomerUrl}/{customer.Id}", contentUpdate);
            Assert.True(putResponse.IsSuccessStatusCode);
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

            var contentCustomerOne = new StringContent(JsonConvert.SerializeObject(dto));
            contentCustomerOne.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var client = _factory.CreateClient();
            var createCustomerOneResponse = await client.PostAsync(CustomerUrl, contentCustomerOne);
            Assert.True(createCustomerOneResponse.IsSuccessStatusCode);

            dto.Email = CreateValidEmail();

            var contentCustomerTwo = new StringContent(JsonConvert.SerializeObject(dto));
            contentCustomerTwo.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            client = _factory.CreateClient();
            var createCustomerTwoResponse = await client.PostAsync(CustomerUrl, contentCustomerTwo);
            Assert.True(createCustomerTwoResponse.IsSuccessStatusCode);

            var parameters = new Dictionary<string, string> { { "email", dto.Email } };
            var requestUri = QueryHelpers.AddQueryString(CustomerUrl, parameters);
            client = _factory.CreateClient();
            var getResponse = await client.GetAsync(requestUri);
            Assert.True(getResponse.IsSuccessStatusCode);
            var customer = JsonConvert.DeserializeObject<CustomerResponseDto>(await getResponse.Content.ReadAsStringAsync());

            dto.Email = customerOneEmail;
            var content = new StringContent(JsonConvert.SerializeObject(dto));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            client = _factory.CreateClient();
            var response = await client.PutAsync($"{CustomerUrl}/{customer.Id}", content);
            Assert.False(response.IsSuccessStatusCode);
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
            
            var content = new StringContent(JsonConvert.SerializeObject(dto));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var client = _factory.CreateClient();
            var response = await client.PostAsync(CustomerUrl, content);
            Assert.True(response.IsSuccessStatusCode);

            client = _factory.CreateClient();
            var getResponse = await client.GetAsync(response.Headers.Location.ToString());
            Assert.True(getResponse.IsSuccessStatusCode);
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
                ConfirmPassword = "Password1@",
            };

            var content = new StringContent(JsonConvert.SerializeObject(dto));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var client = _factory.CreateClient();
            var response = await client.PostAsync(CustomerUrl, content);
            Assert.True(response.IsSuccessStatusCode);

            dto.Email = CreateValidEmail();
            var contentTwo = new StringContent(JsonConvert.SerializeObject(dto));
            contentTwo.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            client = _factory.CreateClient();
            var responseTwo = await client.PostAsync(CustomerUrl, contentTwo);
            Assert.True(responseTwo.IsSuccessStatusCode);

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
            Assert.True(getResponse.IsSuccessStatusCode);
            var customers = JsonConvert.DeserializeObject<PaginationDto<CustomerResponseDto>>(await getResponse.Content.ReadAsStringAsync());
            Assert.True(customers.Count > 1);
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

            var content = new StringContent(JsonConvert.SerializeObject(dto));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var client = _factory.CreateClient();
            var response = await client.PostAsync(CustomerUrl, content);
            Assert.True(response.IsSuccessStatusCode);

            client = _factory.CreateClient();
            var getResponse = await client.GetAsync(response.Headers.Location.ToString());
            Assert.True(getResponse.IsSuccessStatusCode);
            var customer = JsonConvert.DeserializeObject<CustomerResponseDto>(await getResponse.Content.ReadAsStringAsync());

            client = _factory.CreateClient();
            var deleteResponse = await client.DeleteAsync($"{CustomerUrl}/{customer.Id}");
            Assert.True(deleteResponse.IsSuccessStatusCode);
        }

        private static string CreateValidEmail()
        {
            return $"{DateTime.Now:yyyyMMdd_hhmmssfff}@test.com";
        }

        private static string CreateInvalidEmail()
        {
            return $"{DateTime.Now:yyyyMMdd_hhmmssfff}attest.com";
        }
    }
}