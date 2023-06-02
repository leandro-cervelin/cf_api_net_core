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
    [Fact]
    public async Task GetTest()
    {
        //Arrange
        var customer = new Customer.Domain.Entities.Customer
        {
            Id = 1,
            Password = "Password@01",
            Email = "test@test.com",
            Surname = "Surname",
            FirstName = "FirstName",
            Updated = DateTime.Now,
            Created = DateTime.Now
        };

        var cancellationTokenSource = new CancellationTokenSource();

        //Act
        var mockRepository = new Mock<ICustomerRepository>();
        var mockPassword = new Mock<IPasswordHasherService>();
        var filter = new CustomerFilter {Id = 1};
        mockRepository.Setup(x => x.GetByFilterAsync(filter, cancellationTokenSource.Token)).ReturnsAsync(customer);
        var mockService = new CustomerService(mockRepository.Object, mockPassword.Object);
        var result = await mockService.GetByFilterAsync(filter, cancellationTokenSource.Token);

        //Assert
        Assert.Equal(customer.Id, result.Id);
    }

    [Fact]
    public async Task GetListTest()
    {
        //Arrange
        var customerOne = new Customer.Domain.Entities.Customer
        {
            Id = 1,
            Password = "Password@01",
            Email = "test1@test.com",
            Surname = "Surname",
            FirstName = "FirstName",
            Updated = DateTime.Now,
            Created = DateTime.Now
        };

        var customerTwo = new Customer.Domain.Entities.Customer
        {
            Id = 2,
            Password = "Password@01",
            Email = "test2@test.com",
            Surname = "Surname",
            FirstName = "FirstName",
            Updated = DateTime.Now,
            Created = DateTime.Now
        };

        var customers = new List<Customer.Domain.Entities.Customer>
        {
            customerOne,
            customerTwo
        };

        var cancellationTokenSource = new CancellationTokenSource();

        //Act
        var mockRepository = new Mock<ICustomerRepository>();
        var mockPassword = new Mock<IPasswordHasherService>();
        var filter = new CustomerFilter {PageSize = 10, CurrentPage = 1};
        mockRepository.Setup(x => x.CountByFilterAsync(filter, cancellationTokenSource.Token))
            .ReturnsAsync(customers.Count);
        mockRepository.Setup(x => x.GetListByFilterAsync(filter, cancellationTokenSource.Token))
            .ReturnsAsync(customers);
        var mockService = new CustomerService(mockRepository.Object, mockPassword.Object);
        var result = await mockService.GetListByFilterAsync(filter, cancellationTokenSource.Token);

        //Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task CreateInvalidFirstNameMinLengthTest()
    {
        //Arrange
        var customer = new Customer.Domain.Entities.Customer
        {
            Password = "Password@01",
            Email = "test1@test.com",
            Surname = "Surname",
            FirstName = "F"
        };

        var cancellationTokenSource = new CancellationTokenSource();

        //Act
        var mockRepository = new Mock<ICustomerRepository>();
        var mockPassword = new Mock<IPasswordHasherService>();
        var mockService = new CustomerService(mockRepository.Object, mockPassword.Object);
        var exception = await Assert.ThrowsAsync<ValidationException>(() => mockService.CreateAsync(customer, cancellationTokenSource.Token));

        //Assert
        Assert.NotNull(exception);
    }

    [Fact]
    public async Task CreateInvalidFirstNameEmptyTest()
    {
        //Arrange
        var customer = new Customer.Domain.Entities.Customer
        {
            Password = "Password@01",
            Email = "test1@test.com",
            Surname = "Surname",
            FirstName = ""
        };

        var cancellationTokenSource = new CancellationTokenSource();

        //Act
        var mockRepository = new Mock<ICustomerRepository>();
        var mockPassword = new Mock<IPasswordHasherService>();
        var mockService = new CustomerService(mockRepository.Object, mockPassword.Object);
        var exception = await Assert.ThrowsAsync<ValidationException>(() => mockService.CreateAsync(customer, cancellationTokenSource.Token));

        //Assert
        Assert.NotNull(exception);
    }

    [Fact]
    public async Task CreateInvalidFirstNameMaxLengthTest()
    {
        //Arrange
        var customer = new Customer.Domain.Entities.Customer
        {
            Password = "Password@01",
            Email = "test1@test.com",
            Surname = "Surname",
            FirstName =
                "First Name First Name First Name First Name First Name First Name First Name First Name First Name First Name First Name."
        };

        var cancellationTokenSource = new CancellationTokenSource();

        //Act
        var mockRepository = new Mock<ICustomerRepository>();
        var mockPassword = new Mock<IPasswordHasherService>();
        var mockService = new CustomerService(mockRepository.Object, mockPassword.Object);
        var exception = await Assert.ThrowsAsync<ValidationException>(() => mockService.CreateAsync(customer, cancellationTokenSource.Token));

        //Assert
        Assert.NotNull(exception);
    }

    [Fact]
    public async Task CreateInvalidSurnameEmptyTest()
    {
        //Arrange
        var customer = new Customer.Domain.Entities.Customer
        {
            Password = "Password@01",
            Email = "test1@test.com",
            Surname =
                "Surname Surname Surname Surname Surname Surname Surname Surname Surname Surname Surname Surname Surname Surname Surname Surname",
            FirstName = "First Name"
        };
        var cancellationTokenSource = new CancellationTokenSource();

        //Act
        var mockRepository = new Mock<ICustomerRepository>();
        var mockPassword = new Mock<IPasswordHasherService>();
        var mockService = new CustomerService(mockRepository.Object, mockPassword.Object);
        var exception = await Assert.ThrowsAsync<ValidationException>(() => mockService.CreateAsync(customer, cancellationTokenSource.Token));

        //Assert
        Assert.NotNull(exception);
    }

    [Fact]
    public async Task CreateInvalidSurnameMaxLengthTest()
    {
        //Arrange
        var customer = new Customer.Domain.Entities.Customer
        {
            Password = "Password@01",
            Email = "test1@test.com",
            Surname =
                "Surname Surname Surname Surname Surname Surname Surname Surname Surname Surname Surname Surname Surname Surname Surname Surname",
            FirstName = "First Name"
        };
        var cancellationTokenSource = new CancellationTokenSource();

        //Act
        var mockRepository = new Mock<ICustomerRepository>();
        var mockPassword = new Mock<IPasswordHasherService>();
        var mockService = new CustomerService(mockRepository.Object, mockPassword.Object);
        var exception = await Assert.ThrowsAsync<ValidationException>(() => mockService.CreateAsync(customer, cancellationTokenSource.Token));

        //Assert
        Assert.NotNull(exception);
    }

    [Fact]
    public async Task CreateInvalidSurnameMinLengthTest()
    {
        //Arrange
        var customer = new Customer.Domain.Entities.Customer
        {
            Password = "Password@01",
            Email = "test1@test.com",
            Surname = "S",
            FirstName = "First Name"
        };
        var cancellationTokenSource = new CancellationTokenSource();
        //Act
        var mockRepository = new Mock<ICustomerRepository>();
        var mockPassword = new Mock<IPasswordHasherService>();
        var mockService = new CustomerService(mockRepository.Object, mockPassword.Object);
        var exception = await Assert.ThrowsAsync<ValidationException>(() => mockService.CreateAsync(customer, cancellationTokenSource.Token));

        //Assert
        Assert.NotNull(exception);
    }

    [Fact]
    public async Task CreateInvalidEmailTest()
    {
        //Arrange
        var customer = new Customer.Domain.Entities.Customer
        {
            Password = "Password@01",
            Email = "test1",
            Surname = "Surname",
            FirstName = "First Name"
        };
        var cancellationTokenSource = new CancellationTokenSource();

        //Act
        var mockRepository = new Mock<ICustomerRepository>();
        var mockPassword = new Mock<IPasswordHasherService>();
        var mockService = new CustomerService(mockRepository.Object, mockPassword.Object);
        var exception = await Assert.ThrowsAsync<ValidationException>(() => mockService.CreateAsync(customer, cancellationTokenSource.Token));

        //Assert
        Assert.NotNull(exception);
    }

    [Fact]
    public async Task CreateInvalidEmailEmptyTest()
    {
        //Arrange
        var customer = new Customer.Domain.Entities.Customer
        {
            Password = "Password@01",
            Email = "",
            Surname = "Surname",
            FirstName = "First Name"
        };
        
        var cancellationTokenSource = new CancellationTokenSource();

        //Act
        var mockRepository = new Mock<ICustomerRepository>();
        var mockPassword = new Mock<IPasswordHasherService>();
        var mockService = new CustomerService(mockRepository.Object, mockPassword.Object);
        var exception = await Assert.ThrowsAsync<ValidationException>(() => mockService.CreateAsync(customer, cancellationTokenSource.Token));

        //Assert
        Assert.NotNull(exception);
    }

    [Fact]
    public async Task CreateInvalidPasswordEmptyTest()
    {
        //Arrange
        var customer = new Customer.Domain.Entities.Customer
        {
            Password = "",
            Email = "test1@test.com",
            Surname =
                "Surname Surname Surname Surname Surname Surname Surname Surname Surname Surname Surname Surname Surname Surname Surname Surname",
            FirstName = "First Name"
        };

        var cancellationTokenSource = new CancellationTokenSource();

        //Act
        var mockRepository = new Mock<ICustomerRepository>();
        var mockPassword = new Mock<IPasswordHasherService>();
        var mockService = new CustomerService(mockRepository.Object, mockPassword.Object);
        var exception = await Assert.ThrowsAsync<ValidationException>(() => mockService.CreateAsync(customer, cancellationTokenSource.Token));

        //Assert
        Assert.NotNull(exception);
    }

    [Fact]
    public async Task CreateInvalidPasswordMinLengthTest()
    {
        //Arrange
        var cancellationTokenSource = new CancellationTokenSource();

        var customer = new Customer.Domain.Entities.Customer
        {
            Password = "P@01",
            Email = "test1@test.com",
            Surname = "Surname",
            FirstName = "First Name"
        };

        //Act
        var mockRepository = new Mock<ICustomerRepository>();
        var mockPassword = new Mock<IPasswordHasherService>();
        var mockService = new CustomerService(mockRepository.Object, mockPassword.Object);
        var exception = await Assert.ThrowsAsync<ValidationException>(() => mockService.CreateAsync(customer, cancellationTokenSource.Token));

        //Assert
        Assert.NotNull(exception);
    }

    [Fact]
    public async Task UpdateInvalidIdTest()
    {
        //Arrange
        const long id = 0;
        var cancellationTokenSource = new CancellationTokenSource();

        var customer = new Customer.Domain.Entities.Customer
        {
            Password = "P@01",
            Email = "test1@test.com",
            Surname = "Surname",
            FirstName = "First Name"
        };

        //Act
        var mockRepository = new Mock<ICustomerRepository>();
        var mockPassword = new Mock<IPasswordHasherService>();
        var mockService = new CustomerService(mockRepository.Object, mockPassword.Object);
        var exception = await Assert.ThrowsAsync<ValidationException>(() => mockService.UpdateAsync(id, customer, cancellationTokenSource.Token));

        //Assert
        Assert.NotNull(exception);
    }

    [Fact]
    public async Task UpdateInvalidCustomerTest()
    {
        //Arrange
        const long id = 1;
        var cancellationTokenSource = new CancellationTokenSource();

        //Act
        var mockRepository = new Mock<ICustomerRepository>();
        var mockPassword = new Mock<IPasswordHasherService>();
        var mockService = new CustomerService(mockRepository.Object, mockPassword.Object);
        var exception = await Assert.ThrowsAsync<ValidationException>(() => mockService.UpdateAsync(id, null, cancellationTokenSource.Token));

        //Assert
        Assert.NotNull(exception);
    }

    [Fact]
    public async Task UpdateInvalidCustomerNotFoundTest()
    {
        //Arrange
        const long id = 1;
        var cancellationTokenSource = new CancellationTokenSource();

        var customer = new Customer.Domain.Entities.Customer
        {
            Id = 1,
            Password = "P@013333343",
            Email = "test1@test.com",
            Surname = "Surname",
            FirstName = "First Name"
        };

        //Act
        var mockRepository = new Mock<ICustomerRepository>();
        var mockPassword = new Mock<IPasswordHasherService>();
        var mockService = new CustomerService(mockRepository.Object, mockPassword.Object);
        var exception =
            await Assert.ThrowsAsync<EntityNotFoundException>(() => mockService.UpdateAsync(id, customer, cancellationTokenSource.Token));

        //Assert
        Assert.NotNull(exception);
    }

    [Fact]
    public async Task DeleteInvalidIdTest()
    {
        //Arrange
        const long id = 0;
        var cancellationTokenSource = new CancellationTokenSource();

        //Act
        var mockRepository = new Mock<ICustomerRepository>();
        var mockPassword = new Mock<IPasswordHasherService>();
        var mockService = new CustomerService(mockRepository.Object, mockPassword.Object);
        var exception = await Assert.ThrowsAsync<ValidationException>(() => mockService.DeleteAsync(id, cancellationTokenSource.Token));

        //Assert
        Assert.NotNull(exception);
    }

    [Fact]
    public async Task DeleteInvalidNotFoundTest()
    {
        //Arrange
        const long id = 1;
        var cancellationTokenSource = new CancellationTokenSource();

        //Act
        var mockRepository = new Mock<ICustomerRepository>();
        var mockPassword = new Mock<IPasswordHasherService>();
        var mockService = new CustomerService(mockRepository.Object, mockPassword.Object);
        var exception = await Assert.ThrowsAsync<EntityNotFoundException>(() => mockService.DeleteAsync(id, cancellationTokenSource.Token));

        //Assert
        Assert.NotNull(exception);
    }

    [Fact]
    public async Task IsAvailableEmailTest()
    {
        //Arrange
        var customer = new Customer.Domain.Entities.Customer
        {
            Id = 1,
            Password = "P@013333343",
            Email = "test1@test.com",
            Surname = "Surname",
            FirstName = "First Name"
        };

        var cancellationTokenSource = new CancellationTokenSource();

        //Act
        var mockRepository = new Mock<ICustomerRepository>();
        var mockPassword = new Mock<IPasswordHasherService>();
        var filter = new CustomerFilter {Email = customer.Email};
        mockRepository.Setup(x => x.GetByFilterAsync(filter, cancellationTokenSource.Token)).ReturnsAsync(customer);

        //Assert
        var mockService = new CustomerService(mockRepository.Object, mockPassword.Object);
        var existingEmail = await mockService.IsAvailableEmailAsync(customer.Email, cancellationTokenSource.Token);
        Assert.True(existingEmail);
    }
}