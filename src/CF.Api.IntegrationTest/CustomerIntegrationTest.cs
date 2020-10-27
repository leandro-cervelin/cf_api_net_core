﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CF.Api.IntegrationTest.Factories;
using CF.Customer.Application.Dtos;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using Xunit;

namespace CF.Api.IntegrationTest
{
    public class CustomerIntegrationTest : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private const string CustomerUrl = "api/v1/customer";
        private readonly CustomWebApplicationFactory<Startup> _factory;

        public CustomerIntegrationTest(CustomWebApplicationFactory<Startup> factory)
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

            Assert.True(response.StatusCode == HttpStatusCode.Created);
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

            var content = await CreateStringContent(dto);
            var client = _factory.CreateClient();
            var response = await client.PostAsync(CustomerUrl, content);

            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
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
            Assert.True(response.StatusCode == HttpStatusCode.Created);

            var clientNotOk = _factory.CreateClient();
            var responseNotOk = await clientNotOk.PostAsync(CustomerUrl, content);
            Assert.True(responseNotOk.StatusCode == HttpStatusCode.BadRequest);
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

            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
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

            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
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

            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
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

            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
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

            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
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

            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
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

            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
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

            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
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

            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
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

            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
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

            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
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
            Assert.True(createResponse.StatusCode == HttpStatusCode.Created);

            client = _factory.CreateClient();
            var getResponse = await client.GetAsync(createResponse.Headers.Location.ToString());
            Assert.True(getResponse.StatusCode == HttpStatusCode.OK);
            var customer =
                JsonConvert.DeserializeObject<CustomerResponseDto>(await getResponse.Content.ReadAsStringAsync());

            dto.FirstName = "New Name";
            var contentUpdate = await CreateStringContent(dto);
            var putResponse = await client.PutAsync($"{CustomerUrl}/{customer.Id}", contentUpdate);
            Assert.True(putResponse.IsSuccessStatusCode);
            Assert.True(putResponse.StatusCode == HttpStatusCode.NoContent);
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
            Assert.True(createCustomerOneResponse.StatusCode == HttpStatusCode.Created);

            dto.Email = CreateValidEmail();

            var contentCustomerTwo = await CreateStringContent(dto);
            client = _factory.CreateClient();
            var createCustomerTwoResponse = await client.PostAsync(CustomerUrl, contentCustomerTwo);
            Assert.True(createCustomerTwoResponse.StatusCode == HttpStatusCode.Created);

            var parameters = new Dictionary<string, string> {{"email", dto.Email}};
            var requestUri = QueryHelpers.AddQueryString(CustomerUrl, parameters);
            client = _factory.CreateClient();
            var getResponse = await client.GetAsync(requestUri);
            Assert.True(getResponse.StatusCode == HttpStatusCode.OK);
            var customer =
                JsonConvert.DeserializeObject<CustomerResponseDto>(await getResponse.Content.ReadAsStringAsync());

            dto.Email = customerOneEmail;
            var content = await CreateStringContent(dto);
            client = _factory.CreateClient();
            var response = await client.PutAsync($"{CustomerUrl}/{customer.Id}", content);
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
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
            Assert.True(response.StatusCode == HttpStatusCode.Created);

            client = _factory.CreateClient();
            var getResponse = await client.GetAsync(response.Headers.Location.ToString());
            Assert.True(getResponse.StatusCode == HttpStatusCode.OK);
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
            Assert.True(response.StatusCode == HttpStatusCode.Created);

            dto.Email = CreateValidEmail();
            var contentTwo = await CreateStringContent(dto);
            client = _factory.CreateClient();
            var responseTwo = await client.PostAsync(CustomerUrl, contentTwo);
            Assert.True(responseTwo.StatusCode == HttpStatusCode.Created);


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
            Assert.True(getResponse.StatusCode == HttpStatusCode.OK);
            var customers =
                JsonConvert.DeserializeObject<PaginationDto<CustomerResponseDto>>(
                    await getResponse.Content.ReadAsStringAsync());
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

            var content = await CreateStringContent(dto);

            var client = _factory.CreateClient();
            var response = await client.PostAsync(CustomerUrl, content);
            Assert.True(response.StatusCode == HttpStatusCode.Created);

            client = _factory.CreateClient();
            var getResponse = await client.GetAsync(response.Headers.Location.ToString());
            Assert.True(getResponse.StatusCode == HttpStatusCode.OK);
            var customer =
                JsonConvert.DeserializeObject<CustomerResponseDto>(await getResponse.Content.ReadAsStringAsync());

            client = _factory.CreateClient();
            var deleteResponse = await client.DeleteAsync($"{CustomerUrl}/{customer.Id}");
            Assert.True(deleteResponse.StatusCode == HttpStatusCode.NoContent);
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
    }
}