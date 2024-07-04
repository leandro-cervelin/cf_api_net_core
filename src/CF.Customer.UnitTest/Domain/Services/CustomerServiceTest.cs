using CF.Customer.Domain.Exceptions;
using CF.Customer.Domain.Models;
using CF.Customer.Domain.Repositories;
using CF.Customer.Domain.Services;
using CF.Customer.Domain.Services.Interfaces;
using Moq;
using Xunit;

namespace CF.Customer.UnitTest.Domain.Services;

public class CustomerServiceTest
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly Mock<IPasswordHasherService> _mockPassword = new();
    private readonly Mock<ICustomerRepository> _mockRepository = new();

    [Fact]
    public async Task GetByFilterAsync_ReturnsCorrectCustomer()
    {
        // Arrange
        var customer = CreateCustomer();

        _mockRepository.Setup(x => x.GetByFilterAsync(It.IsAny<CustomerFilter>(), _cancellationTokenSource.Token))
            .ReturnsAsync(customer);
        var customerService = new CustomerService(_mockRepository.Object, _mockPassword.Object);

        // Act
        var result =
            await customerService.GetByFilterAsync(new CustomerFilter { Id = 1 }, _cancellationTokenSource.Token);

        // Assert
        Assert.Equal(customer.Id, result.Id);
    }

    [Fact]
    public async Task GetListTestAsync()
    {
        // Arrange
        var customerOne = CreateCustomer();
        var customerTwo = CreateCustomer(2, "test2@test.com");

        var customers = new List<Customer.Domain.Entities.Customer>
        {
            customerOne,
            customerTwo
        };

        // Act
        var filter = new CustomerFilter { PageSize = 10, CurrentPage = 1 };
        _mockRepository.Setup(x => x.CountByFilterAsync(It.IsAny<CustomerFilter>(), _cancellationTokenSource.Token))
            .ReturnsAsync(customers.Count);
        _mockRepository.Setup(x => x.GetListByFilterAsync(It.IsAny<CustomerFilter>(), _cancellationTokenSource.Token))
            .ReturnsAsync(customers);
        var customerService = new CustomerService(_mockRepository.Object, _mockPassword.Object);
        var result = await customerService.GetListByFilterAsync(filter, _cancellationTokenSource.Token);

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Theory]
    [InlineData(0, "F")]
    [InlineData(0, "")]
    [InlineData(0,
        "First Name First Name First Name First Name First Name First Name First Name First Name First Name First Name First Name.")]
    public async Task CreateAsync_InvalidFirstName_ThrowsValidationException(int id, string firstName)
    {
        // Arrange
        var customer = CreateCustomer(id);
        customer.FirstName = firstName;
        var customerService = new CustomerService(_mockRepository.Object, _mockPassword.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            customerService.CreateAsync(customer, _cancellationTokenSource.Token));
    }

    [Theory]
    [InlineData("")]
    [InlineData("S")]
    [InlineData(
        "Surname Surname Surname Surname Surname Surname Surname Surname Surname Surname Surname Surname Surname Surname Surname Surname")]
    public async Task CreateAsync_InvalidSurname_ThrowsValidationException(string surname)
    {
        // Arrange
        var customer = CreateCustomer();
        customer.Surname = surname;
        var customerService = new CustomerService(_mockRepository.Object, _mockPassword.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            customerService.CreateAsync(customer, _cancellationTokenSource.Token));
    }

    [Theory]
    [InlineData("invalid_email")]
    [InlineData("")]
    public async Task CreateAsync_InvalidEmail_ThrowsValidationException(string email)
    {
        // Arrange
        var customer = CreateCustomer();
        customer.Email = email;
        var customerService = new CustomerService(_mockRepository.Object, _mockPassword.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            customerService.CreateAsync(customer, _cancellationTokenSource.Token));
    }

    [Theory]
    [InlineData("")]
    [InlineData("P@01")]
    public async Task CreateAsync_InvalidPassword_ThrowsValidationException(string password)
    {
        // Arrange
        var customer = CreateCustomer();
        customer.Password = password;
        var customerService = new CustomerService(_mockRepository.Object, _mockPassword.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            customerService.CreateAsync(customer, _cancellationTokenSource.Token));
    }


    [Fact]
    public async Task UpdateInvalidIdTestAsync()
    {
        // Arrange
        var customer = CreateCustomer(0);

        // Act
        var customerService = new CustomerService(_mockRepository.Object, _mockPassword.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            customerService.UpdateAsync(customer.Id, customer, _cancellationTokenSource.Token));
    }

    [Fact]
    public async Task UpdateInvalidCustomerIsNullTestAsync()
    {
        // Arrange
        const long id = 1;

        var customerService = new CustomerService(_mockRepository.Object, _mockPassword.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            customerService.UpdateAsync(id, null, _cancellationTokenSource.Token));
    }

    [Fact]
    public async Task UpdateInvalidCustomerNotFoundTestAsync()
    {
        // Arrange
        var customer = CreateCustomer();

        var customerService = new CustomerService(_mockRepository.Object, _mockPassword.Object);

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            customerService.UpdateAsync(customer.Id, customer, _cancellationTokenSource.Token));
    }

    [Fact]
    public async Task DeleteInvalidIdTestAsync()
    {
        // Arrange
        const long id = 0;

        var customerService = new CustomerService(_mockRepository.Object, _mockPassword.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            customerService.DeleteAsync(id, _cancellationTokenSource.Token));
    }

    [Fact]
    public async Task DeleteAsync_InvalidNotFoundTest_ThrowsValidationException()
    {
        // Arrange
        const long id = 1;

        var customerService = new CustomerService(_mockRepository.Object, _mockPassword.Object);

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            customerService.DeleteAsync(id, _cancellationTokenSource.Token));
    }

    [Fact]
    public async Task IsAvailableEmailTestAsync()
    {
        // Arrange
        var customer = CreateCustomer();

        _mockRepository.Setup(x => x.GetByFilterAsync(It.IsAny<CustomerFilter>(), _cancellationTokenSource.Token))
            .ReturnsAsync((Customer.Domain.Entities.Customer)null);

        var customerService = new CustomerService(_mockRepository.Object, _mockPassword.Object);

        // Act
        var existingEmail = await customerService.IsAvailableEmailAsync(customer.Email, _cancellationTokenSource.Token);

        // Assert
        Assert.True(existingEmail);
    }

    [Fact]
    public async Task IsNotAvailableEmailTestAsync()
    {
        // Arrange
        var customer = CreateCustomer();

        var filter = new CustomerFilter { Email = customer.Email };
        _mockRepository.Setup(x => x.GetByFilterAsync(filter, _cancellationTokenSource.Token))
            .ReturnsAsync(customer);

        var customerService = new CustomerService(_mockRepository.Object, _mockPassword.Object);

        // Act
        var existingEmail = await customerService.IsAvailableEmailAsync(customer.Email, _cancellationTokenSource.Token);

        // Assert
        Assert.True(existingEmail);
    }

    private static Customer.Domain.Entities.Customer CreateCustomer(int id = 1, string email = "test1@test.com")
    {
        return new Customer.Domain.Entities.Customer
        {
            Id = id,
            Password = "Password@01",
            Email = email,
            Surname = "Surname",
            FirstName = "FirstName",
            Updated = DateTime.Now,
            Created = DateTime.Now
        };
    }
}