using AutoMapper;
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
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly Mock<ICustomerService> _mockService = new();

    [Fact]
    public async Task CreateTestAsync()
    {
        // Arrange
        var customer = CreateCustomer();
        var customerRequestDto = CreateCustomerRequestDto();
        const long id = 1;

        _mockMapper.Setup(x => x.Map<Customer.Domain.Entities.Customer>(customerRequestDto)).Returns(customer);
        _mockService.Setup(x => x.CreateAsync(customer, _cancellationTokenSource.Token)).ReturnsAsync(id);
        var mockFacade = new CustomerFacade(_mockService.Object, _mockMapper.Object);

        // Act
        var result = await mockFacade.CreateAsync(customerRequestDto, _cancellationTokenSource.Token);

        // Assert
        Assert.Equal(id, result);
        _mockService.Verify(
            x => x.CreateAsync(It.IsAny<Customer.Domain.Entities.Customer>(), _cancellationTokenSource.Token),
            Times.Once);
        _mockMapper.Verify(x => x.Map<Customer.Domain.Entities.Customer>(customerRequestDto), Times.Once);
    }

    [Fact]
    public async Task GetTestAsync()
    {
        // Arrange
        var customer = CreateCustomer();
        var customerResponseDto = CreateCustomerResponseDto();

        var filterDto = new CustomerFilterDto { Id = 1 };
        var filter = new CustomerFilter { Id = 1 };

        _mockMapper.Setup(x => x.Map<CustomerResponseDto>(customer)).Returns(customerResponseDto);
        _mockMapper.Setup(x => x.Map<CustomerFilter>(filterDto)).Returns(filter);
        _mockService.Setup(x => x.GetByFilterAsync(filter, _cancellationTokenSource.Token)).ReturnsAsync(customer);
        var mockFacade = new CustomerFacade(_mockService.Object, _mockMapper.Object);

        // Act
        var result = await mockFacade.GetByFilterAsync(filterDto, _cancellationTokenSource.Token);

        // Assert
        Assert.Equal(customer.Id, result.Id);
        _mockService.Verify(x => x.GetByFilterAsync(It.IsAny<CustomerFilter>(), _cancellationTokenSource.Token),
            Times.Once);
        _mockMapper.Verify(x => x.Map<CustomerFilter>(filterDto), Times.Once);
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

        var customersDto = new List<CustomerResponseDto>
        {
            CreateCustomerResponseDto(),
            CreateCustomerResponseDto(2)
        };
        var paginationDto = CreatePaginationDto(customersDto);

        var filterDto = new CustomerFilterDto { Id = 1 };
        var filter = new CustomerFilter { Id = 1 };

        _mockMapper.Setup(x => x.Map<CustomerFilter>(filterDto)).Returns(filter);
        _mockMapper.Setup(x => x.Map<PaginationDto<CustomerResponseDto>>(pagination)).Returns(paginationDto);
        _mockService.Setup(x => x.GetListByFilterAsync(filter, _cancellationTokenSource.Token))
            .ReturnsAsync(pagination);
        var mockFacade = new CustomerFacade(_mockService.Object, _mockMapper.Object);

        // Act
        var result = await mockFacade.GetListByFilterAsync(filterDto, _cancellationTokenSource.Token);

        // Assert
        Assert.Equal(paginationDto.Count, result.Count);
        _mockService.Verify(x => x.GetListByFilterAsync(It.IsAny<CustomerFilter>(), _cancellationTokenSource.Token),
            Times.Once);
        _mockMapper.Verify(x => x.Map<CustomerFilter>(filterDto), Times.Once);
        _mockMapper.Verify(x => x.Map<PaginationDto<CustomerResponseDto>>(pagination), Times.Once);
    }

    [Fact]
    public async Task UpdateTestAsync()
    {
        // Arrange
        var customerRequestDto = CreateCustomerRequestDto();
        var customer = CreateCustomer();
        const long id = 1;

        _mockMapper.Setup(x => x.Map<Customer.Domain.Entities.Customer>(customerRequestDto)).Returns(customer);
        var mockFacade = new CustomerFacade(_mockService.Object, _mockMapper.Object);

        // Act
        var exception = await Record.ExceptionAsync(() =>
            mockFacade.UpdateAsync(id, customerRequestDto, _cancellationTokenSource.Token));

        // Assert
        Assert.Null(exception);
        _mockMapper.Verify(x => x.Map<Customer.Domain.Entities.Customer>(customerRequestDto), Times.Once);
    }

    [Fact]
    public async Task DeleteTestAsync()
    {
        // Arrange
        const long id = 1;
        var mockFacade = new CustomerFacade(_mockService.Object, _mockMapper.Object);

        // Act
        var exception = await Record.ExceptionAsync(() => mockFacade.DeleteAsync(id, _cancellationTokenSource.Token));

        // Assert
        Assert.Null(exception);
    }

    private static PaginationDto<CustomerResponseDto> CreatePaginationDto(List<CustomerResponseDto> customersDto)
    {
        return new PaginationDto<CustomerResponseDto>
        {
            PageSize = 10,
            CurrentPage = 1,
            Count = 2,
            TotalPages = 1,
            Result = customersDto
        };
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

    private static CustomerResponseDto CreateCustomerResponseDto(int id = 1)
    {
        return new CustomerResponseDto
        {
            Id = id,
            Email = "test1@test.com",
            Surname = "Surname",
            FirstName = "First Name",
            FullName = "First Name Surname"
        };
    }
}