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
    public async Task CreateAsync_Success_HashesPassword()
    {
        // Arrange
        var customer = CreateCustomer();
        const string hashedPassword = "$2a$11$hashedPasswordExample";

        _mockRepository.Setup(x => x.GetByFilterAsync(It.IsAny<CustomerFilter>(), _cancellationTokenSource.Token))
            .ReturnsAsync((Customer.Domain.Entities.Customer)null);
        _mockPassword.Setup(x => x.Hash(customer.Password)).Returns(hashedPassword);
        _mockRepository.Setup(x => x.SaveChangesAsync(_cancellationTokenSource.Token)).ReturnsAsync(1);

        var customerService = new CustomerService(_mockRepository.Object, _mockPassword.Object);

        // Act
        await customerService.CreateAsync(customer, _cancellationTokenSource.Token);

        // Assert
        _mockPassword.Verify(x => x.Hash(It.IsAny<string>()), Times.Once);
        Assert.Equal(hashedPassword, customer.Password);
    }

    [Fact]
    public async Task UpdateAsync_PasswordChanged_HashesNewPassword()
    {
        // Arrange
        var existingCustomer = CreateCustomer();
        const string oldHashedPassword = "$2a$11$oldHashedPassword";
        existingCustomer.Password = oldHashedPassword;

        var updatedCustomer = CreateCustomer();
        const string newPlainPassword = "NewPassword@123";
        updatedCustomer.Password = newPlainPassword;

        const string newHashedPassword = "$2a$11$newHashedPassword";

        _mockRepository.Setup(x => x.GetByIdAsync(existingCustomer.Id, _cancellationTokenSource.Token))
            .ReturnsAsync(existingCustomer);
        _mockRepository.Setup(x => x.GetByFilterAsync(It.IsAny<CustomerFilter>(), _cancellationTokenSource.Token))
            .ReturnsAsync((Customer.Domain.Entities.Customer)null);
        _mockPassword.Setup(x => x.Verify(newPlainPassword, oldHashedPassword)).Returns(false);
        _mockPassword.Setup(x => x.Hash(newPlainPassword)).Returns(newHashedPassword);
        _mockRepository.Setup(x => x.SaveChangesAsync(_cancellationTokenSource.Token)).ReturnsAsync(1);

        var customerService = new CustomerService(_mockRepository.Object, _mockPassword.Object);

        // Act
        await customerService.UpdateAsync(existingCustomer.Id, updatedCustomer, null, false, _cancellationTokenSource.Token);

        // Assert
        _mockPassword.Verify(x => x.Verify(newPlainPassword, oldHashedPassword), Times.Once);
        _mockPassword.Verify(x => x.Hash(newPlainPassword), Times.Once);
        Assert.Equal(newHashedPassword, existingCustomer.Password);
    }

    [Fact]
    public async Task UpdateAsync_PasswordUnchanged_DoesNotHashPassword()
    {
        // Arrange
        var existingCustomer = CreateCustomer();
        existingCustomer.Password = "$2a$11$hashedPassword";

        var updatedCustomer = CreateCustomer();
        updatedCustomer.Password = "Password@01"; // Original password before hashing

        _mockRepository.Setup(x => x.GetByIdAsync(existingCustomer.Id, _cancellationTokenSource.Token))
            .ReturnsAsync(existingCustomer);
        _mockRepository.Setup(x => x.GetByFilterAsync(It.IsAny<CustomerFilter>(), _cancellationTokenSource.Token))
            .ReturnsAsync((Customer.Domain.Entities.Customer)null);
        _mockPassword.Setup(x => x.Verify(updatedCustomer.Password, existingCustomer.Password)).Returns(true);
        _mockRepository.Setup(x => x.SaveChangesAsync(_cancellationTokenSource.Token)).ReturnsAsync(1);

        var customerService = new CustomerService(_mockRepository.Object, _mockPassword.Object);

        // Act
        await customerService.UpdateAsync(existingCustomer.Id, updatedCustomer, null, false, _cancellationTokenSource.Token);

        // Assert
        _mockPassword.Verify(x => x.Verify(updatedCustomer.Password, existingCustomer.Password), Times.Once);
        _mockPassword.Verify(x => x.Hash(It.IsAny<string>()), Times.Never);
        Assert.Equal("$2a$11$hashedPassword", existingCustomer.Password); // Password unchanged
    }

    [Fact]
    public async Task UpdateAsync_EmailCaseOnlyChange_SkipsAvailabilityCheckAndUpdates()
    {
        // Arrange
        var existingCustomer = CreateCustomer(email: "test1@test.com");
        var updatedCustomer = CreateCustomer(email: "Test1@Test.com");

        _mockRepository.Setup(x => x.GetByIdAsync(existingCustomer.Id, _cancellationTokenSource.Token))
            .ReturnsAsync(existingCustomer);
        // The database collation is case-insensitive, so a lookup would find the customer's own row.
        _mockRepository.Setup(x => x.GetByFilterAsync(It.IsAny<CustomerFilter>(), _cancellationTokenSource.Token))
            .ReturnsAsync(existingCustomer);
        _mockPassword.Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

        var customerService = new CustomerService(_mockRepository.Object, _mockPassword.Object);

        // Act
        await customerService.UpdateAsync(existingCustomer.Id, updatedCustomer, null, false, _cancellationTokenSource.Token);

        // Assert
        Assert.Equal("Test1@Test.com", existingCustomer.Email);
        _mockRepository.Verify(
            x => x.GetByFilterAsync(It.IsAny<CustomerFilter>(), _cancellationTokenSource.Token), Times.Never);
    }

    [Theory]
    [InlineData(false, "test1@test.com", false)] // name-only change
    [InlineData(false, "Test1@Test.com", false)] // email case-only change
    [InlineData(true, "test1@test.com", true)] // password change
    [InlineData(false, "other@test.com", true)] // email change
    public async Task UpdateAsync_RotatesSecurityStampOnlyWhenCredentialsChange(bool passwordChanged, string newEmail,
        bool expectRotation)
    {
        // Arrange
        var existingCustomer = CreateCustomer(email: "test1@test.com");
        existingCustomer.SecurityStamp = "original-stamp";

        var updatedCustomer = CreateCustomer(email: newEmail);
        updatedCustomer.FirstName = "Renamed";

        _mockRepository.Setup(x => x.GetByIdAsync(existingCustomer.Id, _cancellationTokenSource.Token))
            .ReturnsAsync(existingCustomer);
        _mockRepository.Setup(x => x.GetByFilterAsync(It.IsAny<CustomerFilter>(), _cancellationTokenSource.Token))
            .ReturnsAsync((Customer.Domain.Entities.Customer)null);
        _mockPassword.Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(!passwordChanged);
        _mockPassword.Setup(x => x.Hash(It.IsAny<string>())).Returns("$2a$11$newHash");

        var customerService = new CustomerService(_mockRepository.Object, _mockPassword.Object);

        // Act
        await customerService.UpdateAsync(existingCustomer.Id, updatedCustomer, null, false, _cancellationTokenSource.Token);

        // Assert
        Assert.Equal(expectRotation, existingCustomer.SecurityStamp != "original-stamp");
        Assert.False(string.IsNullOrEmpty(existingCustomer.SecurityStamp));
    }

    [Theory]
    [InlineData(null, "The current password is required to change the password.")]
    [InlineData("", "The current password is required to change the password.")]
    [InlineData("Wrong@Pass1", "The current password is incorrect.")]
    public async Task UpdateAsync_PasswordChangeWithoutValidCurrentPassword_Throws(string currentPassword,
        string expectedMessage)
    {
        // Arrange
        var (existingCustomer, updatedCustomer) = SetupPasswordChange();
        _mockPassword.Setup(x => x.Verify("Wrong@Pass1", "$2a$11$oldHash")).Returns(false);

        var customerService = new CustomerService(_mockRepository.Object, _mockPassword.Object);

        // Act
        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            customerService.UpdateAsync(existingCustomer.Id, updatedCustomer, currentPassword, true,
                _cancellationTokenSource.Token));

        // Assert
        Assert.Equal(expectedMessage, exception.Message);
        Assert.Equal("$2a$11$oldHash", existingCustomer.Password);
        _mockRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_PasswordChangeWithCorrectCurrentPassword_ChangesPassword()
    {
        // Arrange
        var (existingCustomer, updatedCustomer) = SetupPasswordChange();
        _mockPassword.Setup(x => x.Verify("Old@Password1", "$2a$11$oldHash")).Returns(true);

        var customerService = new CustomerService(_mockRepository.Object, _mockPassword.Object);

        // Act
        await customerService.UpdateAsync(existingCustomer.Id, updatedCustomer, "Old@Password1", true,
            _cancellationTokenSource.Token);

        // Assert
        Assert.Equal("$2a$11$newHash", existingCustomer.Password);
    }

    [Fact]
    public async Task UpdateAsync_PasswordChangeWithoutVerification_ChangesPassword()
    {
        // Arrange: an admin editing another customer's account.
        var (existingCustomer, updatedCustomer) = SetupPasswordChange();

        var customerService = new CustomerService(_mockRepository.Object, _mockPassword.Object);

        // Act
        await customerService.UpdateAsync(existingCustomer.Id, updatedCustomer, null, false,
            _cancellationTokenSource.Token);

        // Assert
        Assert.Equal("$2a$11$newHash", existingCustomer.Password);
    }

    [Fact]
    public async Task UpdateAsync_UnchangedPassword_DoesNotRequireCurrentPassword()
    {
        // Arrange: sending the existing password already proves the caller knows it.
        var existingCustomer = CreateCustomer();
        existingCustomer.Password = "$2a$11$oldHash";
        var updatedCustomer = CreateCustomer(email: "changed@test.com");

        _mockRepository.Setup(x => x.GetByIdAsync(existingCustomer.Id, _cancellationTokenSource.Token))
            .ReturnsAsync(existingCustomer);
        _mockRepository.Setup(x => x.GetByFilterAsync(It.IsAny<CustomerFilter>(), _cancellationTokenSource.Token))
            .ReturnsAsync((Customer.Domain.Entities.Customer)null);
        _mockPassword.Setup(x => x.Verify(updatedCustomer.Password, "$2a$11$oldHash")).Returns(true);

        var customerService = new CustomerService(_mockRepository.Object, _mockPassword.Object);

        // Act
        await customerService.UpdateAsync(existingCustomer.Id, updatedCustomer, null, true,
            _cancellationTokenSource.Token);

        // Assert
        Assert.Equal("changed@test.com", existingCustomer.Email);
    }

    [Theory]
    [InlineData("password", "asc", "OrderBy must be one of: firstName, surname, email.")]
    [InlineData("firstName", "sideways", "SortBy must be one of: asc, desc.")]
    public async Task GetListByFilterAsync_InvalidSorting_Throws(string orderBy, string sortBy, string expectedMessage)
    {
        // Arrange
        var customerService = new CustomerService(_mockRepository.Object, _mockPassword.Object);
        var filter = new CustomerFilter { OrderBy = orderBy, SortBy = sortBy };

        // Act
        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            customerService.GetListByFilterAsync(filter, _cancellationTokenSource.Token));

        // Assert
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Theory]
    [InlineData("FIRSTNAME", "ASC")]
    [InlineData("email", "desc")]
    public async Task GetListByFilterAsync_SortingIsCaseInsensitive(string orderBy, string sortBy)
    {
        // Arrange
        var customerService = new CustomerService(_mockRepository.Object, _mockPassword.Object);
        var filter = new CustomerFilter { OrderBy = orderBy, SortBy = sortBy };

        // Act
        var exception = await Record.ExceptionAsync(() =>
            customerService.GetListByFilterAsync(filter, _cancellationTokenSource.Token));

        // Assert
        Assert.Null(exception);
    }

    private (Customer.Domain.Entities.Customer Existing, Customer.Domain.Entities.Customer Updated)
        SetupPasswordChange()
    {
        var existingCustomer = CreateCustomer();
        existingCustomer.Password = "$2a$11$oldHash";
        var updatedCustomer = CreateCustomer();
        updatedCustomer.Password = "New@Password1";

        _mockRepository.Setup(x => x.GetByIdAsync(existingCustomer.Id, _cancellationTokenSource.Token))
            .ReturnsAsync(existingCustomer);
        _mockPassword.Setup(x => x.Verify("New@Password1", "$2a$11$oldHash")).Returns(false);
        _mockPassword.Setup(x => x.Hash("New@Password1")).Returns("$2a$11$newHash");

        return (existingCustomer, updatedCustomer);
    }

    [Fact]
    public async Task CreateAsync_AssignsSecurityStamp()
    {
        // Arrange
        var customer = CreateCustomer();
        customer.SecurityStamp = null!;

        _mockRepository.Setup(x => x.GetByFilterAsync(It.IsAny<CustomerFilter>(), _cancellationTokenSource.Token))
            .ReturnsAsync((Customer.Domain.Entities.Customer)null);
        _mockPassword.Setup(x => x.Hash(It.IsAny<string>())).Returns("$2a$11$hash");

        var customerService = new CustomerService(_mockRepository.Object, _mockPassword.Object);

        // Act
        await customerService.CreateAsync(customer, _cancellationTokenSource.Token);

        // Assert
        Assert.Equal(32, customer.SecurityStamp.Length);
    }

    [Fact]
    public async Task AuthenticateAsync_ValidCredentials_ReturnsCustomer()
    {
        // Arrange
        var customer = CreateCustomer();
        customer.Password = "$2a$11$hashedPassword";

        _mockRepository.Setup(x => x.GetByFilterAsync(It.IsAny<CustomerFilter>(), _cancellationTokenSource.Token))
            .ReturnsAsync(customer);
        _mockPassword.Setup(x => x.Verify("Password@01", customer.Password)).Returns(true);

        var customerService = new CustomerService(_mockRepository.Object, _mockPassword.Object);

        // Act
        var result = await customerService.AuthenticateAsync(customer.Email, "Password@01",
            _cancellationTokenSource.Token);

        // Assert
        Assert.Same(customer, result);
    }

    [Fact]
    public async Task AuthenticateAsync_WrongPassword_ReturnsNull()
    {
        // Arrange
        var customer = CreateCustomer();

        _mockRepository.Setup(x => x.GetByFilterAsync(It.IsAny<CustomerFilter>(), _cancellationTokenSource.Token))
            .ReturnsAsync(customer);
        _mockPassword.Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        var customerService = new CustomerService(_mockRepository.Object, _mockPassword.Object);

        // Act
        var result = await customerService.AuthenticateAsync(customer.Email, "Wrong@Password1",
            _cancellationTokenSource.Token);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AuthenticateAsync_UnknownEmail_ReturnsNullAfterEquivalentHashWork()
    {
        // Arrange
        _mockRepository.Setup(x => x.GetByFilterAsync(It.IsAny<CustomerFilter>(), _cancellationTokenSource.Token))
            .ReturnsAsync((Customer.Domain.Entities.Customer)null);

        var customerService = new CustomerService(_mockRepository.Object, _mockPassword.Object);

        // Act
        var result = await customerService.AuthenticateAsync("nobody@test.com", "Password@01",
            _cancellationTokenSource.Token);

        // Assert
        Assert.Null(result);
        _mockPassword.Verify(x => x.Hash("Password@01"), Times.Once);
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
            customerService.UpdateAsync(customer.Id, customer, null, false, _cancellationTokenSource.Token));
    }

    [Fact]
    public async Task UpdateInvalidCustomerIsNullTestAsync()
    {
        // Arrange
        const long id = 1;

        var customerService = new CustomerService(_mockRepository.Object, _mockPassword.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            customerService.UpdateAsync(id, null, null, false, _cancellationTokenSource.Token));
    }

    [Fact]
    public async Task UpdateInvalidCustomerNotFoundTestAsync()
    {
        // Arrange
        var customer = CreateCustomer();

        var customerService = new CustomerService(_mockRepository.Object, _mockPassword.Object);

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            customerService.UpdateAsync(customer.Id, customer, null, false, _cancellationTokenSource.Token));
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

        _mockRepository.Setup(x => x.GetByFilterAsync(It.IsAny<CustomerFilter>(), _cancellationTokenSource.Token))
            .ReturnsAsync(customer);

        var customerService = new CustomerService(_mockRepository.Object, _mockPassword.Object);

        // Act
        var existingEmail = await customerService.IsAvailableEmailAsync(customer.Email, _cancellationTokenSource.Token);

        // Assert
        Assert.False(existingEmail);
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
            Updated = DateTime.UtcNow,
            Created = DateTime.UtcNow
        };
    }
}