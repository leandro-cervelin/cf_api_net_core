﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using CF.CustomerMngt.Application.Dtos;
using CF.CustomerMngt.Application.Facades;
using CF.CustomerMngt.Domain.Models;
using CF.CustomerMngt.Domain.Services.Interfaces;
using Moq;
using Xunit;
using Xunit.Sdk;

namespace CF.Test.Customer.Facade
{
    public class CustomerFacadeTest
    {
        [Fact]
        public async Task CreateTest()
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

            var customerRequestDto = new CustomerRequestDto
            {
                Email = "test1@test.com",
                Surname = "Surname",
                FirstName = "First Name",
                Password = "P@013333343",
                ConfirmPassword = "P@013333343",
            };

            //Act
            const long id = 1;
            var mockService = new Mock<ICustomerService>();
            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<CustomerMngt.Domain.Entities.Customer>(customerRequestDto)).Returns(customer);
            mockService.Setup(x => x.Create(customer)).ReturnsAsync(id);
            //Assert
            var mockFacade = new CustomerFacade(mockService.Object, mockMapper.Object);
            var result = await mockFacade.Create(customerRequestDto);
            Assert.Equal(id, result);
        }

        [Fact]
        public async Task GetTest()
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

            var customerResponseDto = new CustomerResponseDto
            {
                Id = 1,
                Email = "test1@test.com",
                Surname = "Surname",
                FirstName = "First Name",
                FullName = "First Name Surname"
            };

            var filterDto = new CustomerFilterDto {Id = 1};
            var filter = new CustomerFilter {Id = 1};

            //Act
            var mockService = new Mock<ICustomerService>();
            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<CustomerResponseDto>(customer)).Returns(customerResponseDto);
            mockMapper.Setup(x => x.Map<CustomerFilter>(filterDto)).Returns(filter);
            mockService.Setup(x => x.GetByFilter(filter)).ReturnsAsync(customer);

            //Assert
            var mockFacade = new CustomerFacade(mockService.Object, mockMapper.Object);
            var result = await mockFacade.GetByFilter(filterDto);
            Assert.Equal(customer.Id, result.Id);
        }

        [Fact]
        public async Task GetListTest()
        {
            //Arrange
            var customerOne = new CustomerMngt.Domain.Entities.Customer
            {
                Id = 1,
                Password = "sdfdsfdsfds",
                Email = "test@test.com",
                Surname = "Surname",
                FirstName = "Ronaldo",
                Updated = DateTime.Now,
                Created = DateTime.Now
            };

            var customerTwo = new CustomerMngt.Domain.Entities.Customer
            {
                Id = 2,
                Password = "sdfdsfdsfds",
                Email = "test@test.com",
                Surname = "Surname",
                FirstName = "Ronaldinho",
                Updated = DateTime.Now,
                Created = DateTime.Now
            };

            var customers = new List<CustomerMngt.Domain.Entities.Customer>
            {
                customerOne,
                customerTwo
            };

            var pagination = new Pagination<CustomerMngt.Domain.Entities.Customer>
            {
                PageSize = 10,
                CurrentPage = 1,
                Count = 2,
                Result = customers
            };

            var customerOneDto = new CustomerResponseDto
            {
                Id = 1,
                Email = "test@test.com",
                Surname = "Surname",
                FirstName = "Ronaldo"
            };

            var customerTwoDto = new CustomerResponseDto
            {
                Id = 2,
                Email = "test@test.com",
                Surname = "Surname",
                FirstName = "Ronaldinho"
            };

            var customersDto = new List<CustomerResponseDto>
            {
                customerOneDto,
                customerTwoDto
            };

            var paginationDto = new PaginationDto<CustomerResponseDto>
            {
                PageSize = 10,
                CurrentPage = 1,
                Count = 2,
                TotalPages = 1,
                Result = customersDto
            };

            var filterDto = new CustomerFilterDto {Id = 1};
            var filter = new CustomerFilter {Id = 1};

            //Act
            var mockService = new Mock<ICustomerService>();
            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<CustomerFilter>(filterDto)).Returns(filter);
            mockMapper.Setup(x => x.Map<PaginationDto<CustomerResponseDto>>(pagination)).Returns(paginationDto);
            mockService.Setup(x => x.GetListByFilter(filter)).ReturnsAsync(pagination);

            //Assert
            var mockFacade = new CustomerFacade(mockService.Object, mockMapper.Object);
            var result = await mockFacade.GetListByFilter(filterDto);
            Assert.Equal(paginationDto.Count, result.Count);
        }

        [Fact]
        public async Task UpdateTest()
        {
            //Arrange
            var customerRequestDto = new CustomerRequestDto
            {
                Password = "Passrrr@1",
                ConfirmPassword = "Passrrr@1",
                Email = "test@test.com",
                Surname = "Surname",
                FirstName = "First Name"
            };

            var customer = new CustomerMngt.Domain.Entities.Customer
            {
                Password = "Passrrr@1",
                Email = "test@test.com",
                Surname = "Surname",
                FirstName = "First Name"
            };

            const long id = 1;

            //Act
            var mockService = new Mock<ICustomerService>();
            var mockMapper = new Mock<IMapper>();

            //Assert
            mockMapper.Setup(x => x.Map<CustomerMngt.Domain.Entities.Customer>(customerRequestDto)).Returns(customer);
            var mockFacade = new CustomerFacade(mockService.Object, mockMapper.Object);
            try
            {
                await Assert.ThrowsAsync<Exception>(() => mockFacade.Update(id, customerRequestDto));
            }
            catch (AssertActualExpectedException exception)
            {
                Assert.Equal("(No exception was thrown)", exception.Actual);
            }
        }

        [Fact]
        public async Task DeleteTest()
        {
            //Arrange
            const long id = 1;

            //Act
            var mockService = new Mock<ICustomerService>();
            var mockMapper = new Mock<IMapper>();

            //Assert
            var mockFacade = new CustomerFacade(mockService.Object, mockMapper.Object);
            try
            {
                await Assert.ThrowsAsync<Exception>(() => mockFacade.Delete(id));
            }
            catch (AssertActualExpectedException exception)
            {
                Assert.Equal("(No exception was thrown)", exception.Actual);
            }
        }
    }
}
