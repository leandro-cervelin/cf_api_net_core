using System;
using System.Data.Common;
using System.Threading.Tasks;
using CF.Customer.Domain.Models;
using CF.Customer.Infrastructure.DbContext;
using CF.Customer.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CF.Customer.UnitTest.Infrastructure.Repositories
{
    public class CustomerRepositoryTest
    {
        [Fact]
        public async Task GetListTest()
        {
            var connection = CreateSqLiteConnection();
            connection.Open();

            try
            {
                var options = SetDbContextOptionsBuilder(connection);

                await using var context = new CustomerContext(options);
                Assert.True(await context.Database.EnsureCreatedAsync());

                //Arrange
                var customerOne = new Customer.Domain.Entities.Customer
                {
                    Password = "Password01@",
                    Email = "test1@test.com",
                    Surname = "Surname1",
                    FirstName = "FirstName1",
                    Updated = DateTime.Now,
                    Created = DateTime.Now
                };

                var customerTwo = new Customer.Domain.Entities.Customer
                {
                    Password = "Password01@",
                    Email = "test2@test.com",
                    Surname = "Surname2",
                    FirstName = "FirstName2",
                    Updated = DateTime.Now,
                    Created = DateTime.Now
                };

                //Act
                var repository = new CustomerRepository(context);
                repository.Add(customerOne);
                repository.Add(customerTwo);
                await repository.SaveChangesAsync();

                //Assert
                var filter = new CustomerFilter {FirstName = "FirstName"};
                var result = await repository.GetListByFilterAsync(filter);
                Assert.Equal(2, result.Count);
            }
            finally
            {
                connection.Close();
            }
        }

        [Fact]
        public async Task GetTest()
        {
            var connection = CreateSqLiteConnection();
            connection.Open();

            try
            {
                var options = SetDbContextOptionsBuilder(connection);

                await using var context = new CustomerContext(options);
                Assert.True(await context.Database.EnsureCreatedAsync());

                //Arrange
                var customerOne = new Customer.Domain.Entities.Customer
                {
                    Password = "Password01@",
                    Email = "test1@test.com",
                    Surname = "Surname1",
                    FirstName = "FirstName1",
                    Updated = DateTime.Now,
                    Created = DateTime.Now
                };

                //Act
                var repository = new CustomerRepository(context);
                repository.Add(customerOne);
                await repository.SaveChangesAsync();

                //Assert
                var filter = new CustomerFilter {Email = "test1@test.com"};
                var result = await repository.GetByFilterAsync(filter);
                Assert.Equal("test1@test.com", result.Email);
            }
            finally
            {
                connection.Close();
            }
        }

        [Fact]
        public async Task CreateOkTest()
        {
            var connection = CreateSqLiteConnection();
            connection.Open();

            try
            {
                var options = SetDbContextOptionsBuilder(connection);

                await using var context = new CustomerContext(options);
                Assert.True(await context.Database.EnsureCreatedAsync());

                //Arrange
                var customer = new Customer.Domain.Entities.Customer
                {
                    Password = "Password01@",
                    Email = "test1@test.com",
                    Surname = "Surname1",
                    FirstName = "FirstName1",
                    Updated = DateTime.Now,
                    Created = DateTime.Now
                };

                //Act
                var repository = new CustomerRepository(context);
                repository.Add(customer);
                var result = await repository.SaveChangesAsync();

                //Assert
                Assert.Equal(1, result);
            }
            finally
            {
                connection.Close();
            }
        }

        [Fact]
        public async Task DeleteTest()
        {
            var connection = CreateSqLiteConnection();
            connection.Open();

            try
            {
                var options = SetDbContextOptionsBuilder(connection);

                await using var context = new CustomerContext(options);
                Assert.True(await context.Database.EnsureCreatedAsync());

                //Arrange
                var newCustomer = new Customer.Domain.Entities.Customer
                {
                    Password = "Password01@",
                    Email = "test1@test.com",
                    Surname = "Surname1",
                    FirstName = "FirstName1",
                    Updated = DateTime.Now,
                    Created = DateTime.Now
                };

                //Act
                var repository = new CustomerRepository(context);
                repository.Add(newCustomer);
                await repository.SaveChangesAsync();

                var filterStored = new CustomerFilter {Id = newCustomer.Id};
                var storedCustomer = await repository.GetByFilterAsync(filterStored);
                repository.Remove(storedCustomer);
                await repository.SaveChangesAsync();

                //Assert
                var filterNonExistentUser = new CustomerFilter {Id = newCustomer.Id};
                var nonExistentUser = await repository.GetByFilterAsync(filterNonExistentUser);
                Assert.Null(nonExistentUser);
            }
            finally
            {
                connection.Close();
            }
        }

        [Fact]
        public async Task DuplicatedEmailTest()
        {
            var connection = CreateSqLiteConnection();
            connection.Open();

            try
            {
                var options = SetDbContextOptionsBuilder(connection);

                await using var context = new CustomerContext(options);
                Assert.True(await context.Database.EnsureCreatedAsync());

                //Arrange
                var customerOne = new Customer.Domain.Entities.Customer
                {
                    Password = "Password01@",
                    Email = "test1@test.com",
                    Surname = "Surname1",
                    FirstName = "FirstName1",
                    Created = DateTime.Now
                };

                var customerTwo = new Customer.Domain.Entities.Customer
                {
                    Password = "Password01@",
                    Email = "test1@test.com",
                    Surname = "Surname2",
                    FirstName = "FirstName2",
                    Created = DateTime.Now
                };

                //Act
                var repository = new CustomerRepository(context);
                repository.Add(customerOne);
                await repository.SaveChangesAsync();

                //Assert
                repository.Add(customerTwo);
                var exception =
                    await Assert.ThrowsAsync<DbUpdateException>(() => repository.SaveChangesAsync());
                Assert.NotNull(exception);
            }
            finally
            {
                connection.Close();
            }
        }

        [Fact]
        public async Task CreateInvalidEmailTest()
        {
            var connection = CreateSqLiteConnection();
            connection.Open();

            try
            {
                var options = SetDbContextOptionsBuilder(connection);

                await using var context = new CustomerContext(options);
                Assert.True(await context.Database.EnsureCreatedAsync());

                //Arrange
                var customer = new Customer.Domain.Entities.Customer
                {
                    Password = "Password01@",
                    Email = null,
                    Surname = "Surname1",
                    FirstName = "FirstName1",
                    Created = DateTime.Now
                };

                //Act
                var repository = new CustomerRepository(context);
                repository.Add(customer);

                //Assert
                var exception =
                    await Assert.ThrowsAsync<DbUpdateException>(() => repository.SaveChangesAsync());
                Assert.NotNull(exception);
            }
            finally
            {
                connection.Close();
            }
        }

        [Fact]
        public async Task CreateInvalidPasswordTest()
        {
            var connection = CreateSqLiteConnection();
            connection.Open();

            try
            {
                var options = SetDbContextOptionsBuilder(connection);

                await using var context = new CustomerContext(options);
                Assert.True(await context.Database.EnsureCreatedAsync());

                //Arrange
                var customer = new Customer.Domain.Entities.Customer
                {
                    Password = null,
                    Email = "test@test.com",
                    Surname = "Surname1",
                    FirstName = "FirstName1",
                    Created = DateTime.Now
                };

                //Act
                var repository = new CustomerRepository(context);
                repository.Add(customer);

                //Assert
                var exception =
                    await Assert.ThrowsAsync<DbUpdateException>(() => repository.SaveChangesAsync());
                Assert.NotNull(exception);
            }
            finally
            {
                connection.Close();
            }
        }

        [Fact]
        public async Task CreateInvalidFirstNameTest()
        {
            var connection = CreateSqLiteConnection();
            connection.Open();

            try
            {
                var options = SetDbContextOptionsBuilder(connection);

                await using var context = new CustomerContext(options);
                Assert.True(await context.Database.EnsureCreatedAsync());

                //Arrange
                var customer = new Customer.Domain.Entities.Customer
                {
                    Password = "Passw0rd1",
                    Email = "test@test.com",
                    Surname = "Surname1",
                    FirstName = null,
                    Created = DateTime.Now
                };

                //Act
                var repository = new CustomerRepository(context);
                repository.Add(customer);

                //Assert
                var exception =
                    await Assert.ThrowsAsync<DbUpdateException>(() => repository.SaveChangesAsync());
                Assert.NotNull(exception);
            }
            finally
            {
                connection.Close();
            }
        }

        [Fact]
        public async Task CreateInvalidSurnameTest()
        {
            var connection = CreateSqLiteConnection();
            connection.Open();

            try
            {
                var options = SetDbContextOptionsBuilder(connection);

                await using var context = new CustomerContext(options);
                Assert.True(await context.Database.EnsureCreatedAsync());

                //Arrange
                var customer = new Customer.Domain.Entities.Customer
                {
                    Password = "Passw0rd1",
                    Email = "test@test.com",
                    Surname = null,
                    FirstName = "FirstName",
                    Created = DateTime.Now
                };

                //Act
                var repository = new CustomerRepository(context);
                repository.Add(customer);

                //Assert
                var exception = await Assert.ThrowsAsync<DbUpdateException>(() => repository.SaveChangesAsync());
                Assert.NotNull(exception);
            }
            finally
            {
                connection.Close();
            }
        }

        private static DbContextOptions<CustomerContext> SetDbContextOptionsBuilder(DbConnection connection)
        {
            return new DbContextOptionsBuilder<CustomerContext>()
                .UseSqlite(connection)
                .Options;
        }

        private static SqliteConnection CreateSqLiteConnection()
        {
            return new("DataSource=:memory:");
        }
    }
}