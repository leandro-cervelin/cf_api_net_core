using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CF.CustomerMngt.Application.Dtos;
using Newtonsoft.Json;
using Xunit;

namespace CF.Test.Integration.Customer
{
    public class CustomerIntegrationTest : BaseIntegrationTest
    {
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

            var httpRequestMessage = BuildPostHttpRequest(CustomerUrl, dto);
            var response = await Client.SendAsync(httpRequestMessage);

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

            var httpRequestMessage = BuildPostHttpRequest(CustomerUrl, dto);
            var response = await Client.SendAsync(httpRequestMessage);

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

            var httpRequestMessage = BuildPostHttpRequest(CustomerUrl, dto);
            var response = await Client.SendAsync(httpRequestMessage);

            Assert.True(response.IsSuccessStatusCode);

            var errorHttpRequestMessage = BuildPostHttpRequest(CustomerUrl, dto);
            var errorResponse = await Client.SendAsync(errorHttpRequestMessage);

            Assert.False(errorResponse.IsSuccessStatusCode);
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

            var httpRequestMessage = BuildPostHttpRequest(CustomerUrl, dto);
            var response = await Client.SendAsync(httpRequestMessage);

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

            var httpRequestMessage = BuildPostHttpRequest(CustomerUrl, dto);
            var response = await Client.SendAsync(httpRequestMessage);

