using CF.Customer.Application.Dtos;
using CF.Customer.Application.Facades;
using CF.Customer.Domain.Models;
using CF.Customer.Domain.Services.Interfaces;
using Moq;
using Xunit;

namespace CF.Customer.UnitTest.Application.Facades;

public class CustomerFacadeTest
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly Mock<ICustomerService> _mockService = new();

    [Fact]
    public async Task CreateTestAsync()
    {
        // Arrange
        var customerRequestDto = CreateCustomerRequestDto();
        const long id = 1;

        _mockService.Setup(x =>
                x.CreateAsync(It.IsAny<Customer.Domain.Entities.Customer>(), _cancellationTokenSource.Token))
            .ReturnsAsync(id);
        var facade = new CustomerFacade(_mockService.Object);

        // Act
        var result = await facade.CreateAsync(customerRequestDto, _cancellationTokenSource.Token);

        // Assert
        Assert.Equal(id, result);
        _mockService.Verify(x => x.CreateAsync(
                It.Is<Customer.Domain.Entities.Customer>(c =>
                    c.Email == customerRequestDto.Email && c.FirstName == customerRequestDto.FirstName &&
                    c.Surname == customerRequestDto.Surname && c.Password == customerRequestDto.Password),
                _cancellationTokenSource.Token),
            Times.Once);
    }

    [Fact]
    public async Task GetTestAsync()
    {
        // Arrange
        var customer = CreateCustomer();
        var filterDto = new CustomerFilterDto { Id = 1 };

        _mockService.Setup(x => x.GetByFilterAsync(It.IsAny<CustomerFilter>(), _cancellationTokenSource.Token))
            .ReturnsAsync(customer);
        var facade = new CustomerFacade(_mockService.Object);

        // Act
        var result = await facade.GetByFilterAsync(filterDto, _cancellationTokenSource.Token);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(customer.Id, result.Id);
        Assert.Equal(customer.Email, result.Email);
        Assert.Equal("First Name Surname", result.FullName);
        _mockService.Verify(
            x => x.GetByFilterAsync(It.Is<CustomerFilter>(f => f.Id == filterDto.Id), _cancellationTokenSource.Token),
            Times.Once);
    }

    [Fact]
    public async Task GetTestAsync_NotFound_ReturnsNull()
    {
        // Arrange
        var facade = new CustomerFacade(_mockService.Object);

        // Act
        var result = await facade.GetByFilterAsync(new CustomerFilterDto { Id = 1 }, _cancellationTokenSource.Token);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetListTestAsync()
    {
        // Arrange
        var customers = new List<Customer.Domain.Entities.Customer>
        {
            CreateCustomer(),
            CreateCustomer(2)
        };
        var pagination = CreatePagination(customers);
        var filterDto = new CustomerFilterDto { FirstName = "First Name", PageSize = 10, CurrentPage = 1 };

        _mockService.Setup(x => x.GetListByFilterAsync(It.IsAny<CustomerFilter>(), _cancellationTokenSource.Token))
            .ReturnsAsync(pagination);
        var facade = new CustomerFacade(_mockService.Object);

        // Act
        var result = await facade.GetListByFilterAsync(filterDto, _cancellationTokenSource.Token);

        // Assert
        Assert.Equal(pagination.Count, result.Count);
        Assert.Equal([1L, 2L], result.Result.Select(c => c.Id));
        _mockService.Verify(x => x.GetListByFilterAsync(
                It.Is<CustomerFilter>(f => f.FirstName == filterDto.FirstName && f.PageSize == filterDto.PageSize),
                _cancellationTokenSource.Token),
            Times.Once);
    }

    [Fact]
    public async Task UpdateTestAsync()
    {
        // Arrange
        var customerRequestDto = CreateCustomerRequestDto() with { CurrentPassword = "Old@Password1" };
        const long id = 1;
        var facade = new CustomerFacade(_mockService.Object);

        // Act
        await facade.UpdateAsync(id, customerRequestDto, true, _cancellationTokenSource.Token);

        // Assert
        _mockService.Verify(x => x.UpdateAsync(id,
                It.Is<Customer.Domain.Entities.Customer>(c => c.Email == customerRequestDto.Email),
                customerRequestDto.CurrentPassword, true, _cancellationTokenSource.Token),
            Times.Once);
    }

    [Fact]
    public async Task DeleteTestAsync()
    {
        // Arrange
        const long id = 1;
        var facade = new CustomerFacade(_mockService.Object);

        // Act
        await facade.DeleteAsync(id, _cancellationTokenSource.Token);

        // Assert
        _mockService.Verify(x => x.DeleteAsync(id, _cancellationTokenSource.Token), Times.Once);
    }

    [Fact]
    public async Task AuthenticateAsync_ValidCredentials_ReturnsMappedCustomer()
    {
        // Arrange
        var customer = CreateCustomer();
        customer.SecurityStamp = "stamp";
        var loginRequestDto = new LoginRequestDto { Email = customer.Email, Password = "P@013333343" };

        _mockService.Setup(x => x.AuthenticateAsync(loginRequestDto.Email, loginRequestDto.Password,
            _cancellationTokenSource.Token)).ReturnsAsync(customer);
        var facade = new CustomerFacade(_mockService.Object);

        // Act
        var result = await facade.AuthenticateAsync(loginRequestDto, _cancellationTokenSource.Token);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(customer.Id, result.Id);
        Assert.Equal(customer.Email, result.Email);
        Assert.Equal("stamp", result.SecurityStamp);
    }

    [Fact]
    public async Task AuthenticateAsync_InvalidCredentials_ReturnsNull()
    {
        // Arrange
        var loginRequestDto = new LoginRequestDto { Email = "test1@test.com", Password = "Wrong@Password1" };
        var facade = new CustomerFacade(_mockService.Object);

        // Act
        var result = await facade.AuthenticateAsync(loginRequestDto, _cancellationTokenSource.Token);

        // Assert
        Assert.Null(result);
    }

    private static Pagination<Customer.Domain.Entities.Customer> CreatePagination(
        List<Customer.Domain.Entities.Customer> customers)
    {
        return new Pagination<Customer.Domain.Entities.Customer>
        {
            PageSize = 10,
            CurrentPage = 1,
            Count = 2,
            Result = customers
        };
    }

    private static CustomerRequestDto CreateCustomerRequestDto()
    {
        return new CustomerRequestDto
        {
            Email = "test1@test.com",
            Surname = "Surname",
            FirstName = "First Name",
            Password = "P@013333343",
            ConfirmPassword = "P@013333343"
        };
    }

    private static Customer.Domain.Entities.Customer CreateCustomer(int id = 1)
    {
        return new Customer.Domain.Entities.Customer
        {
            Id = id,
            Password = "P@013333343",
            Email = "test1@test.com",
            Surname = "Surname",
            FirstName = "First Name"
        };
    }
}
