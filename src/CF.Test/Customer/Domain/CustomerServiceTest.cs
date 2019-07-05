using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using CF.CustomerMngt.Application.Dtos;
using CF.CustomerMngt.Application.Facades;
using CF.CustomerMngt.Domain.Exceptions;
using CF.CustomerMngt.Domain.Helpers.PasswordHasher;
using CF.CustomerMngt.Domain.Models;
using CF.CustomerMngt.Domain.Repositories;
using CF.CustomerMngt.Domain.Services;
using Moq;
using Xunit;

namespace CF.Test.Customer.Domain
{
    public class CustomerServiceTest
    {
        [Fact]
        public async Task GetTest()
        {
            //Arrange
            var customer = new CustomerMngt.Domain.Entities.Customer
            {
                Id = 1,
                Password = "Password@01",
                Email = "test@test.com",
                Surname = "Surname",
                FirstName = "FirstName",
                Updated = DateTime.Now,
                Created = DateTime.Now
            };

            //Act
            var mockRepository = new Mock<ICustomerRepository>();
            var mockPassword = new Mock<IPasswordHasher>();
            var filter = new CustomerFilter {Id = 1};
            mockRepository.Setup(x => x.GetByFilter(filter)).Returns(Task.FromResult(customer));
            var mockService = new CustomerService(mockRepository.Object, mockPassword.Object);
            var result = await mockService.GetByFilter(filter);

            //Assert
            Assert.Equal(customer.Id, result.Id);
        }

