using CF.Customer.Application.Dtos;
using CF.Customer.Application.Mappers;
using CF.Customer.Domain.Entities;
using CF.Customer.Domain.Models;
using CF.Customer.Infrastructure.Mappers;
using Xunit;

namespace CF.Customer.UnitTest.Infrastructure.Mappers;

public class CustomerMapperTest
{
    private readonly ICustomerMapper _mapper = new CustomerMapper();

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

        //Act
        var customer = _mapper.MapToCustomer(customerRequestDto);

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

        //Act
        var customerResponseDto = _mapper.MapToCustomerResponseDto(customer);

        //Assert
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

        //Act
        var customerResponseDtoPagination = _mapper.MapToPaginationDto(customerPagination);

        //Assert
        Assert.Equal(customerList.First().FirstName, customerResponseDtoPagination.Result.First().FirstName);
        Assert.Equal(customerList.First().Surname, customerResponseDtoPagination.Result.First().Surname);
        Assert.Equal(customerList.First().Email, customerResponseDtoPagination.Result.First().Email);
        Assert.Equal(customerList.First().Id, customerResponseDtoPagination.Result.First().Id);
        Assert.Equal(customerList.First().GetFullName(), customerResponseDtoPagination.Result.First().FullName);
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

        //Act
        var customerFilter = _mapper.MapToCustomerFilter(customerFilterDto);

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