            Assert.False(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task CreateCustomerRequiredSurnameNameTest()
        {
            var dto = new CustomerRequestDto
            {
                FirstName = "Test First Name",
                Surname = "",
                Email = CreateValidEmail(),
                Password = "Password1@",
                ConfirmPassword = "Password1@"
            };

            var httpRequestMessage = BuildPostHttpRequest(CustomerUrl, dto);
            var response = await Client.SendAsync(httpRequestMessage);

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

            var httpRequestMessage = BuildPostHttpRequest(CustomerUrl, dto);
            var response = await Client.SendAsync(httpRequestMessage);

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

            var httpRequestMessage = BuildPostHttpRequest(CustomerUrl, dto);
            var response = await Client.SendAsync(httpRequestMessage);

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

            var httpRequestMessage = BuildPostHttpRequest(CustomerUrl, dto);
            var response = await Client.SendAsync(httpRequestMessage);

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

            var httpRequestMessage = BuildPostHttpRequest(CustomerUrl, dto);
            var response = await Client.SendAsync(httpRequestMessage);

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

            var httpRequestMessage = BuildPostHttpRequest(CustomerUrl, dto);
            var response = await Client.SendAsync(httpRequestMessage);

            Assert.False(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task CreatePasswordsDontMatchTest()
        {
            var dto = new CustomerRequestDto
            {
                FirstName = "Test First",
                Surname = "Test Surname",
                Email = CreateValidEmail(),
                Password = "Password1@",
                ConfirmPassword = "Password3!"
            };

            var httpRequestMessage = BuildPostHttpRequest(CustomerUrl, dto);
            var response = await Client.SendAsync(httpRequestMessage);

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

            var httpRequestMessage = BuildPostHttpRequest(CustomerUrl, dto);
            var response = await Client.SendAsync(httpRequestMessage);

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

            var httpRequestMessage = BuildPostHttpRequest(CustomerUrl, dto);
            var response = await Client.SendAsync(httpRequestMessage);

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

            var httpRequestMessage = BuildPostHttpRequest(CustomerUrl, dto);
            var response = await Client.SendAsync(httpRequestMessage);

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

            var createHttpRequestMessage = BuildPostHttpRequest(CustomerUrl, dto);
            var createResponse = await Client.SendAsync(createHttpRequestMessage);
            Assert.True(createResponse.IsSuccessStatusCode);

            var getHttpRequestMessage = BuildGetHttpRequest(createResponse.Headers.Location.ToString(), null);
            var getResponse = await Client.SendAsync(getHttpRequestMessage);
            var customer = JsonConvert.DeserializeObject<CustomerResponseDto>(await getResponse.Content.ReadAsStringAsync());

            dto.FirstName = "New Name";
            var putHttpRequestMessage = BuildPutHttpRequest($"{CustomerUrl}/{customer.Id}", dto);
            var putResponse = await Client.SendAsync(putHttpRequestMessage);
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

            var createCustomerOneHttpRequestMessage = BuildPostHttpRequest(CustomerUrl, dto);
            var createCustomerOneResponse = await Client.SendAsync(createCustomerOneHttpRequestMessage);
            Assert.True(createCustomerOneResponse.IsSuccessStatusCode);

            dto.Email = CreateValidEmail();
            var createCustomerTwoHttpRequestMessage = BuildPostHttpRequest(CustomerUrl, dto);
            var createCustomerTwoResponse = await Client.SendAsync(createCustomerTwoHttpRequestMessage);
            Assert.True(createCustomerTwoResponse.IsSuccessStatusCode);

            var parameters = new Dictionary<string, string> {{"email", dto.Email}};
            var getHttpRequestMessage = BuildGetHttpRequest(CustomerUrl, parameters);
            var getResponse = await Client.SendAsync(getHttpRequestMessage);
            Assert.True(getResponse.IsSuccessStatusCode);
            var customer = JsonConvert.DeserializeObject<CustomerResponseDto>(await getResponse.Content.ReadAsStringAsync());

            dto.Email = customerOneEmail;
            var putHttpRequestMessage = BuildPutHttpRequest($"{CustomerUrl}/{customer.Id}", dto);
            var putResponse = await Client.SendAsync(putHttpRequestMessage);
            Assert.False(putResponse.IsSuccessStatusCode);
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

            var createCustomerOneHttpRequestMessage = BuildPostHttpRequest(CustomerUrl, dto);
            var createCustomerOneResponse = await Client.SendAsync(createCustomerOneHttpRequestMessage);
            Assert.True(createCustomerOneResponse.IsSuccessStatusCode);

            var getHttpRequestMessage = BuildGetHttpRequest(createCustomerOneResponse.Headers.Location.ToString(), null);
            var getResponse = await Client.SendAsync(getHttpRequestMessage);
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

            var createCustomerOneHttpRequestMessage = BuildPostHttpRequest(CustomerUrl, dto);
            var createCustomerOneResponse = await Client.SendAsync(createCustomerOneHttpRequestMessage);
            Assert.True(createCustomerOneResponse.IsSuccessStatusCode);

            dto.Email = CreateValidEmail();
            var createCustomerTwoHttpRequestMessage = BuildPostHttpRequest(CustomerUrl, dto);
            var createCustomerTwoResponse = await Client.SendAsync(createCustomerTwoHttpRequestMessage);
            Assert.True(createCustomerTwoResponse.IsSuccessStatusCode);

            var parameters = new Dictionary<string, string>
            {
                {"currentPage", "1"},
                {"pageSize", "1"},
                {"orderBy", dto.FirstName},
                {"sortBy", "asc"}
            };

            var getHttpRequestMessage = BuildGetHttpRequest(CustomerUrl, parameters);
            var getResponse = await Client.SendAsync(getHttpRequestMessage);
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

            var createHttpRequestMessage = BuildPostHttpRequest(CustomerUrl, dto);
            var createResponse = await Client.SendAsync(createHttpRequestMessage);
            Assert.True(createResponse.IsSuccessStatusCode);

            var getHttpRequestMessage = BuildGetHttpRequest(createResponse.Headers.Location.ToString(), null);
            var getResponse = await Client.SendAsync(getHttpRequestMessage);
            var customer = JsonConvert.DeserializeObject<CustomerResponseDto>(await getResponse.Content.ReadAsStringAsync());

            var deleteHttpRequestMessage = BuildDeleteHttpRequest($"{CustomerUrl}/{customer.Id}");
            var deleteResponse = await Client.SendAsync(deleteHttpRequestMessage);
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