        [Fact]
        public async Task GetListTest()
        {
            //Arrange
            var customerOne = new CustomerMngt.Domain.Entities.Customer
            {
                Id = 1,
                Password = "Password@01",
                Email = "test1@test.com",
                Surname = "Surname",
                FirstName = "FirstName",
                Updated = DateTime.Now,
                Created = DateTime.Now
            };

            var customerTwo = new CustomerMngt.Domain.Entities.Customer
            {
                Id = 2,
                Password = "Password@01",
                Email = "test2@test.com",
                Surname = "Surname",
                FirstName = "FirstName",
                Updated = DateTime.Now,
                Created = DateTime.Now
            };

            var customers = new List<CustomerMngt.Domain.Entities.Customer>
            {
                customerOne,
                customerTwo
            };

            //Act
            var mockRepository = new Mock<ICustomerRepository>();
            var mockPassword = new Mock<IPasswordHasher>();
            var filter = new CustomerFilter {PageSize = 10, CurrentPage = 1};
            mockRepository.Setup(x => x.CountByFilter(filter)).Returns(Task.FromResult(customers.Count));
            mockRepository.Setup(x => x.GetListByFilter(filter)).Returns(Task.FromResult(customers));
            var mockService = new CustomerService(mockRepository.Object, mockPassword.Object);
            var result = await mockService.GetListByFilter(filter);

            //Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task CreateInvalidFirstNameMinLengthTest()
        {
            //Arrange
            var customer = new CustomerMngt.Domain.Entities.Customer
            {
                Password = "Password@01",
                Email = "test1@test.com",
                Surname = "Surname",
                FirstName = "F"
            };

            //Act
            var mockRepository = new Mock<ICustomerRepository>();
            var mockPassword = new Mock<IPasswordHasher>();
            var mockService = new CustomerService(mockRepository.Object, mockPassword.Object);
            var exception = await Assert.ThrowsAsync<ValidationException>(() => mockService.Create(customer));

            //Assert
            Assert.NotNull(exception);
        }

        [Fact]
        public async Task CreateInvalidFirstNameEmptyTest()
        {
            //Arrange
            var customer = new CustomerMngt.Domain.Entities.Customer
            {
                Password = "Password@01",
                Email = "test1@test.com",
                Surname = "Surname",
                FirstName = ""
            };

            //Act
            var mockRepository = new Mock<ICustomerRepository>();
            var mockPassword = new Mock<IPasswordHasher>();
            var mockService = new CustomerService(mockRepository.Object, mockPassword.Object);
            var exception = await Assert.ThrowsAsync<ValidationException>(() => mockService.Create(customer));

            //Assert
            Assert.NotNull(exception);
        }

        [Fact]
        public async Task CreateInvalidFirstNameMaxLengthTest()
        {
            //Arrange
            var customer = new CustomerMngt.Domain.Entities.Customer
            {
                Password = "Password@01",
                Email = "test1@test.com",
                Surname = "Surname",
                FirstName =
                    "First Name First Name First Name First Name First Name First Name First Name First Name First Name First Name First Name."
            };

            //Act
            var mockRepository = new Mock<ICustomerRepository>();
            var mockPassword = new Mock<IPasswordHasher>();
            var mockService = new CustomerService(mockRepository.Object, mockPassword.Object);
            var exception = await Assert.ThrowsAsync<ValidationException>(() => mockService.Create(customer));

            //Assert
            Assert.NotNull(exception);
        }

        [Fact]
        public async Task CreateInvalidSurnameEmptyTest()
        {
            //Arrange
            var customer = new CustomerMngt.Domain.Entities.Customer
            {
                Password = "Password@01",
                Email = "test1@test.com",
                Surname =
                    "Surname Surname Surname Surname Surname Surname Surname Surname Surname Surname Surname Surname Surname Surname Surname Surname",
                FirstName = "First Name"
            };

            //Act
            var mockRepository = new Mock<ICustomerRepository>();
            var mockPassword = new Mock<IPasswordHasher>();
            var mockService = new CustomerService(mockRepository.Object, mockPassword.Object);
            var exception = await Assert.ThrowsAsync<ValidationException>(() => mockService.Create(customer));

            //Assert
            Assert.NotNull(exception);
        }

        [Fact]
        public async Task CreateInvalidSurnameMaxLengthTest()
        {
            //Arrange
            var customer = new CustomerMngt.Domain.Entities.Customer
            {
                Password = "Password@01",
                Email = "test1@test.com",
                Surname =
                    "Surname Surname Surname Surname Surname Surname Surname Surname Surname Surname Surname Surname Surname Surname Surname Surname",
                FirstName = "First Name"
            };

            //Act
            var mockRepository = new Mock<ICustomerRepository>();
            var mockPassword = new Mock<IPasswordHasher>();
            var mockService = new CustomerService(mockRepository.Object, mockPassword.Object);
            var exception = await Assert.ThrowsAsync<ValidationException>(() => mockService.Create(customer));

            //Assert
            Assert.NotNull(exception);
        }

        [Fact]
        public async Task CreateInvalidSurnameMinLengthTest()
        {
            //Arrange
            var customer = new CustomerMngt.Domain.Entities.Customer
            {
                Password = "Password@01",
                Email = "test1@test.com",
                Surname = "S",
                FirstName = "First Name"
            };

            //Act
            var mockRepository = new Mock<ICustomerRepository>();
            var mockPassword = new Mock<IPasswordHasher>();
            var mockService = new CustomerService(mockRepository.Object, mockPassword.Object);
            var exception = await Assert.ThrowsAsync<ValidationException>(() => mockService.Create(customer));

            //Assert
            Assert.NotNull(exception);
        }

        [Fact]
        public async Task CreateInvalidEmailTest()
        {
            //Arrange
            var customer = new CustomerMngt.Domain.Entities.Customer
            {
                Password = "Password@01",
                Email = "test1",
                Surname = "Surname",
                FirstName = "First Name"
            };

            //Act
            var mockRepository = new Mock<ICustomerRepository>();
            var mockPassword = new Mock<IPasswordHasher>();
            var mockService = new CustomerService(mockRepository.Object, mockPassword.Object);
            var exception = await Assert.ThrowsAsync<ValidationException>(() => mockService.Create(customer));

            //Assert
            Assert.NotNull(exception);
        }

        [Fact]
        public async Task CreateInvalidEmailEmptyTest()
        {
            //Arrange
            var customer = new CustomerMngt.Domain.Entities.Customer
            {
                Password = "Password@01",
                Email = "",
                Surname = "Surname",
                FirstName = "First Name"
            };

            //Act
            var mockRepository = new Mock<ICustomerRepository>();
            var mockPassword = new Mock<IPasswordHasher>();
            var mockService = new CustomerService(mockRepository.Object, mockPassword.Object);
            var exception = await Assert.ThrowsAsync<ValidationException>(() => mockService.Create(customer));

            //Assert
            Assert.NotNull(exception);
        }

        [Fact]
        public async Task CreateInvalidPasswordEmptyTest()
        {
            //Arrange
            var customer = new CustomerMngt.Domain.Entities.Customer
            {
                Password = "",
                Email = "test1@test.com",
                Surname =
                    "Surname Surname Surname Surname Surname Surname Surname Surname Surname Surname Surname Surname Surname Surname Surname Surname",
                FirstName = "First Name"
            };

            //Act
            var mockRepository = new Mock<ICustomerRepository>();
            var mockPassword = new Mock<IPasswordHasher>();
            var mockService = new CustomerService(mockRepository.Object, mockPassword.Object);
            var exception = await Assert.ThrowsAsync<ValidationException>(() => mockService.Create(customer));

            //Assert
            Assert.NotNull(exception);
        }

        [Fact]
        public async Task CreateInvalidPasswordMaxLengthTest()
        {
            //Arrange
            var customer = new CustomerMngt.Domain.Entities.Customer
            {
                Password = "Password@0122222222222222222",
                Email = "test1@test.com",
                Surname = "Surname",
                FirstName = "First Name"
            };

            //Act
            var mockRepository = new Mock<ICustomerRepository>();
            var mockPassword = new Mock<IPasswordHasher>();
            var mockService = new CustomerService(mockRepository.Object, mockPassword.Object);
            var exception = await Assert.ThrowsAsync<ValidationException>(() => mockService.Create(customer));

            //Assert
            Assert.NotNull(exception);
        }

        [Fact]
        public async Task CreateInvalidPasswordMinLengthTest()
        {
            //Arrange
            var customer = new CustomerMngt.Domain.Entities.Customer
            {
                Password = "P@01",
                Email = "test1@test.com",
                Surname = "Surname",
                FirstName = "First Name"
            };

            //Act
            var mockRepository = new Mock<ICustomerRepository>();
            var mockPassword = new Mock<IPasswordHasher>();
            var mockService = new CustomerService(mockRepository.Object, mockPassword.Object);
            var exception = await Assert.ThrowsAsync<ValidationException>(() => mockService.Create(customer));

            //Assert
            Assert.NotNull(exception);
        }

        [Fact]
        public async Task UpdateInvalidIdTest()
        {
            //Arrange
            const long id = 0;
            var customer = new CustomerMngt.Domain.Entities.Customer
            {
                Password = "P@01",
                Email = "test1@test.com",
                Surname = "Surname",
                FirstName = "First Name"
            };

            //Act
            var mockRepository = new Mock<ICustomerRepository>();
            var mockPassword = new Mock<IPasswordHasher>();
            var mockService = new CustomerService(mockRepository.Object, mockPassword.Object);
            var exception = await Assert.ThrowsAsync<ValidationException>(() => mockService.Update(id, customer));

            //Assert
            Assert.NotNull(exception);
        }

        [Fact]
        public async Task UpdateInvalidCustomerTest()
        {
            //Arrange
            const long id = 1;
            
            //Act
            var mockRepository = new Mock<ICustomerRepository>();
            var mockPassword = new Mock<IPasswordHasher>();
            var mockService = new CustomerService(mockRepository.Object, mockPassword.Object);
            var exception = await Assert.ThrowsAsync<ValidationException>(() => mockService.Update(id, null));

            //Assert
            Assert.NotNull(exception);
        }

        [Fact]
        public async Task UpdateInvalidCustomerNotFoundTest()
        {
            //Arrange
            const long id = 1;
            
            var customer = new CustomerMngt.Domain.Entities.Customer
            {
                Id = 1,
                Password = "P@013333343",
                Email = "test1@test.com",
                Surname = "Surname",
                FirstName = "First Name"
            };

            //Act
            var mockRepository = new Mock<ICustomerRepository>();
            var mockPassword = new Mock<IPasswordHasher>();
            var mockService = new CustomerService(mockRepository.Object, mockPassword.Object);
            var exception = await Assert.ThrowsAsync<EntityNotFoundException>(() => mockService.Update(id, customer));

            //Assert
            Assert.NotNull(exception);
        }

        [Fact]
        public async Task DeleteInvalidIdTest()
        {
            //Arrange
            const long id = 0;
            
            //Act
            var mockRepository = new Mock<ICustomerRepository>();
            var mockPassword = new Mock<IPasswordHasher>();
            var mockService = new CustomerService(mockRepository.Object, mockPassword.Object);
            var exception = await Assert.ThrowsAsync<ValidationException>(() => mockService.Delete(id));

            //Assert
            Assert.NotNull(exception);
        }

        [Fact]
        public async Task DeleteInvalidNotFoundTest()
        {
            //Arrange
            const long id = 1;
            
            //Act
            var mockRepository = new Mock<ICustomerRepository>();
            var mockPassword = new Mock<IPasswordHasher>();
            var mockService = new CustomerService(mockRepository.Object, mockPassword.Object);
            var exception = await Assert.ThrowsAsync<EntityNotFoundException>(() => mockService.Delete(id));

            //Assert
            Assert.NotNull(exception);
        }

        [Fact]
        public async Task IsAvailableEmailTest()
        {
            //Arrange
            var customer = new CustomerMngt.Domain.Entities.Customer
            {
                Id = 1,
                Password = "P@013333343",
                Email = "test1@test.com",
                Surname = "Surname",
                FirstName = "First Name"
            };

            //Act
            var mockRepository = new Mock<ICustomerRepository>();
            var mockPassword = new Mock<IPasswordHasher>();
            var filter = new CustomerFilter {Email = customer.Email};
            mockRepository.Setup(x => x.GetByFilter(filter)).Returns(Task.FromResult(customer));

            //Assert
            var mockService = new CustomerService(mockRepository.Object, mockPassword.Object);
            var existingEmail = await mockService.IsAvailableEmail(customer.Email);
            Assert.True(existingEmail);
        }
    }
}
