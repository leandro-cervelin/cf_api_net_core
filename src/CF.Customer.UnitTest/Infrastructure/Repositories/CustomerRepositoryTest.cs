using System.Data.Common;
using CF.Customer.Domain.Models;
using CF.Customer.Infrastructure.DbContext;
using CF.Customer.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CF.Customer.UnitTest.Infrastructure.Repositories;

public class CustomerRepositoryTest
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    [Fact]
    public async Task GetListTestAsync()
    {
        await using var connection = await CreateAndOpenSqliteConnectionAsync();

        var options = await SetDbContextOptionsBuilderAsync(connection);

        await using var context = new CustomerContext(options);
        Assert.True(await context.Database.EnsureCreatedAsync());

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
        await using var connection = await CreateAndOpenSqliteConnectionAsync();
        var options = await SetDbContextOptionsBuilderAsync(connection);

        await using var context = new CustomerContext(options);
        Assert.True(await context.Database.EnsureCreatedAsync());

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
        await using var connection = await CreateAndOpenSqliteConnectionAsync();
        var options = await SetDbContextOptionsBuilderAsync(connection);

        await using var context = new CustomerContext(options);
        Assert.True(await context.Database.EnsureCreatedAsync());

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
        await using var connection = await CreateAndOpenSqliteConnectionAsync();
        var options = await SetDbContextOptionsBuilderAsync(connection);

        await using var context = new CustomerContext(options);
        Assert.True(await context.Database.EnsureCreatedAsync());

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
        await using var connection = await CreateAndOpenSqliteConnectionAsync();
        var options = await SetDbContextOptionsBuilderAsync(connection);

        await using var context = new CustomerContext(options);
        Assert.True(await context.Database.EnsureCreatedAsync());

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
        await using var connection = await CreateAndOpenSqliteConnectionAsync();
        var options = await SetDbContextOptionsBuilderAsync(connection);

        await using var context = new CustomerContext(options);
        Assert.True(await context.Database.EnsureCreatedAsync());

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
        await using var connection = await CreateAndOpenSqliteConnectionAsync();
        var options = await SetDbContextOptionsBuilderAsync(connection);

        await using var context = new CustomerContext(options);
        Assert.True(await context.Database.EnsureCreatedAsync());

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
        await using var connection = await CreateAndOpenSqliteConnectionAsync();
        var options = await SetDbContextOptionsBuilderAsync(connection);

        await using var context = new CustomerContext(options);
        Assert.True(await context.Database.EnsureCreatedAsync());

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
        await using var connection = await CreateAndOpenSqliteConnectionAsync();
        var options = await SetDbContextOptionsBuilderAsync(connection);

        await using var context = new CustomerContext(options);
        Assert.True(await context.Database.EnsureCreatedAsync());

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

    private static async Task<DbContextOptions<CustomerContext>> SetDbContextOptionsBuilderAsync(
        DbConnection connection)
    {
        return await Task.FromResult(new DbContextOptionsBuilder<CustomerContext>()
            .UseSqlite(connection)
            .Options);
    }

    private static async Task<SqliteConnection> CreateAndOpenSqliteConnectionAsync()
    {
        var connection = await CreateSqLiteConnectionAsync();
        await connection.OpenAsync();
        return connection;
    }

    private static async Task<SqliteConnection> CreateSqLiteConnectionAsync()
    {
        return await Task.FromResult(new SqliteConnection("DataSource=:memory:"));
    }
}