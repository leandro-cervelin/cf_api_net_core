using AutoMapper;
using CF.Customer.Application.Dtos;
using CF.Customer.Domain.Entities;
using CF.Customer.Domain.Models;
using CF.Customer.Infrastructure.Mappers;
using Xunit;

namespace CF.Customer.UnitTest.Infrastructure.Mappers;

public class CustomerProfileTest
{
    public MapperConfiguration MapperConfiguration =
        new(cfg => cfg.AddProfile<CustomerProfile>());

    [Fact]
    public void CustomerRequestDtoToCustomer()
    {
        //Arrange
        var customerRequestDto = new CustomerRequestDto
        {
            Surname = "Dickinson",
            FirstName = "Bruce",
            Password = "Blah@1234!",
            Email = "maiden@metal.com"
        };

        var mapper = MapperConfiguration.CreateMapper();

        //Act
        var customer = mapper.Map<Customer.Domain.Entities.Customer>(customerRequestDto);

        //Assert
        Assert.Equal(customerRequestDto.FirstName, customer.FirstName);
        Assert.Equal(customerRequestDto.Surname, customer.Surname);
        Assert.Equal(customerRequestDto.Password, customer.Password);
        Assert.Equal(customerRequestDto.Email, customer.Email);
    }

    [Fact]
    public void CustomerToCustomerResponseDto()
    {
        //Arrange
        var customer = new Customer.Domain.Entities.Customer
        {
            Surname = "Dickinson",
            FirstName = "Bruce",
            Email = "maiden@metal.com",
            Created = DateTime.Now,
            Updated = DateTime.Now,
            Id = 1
        };

        var mapper = MapperConfiguration.CreateMapper();

        //Act
        var customerResponseDto = mapper.Map<CustomerResponseDto>(customer);

        //Arrange
        Assert.Equal(customer.FirstName, customerResponseDto.FirstName);
        Assert.Equal(customer.Surname, customerResponseDto.Surname);
        Assert.Equal(customer.Email, customerResponseDto.Email);
        Assert.Equal(customer.Id, customerResponseDto.Id);
        Assert.Equal(customer.GetFullName(), customerResponseDto.FullName);
    }

    [Fact]
    public void CustomerPaginationToCustomerResponseDtoPagination()
    {
        //Arrange
        var customerList = new List<Customer.Domain.Entities.Customer>
        {
            new()
            {
                Surname = "Dickinson",
                FirstName = "Bruce",
                Email = "maiden@metal.com",
                Created = DateTime.Now,
                Updated = DateTime.Now,
                Id = 1
            }
        };

        var customerPagination = new Pagination<Customer.Domain.Entities.Customer>
        {
            Count = 1,
            CurrentPage = 1,
            PageSize = 1,
            Result = customerList
        };

        var mapper = MapperConfiguration.CreateMapper();

        //Act
        var customerResponseDtoPagination = mapper.Map<PaginationDto<CustomerResponseDto>>(customerPagination);
        var customerResponseDtoList = mapper.Map<List<CustomerResponseDto>>(customerResponseDtoPagination.Result);

        //Assert
        Assert.Equal(customerList.First().FirstName, customerResponseDtoList.First().FirstName);
        Assert.Equal(customerList.First().Surname, customerResponseDtoList.First().Surname);
        Assert.Equal(customerList.First().Email, customerResponseDtoList.First().Email);
        Assert.Equal(customerList.First().Id, customerResponseDtoList.First().Id);
        Assert.Equal(customerList.First().GetFullName(), customerResponseDtoList.First().FullName);
        Assert.Equal(1, customerResponseDtoPagination.TotalPages);
    }

    [Fact]
    public void CustomerFilterToCustomerFilterDto()
    {
        //Arrange
        var customerFilterDto = new CustomerFilterDto
        {
            Surname = "Dickinson",
            FirstName = "Bruce",
            Email = "maiden@metal.com",
            Id = 1,
            CurrentPage = 1,
            PageSize = 1,
            OrderBy = "desc",
            SortBy = "firstName"
        };

        var mapper = MapperConfiguration.CreateMapper();

        //Act
        var customerFilter = mapper.Map<CustomerFilter>(customerFilterDto);

        //Assert
        Assert.Equal(customerFilterDto.FirstName, customerFilter.FirstName);
        Assert.Equal(customerFilterDto.Surname, customerFilter.Surname);
        Assert.Equal(customerFilterDto.Id, customerFilter.Id);
        Assert.Equal(customerFilterDto.Email, customerFilter.Email);
        Assert.Equal(customerFilterDto.CurrentPage, customerFilter.CurrentPage);
        Assert.Equal(customerFilterDto.OrderBy, customerFilter.OrderBy);
        Assert.Equal(customerFilterDto.PageSize, customerFilter.PageSize);
        Assert.Equal(customerFilterDto.SortBy, customerFilter.SortBy);
    }
}