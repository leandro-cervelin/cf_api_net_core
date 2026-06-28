using System;
using System.Threading;
using System.Threading.Tasks;
using CF.Customer.Domain.Models;
using CF.Customer.Infrastructure.DbContext;
using CF.Customer.Infrastructure.Repositories;
using CF.IntegrationTest.Factories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CF.IntegrationTest.Infrastructure.Repositories;

public class CustomerRepositoryTest
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    [Fact]
    public async Task GetListTestAsync()
    {
        await using var context = await CreateContextAsync();
        Assert.True(await context.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken));

        //Arrange
        var customerOne = CreateCustomer();
        var customerTwo = CreateCustomer();
        customerTwo.Email = "email2@test.com";

        //Act
        var repository = new CustomerRepository(context);
        await repository.AddRangeAsync([customerOne, customerTwo]);
        await repository.SaveChangesAsync(_cancellationTokenSource.Token);

        var filter = new CustomerFilter { FirstName = "FirstName" };
        var result = await repository.GetListByFilterAsync(filter, _cancellationTokenSource.Token);

        //Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetTestAsync()
    {
        await using var context = await CreateContextAsync();
        Assert.True(await context.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken));

        //Arrange
        var customerOne = CreateCustomer();

        var repository = new CustomerRepository(context);
        repository.Add(customerOne);
        await repository.SaveChangesAsync(_cancellationTokenSource.Token);

        var filter = new CustomerFilter { Email = "test1@test.com" };

        //Act
        var result = await repository.GetByFilterAsync(filter, _cancellationTokenSource.Token);

        //Assert
        Assert.Equal("test1@test.com", result.Email);
    }

    [Fact]
    public async Task CreateOkTestAsync()
    {
        await using var context = await CreateContextAsync();
        Assert.True(await context.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken));

        //Arrange
        var customer = CreateCustomer();
        var repository = new CustomerRepository(context);
        repository.Add(customer);

        //Act
        var result = await repository.SaveChangesAsync(_cancellationTokenSource.Token);

        //Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task DeleteTestAsync()
    {
        await using var context = await CreateContextAsync();
        Assert.True(await context.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken));

        //Arrange
        var newCustomer = CreateCustomer();

        var repository = new CustomerRepository(context);
        repository.Add(newCustomer);
        await repository.SaveChangesAsync(_cancellationTokenSource.Token);

        var storedCustomer = await repository.GetByIdAsync(newCustomer.Id, _cancellationTokenSource.Token);
        repository.Remove(storedCustomer);
        await repository.SaveChangesAsync(_cancellationTokenSource.Token);

        //Act
        var nonExistentUser = await repository.GetByIdAsync(newCustomer.Id, _cancellationTokenSource.Token);

        //Assert
        Assert.Null(nonExistentUser);
    }

    [Fact]
    public async Task DuplicatedEmailTestAsync()
    {
        await using var context = await CreateContextAsync();
        Assert.True(await context.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken));

        //Arrange
        var customerOne = CreateCustomer();
        var customerTwo = CreateCustomer();
        customerTwo.FirstName = "FirstNameTwo";
        customerTwo.Surname = "Surname Two";

        var repository = new CustomerRepository(context);
        repository.Add(customerOne);
        await repository.SaveChangesAsync(_cancellationTokenSource.Token);

        repository.Add(customerTwo);

        //Act & Assert
        await Assert.ThrowsAsync<DbUpdateException>(() => repository.SaveChangesAsync(_cancellationTokenSource.Token));
    }

    [Fact]
    public async Task CreateInvalidEmailTestAsync()
    {
        await using var context = await CreateContextAsync();
        Assert.True(await context.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken));

        //Arrange
        var customer = CreateCustomer();
        customer.Email = null;

        var repository = new CustomerRepository(context);
        repository.Add(customer);

        //Act & Assert
        await Assert.ThrowsAsync<DbUpdateException>(() => repository.SaveChangesAsync(_cancellationTokenSource.Token));
    }

    [Fact]
    public async Task CreateInvalidPasswordTestAsync()
    {
        await using var context = await CreateContextAsync();
        Assert.True(await context.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken));

        //Arrange
        var customer = CreateCustomer();
        customer.Password = null;

        var repository = new CustomerRepository(context);
        repository.Add(customer);

        //Act & Assert
        await Assert.ThrowsAsync<DbUpdateException>(() => repository.SaveChangesAsync(_cancellationTokenSource.Token));
    }

    [Fact]
    public async Task CreateInvalidFirstNameTestAsync()
    {
        await using var context = await CreateContextAsync();
        Assert.True(await context.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken));

        //Arrange
        var customer = CreateCustomer();
        customer.FirstName = null;

        var repository = new CustomerRepository(context);
        repository.Add(customer);

        //Act & Assert
        await Assert.ThrowsAsync<DbUpdateException>(() => repository.SaveChangesAsync(_cancellationTokenSource.Token));
    }

    [Fact]
    public async Task CreateInvalidSurnameTestAsync()
    {
        await using var context = await CreateContextAsync();
        Assert.True(await context.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken));

        //Arrange
        var customer = CreateCustomer();
        customer.Surname = null;

        var repository = new CustomerRepository(context);
        repository.Add(customer);

        //Act & Assert
        await Assert.ThrowsAsync<DbUpdateException>(() => repository.SaveChangesAsync(_cancellationTokenSource.Token));
    }

    private static Customer.Domain.Entities.Customer CreateCustomer()
    {
        return new Customer.Domain.Entities.Customer
        {
            Password = "Password01@",
            Email = "test1@test.com",
            Surname = "Surname1",
            FirstName = "FirstName1",
            Updated = DateTime.Now,
            Created = DateTime.Now
        };
    }

    private static async Task<CustomerContext> CreateContextAsync()
    {
        var connectionString = await SqlServerContainer.GetConnectionStringAsync($"RepoTest_{Guid.NewGuid():N}");

        var options = new DbContextOptionsBuilder<CustomerContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new CustomerContext(options);
    }
}
