using System;
using System.Data.Common;
using System.Threading.Tasks;
using CF.CustomerMngt.Domain.Models;
using CF.CustomerMngt.Infrastructure.DbContext;
using CF.CustomerMngt.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CF.Test.Customer.Repository
{
    public class CustomerRepositoryTest
    {
        [Fact]
        public async Task GetListTest()
        {
            var connection = CreateSqliteConnection();
            connection.Open();

            try
            {
                var options = SetDbContextOptionsBuilder(connection);

                await using (var context = new CustomerMngtContext(options))
                {
                    context.Database.EnsureCreated();
                }

                //Arrange
                var customerOne = new CustomerMngt.Domain.Entities.Customer
                {
                    Password = "Password01@",
                    Email = "test1@test.com",
                    Surname = "Surname1",
                    FirstName = "FirstName1",
                    Updated = DateTime.Now,
                    Created = DateTime.Now
                };

                var customerTwo = new CustomerMngt.Domain.Entities.Customer
                {
                    Password = "Password01@",
                    Email = "test2@test.com",
                    Surname = "Surname2",
                    FirstName = "FirstName2",
                    Updated = DateTime.Now,
                    Created = DateTime.Now
                };

                //Act
                await using (var context = new CustomerMngtContext(options))
                { 
                    var repository = new CustomerRepository(context);
                    await repository.AddAsync(customerOne);
                    await repository.AddAsync(customerTwo);
                    await repository.SaveChangesAsync();
                }

                //Assert
                await using (var context = new CustomerMngtContext(options))
                {
                    var repository = new CustomerRepository(context);
                    var filter = new CustomerFilter {Email = "test"};
                    var result = await repository.GetListByFilterAsync(filter);
                    Assert.Equal(2, result.Count);
                }
            }
            finally
            {
                connection.Close();
            }
        }

        [Fact]
        public async Task GetTest()
        {
            var connection = CreateSqliteConnection();
            connection.Open();

            try
            {
                var options = SetDbContextOptionsBuilder(connection);

                await using (var context = new CustomerMngtContext(options))
                {
                    context.Database.EnsureCreated();
                }

                //Arrange
                var customerOne = new CustomerMngt.Domain.Entities.Customer
                {
                    Password = "Password01@",
                    Email = "test1@test.com",
                    Surname = "Surname1",
                    FirstName = "FirstName1",
                    Updated = DateTime.Now,
                    Created = DateTime.Now
                };

                //Act
                await using (var context = new CustomerMngtContext(options))
                { 
                    var repository = new CustomerRepository(context);
                    await repository.AddAsync(customerOne);
                    await repository.SaveChangesAsync();
                }

                //Assert
                await using (var context = new CustomerMngtContext(options))
                {
                    var repository = new CustomerRepository(context);
                    var filter = new CustomerFilter {Email = "test1@test.com"};
                    var result = await repository.GetByFilterAsync(filter);
                    Assert.Equal("test1@test.com", result.Email);
                }
            }
            finally
            {
                connection.Close();
            }
        }

        [Fact]
        public async Task CreateOkTest()
        {
            var connection = CreateSqliteConnection();
            connection.Open();

            try
            {
                var options = SetDbContextOptionsBuilder(connection);

                await using (var context = new CustomerMngtContext(options))
                {
                    context.Database.EnsureCreated();
                }

                //Arrange
                var customer = new CustomerMngt.Domain.Entities.Customer
                {
                    Password = "Password01@",
                    Email = "test1@test.com",
                    Surname = "Surname1",
                    FirstName = "FirstName1",
                    Updated = DateTime.Now,
                    Created = DateTime.Now
                };
               
                //Act
                await using (var context = new CustomerMngtContext(options))
                {
                    var repository = new CustomerRepository(context);
                    await repository.AddAsync(customer);
                    var result = await repository.SaveChangesAsync();
                    //Assert
                    Assert.Equal(1, result);
                }
            }
            finally
            {
                connection.Close();
            }
        }

        [Fact]
        public async Task DeleteTest()
        {
            var connection = CreateSqliteConnection();
            connection.Open();

            try
            {
                var options = SetDbContextOptionsBuilder(connection);

                await using (var context = new CustomerMngtContext(options))
                {
                    context.Database.EnsureCreated();
                }

                //Arrange
                var newCustomer = new CustomerMngt.Domain.Entities.Customer
                {
                    Password = "Password01@",
                    Email = "test1@test.com",
                    Surname = "Surname1",
                    FirstName = "FirstName1",
                    Updated = DateTime.Now,
                    Created = DateTime.Now
                };

                //Act
                await using (var context = new CustomerMngtContext(options))
                { 
                    var repository = new CustomerRepository(context);
                    await repository.AddAsync(newCustomer);
                    await repository.SaveChangesAsync();
                }

                await using (var context = new CustomerMngtContext(options))
                { 
                    var repository = new CustomerRepository(context);
                    var filterStored = new CustomerFilter {Id = newCustomer.Id};
                    var storedCustomer = await repository.GetByFilterAsync(filterStored);
                    repository.Remove(storedCustomer);
                    await repository.SaveChangesAsync();
                }

                //Assert
                await using (var context = new CustomerMngtContext(options))
                {
                    var filterNonExistentUser = new CustomerFilter {Id = newCustomer.Id};
                    var repository = new CustomerRepository(context);
                    var nonExistentUser = await repository.GetByFilterAsync(filterNonExistentUser);
                    Assert.Null(nonExistentUser);
                }
            }
            finally
            {
                connection.Close();
            }
        }

        [Fact]
        public async Task DuplicatedEmailTest()
        {
            var connection = CreateSqliteConnection();
            connection.Open();

            try
            {
                var options = SetDbContextOptionsBuilder(connection);

                await using (var context = new CustomerMngtContext(options))
                {
                    context.Database.EnsureCreated();
                }

                //Arrange
                var customerOne = new CustomerMngt.Domain.Entities.Customer
                {
                    Password = "Password01@",
                    Email = "test1@test.com",
                    Surname = "Surname1",
                    FirstName = "FirstName1",
                    Created = DateTime.Now
                };

                var customerTwo = new CustomerMngt.Domain.Entities.Customer
                {
                    Password = "Password01@",
                    Email = "test1@test.com",
                    Surname = "Surname2",
                    FirstName = "FirstName2",
                    Created = DateTime.Now
                };

                //Act
                await using (var context = new CustomerMngtContext(options))
                { 
                    var repository = new CustomerRepository(context);
                    await repository.AddAsync(customerOne);
                    await repository.SaveChangesAsync();
                }

                //Assert
                await using (var context = new CustomerMngtContext(options))
                { 
                    var repository = new CustomerRepository(context);
                    await repository.AddAsync(customerTwo);
                    var exception = await Assert.ThrowsAsync<DbUpdateException>(() => repository.SaveChangesAsync());
                    Assert.NotNull(exception);
                }
            }
            finally
            {
                connection.Close();
            }
        }

        [Fact]
        public async Task CreateInvalidEmailTest()
        {
            var connection = CreateSqliteConnection();
            connection.Open();

            try
            {
                var options = SetDbContextOptionsBuilder(connection);

                await using (var context = new CustomerMngtContext(options))
                {
                    context.Database.EnsureCreated();
                }

                //Arrange
                var customer = new CustomerMngt.Domain.Entities.Customer
                {
                    Password = "Password01@",
                    Email = null,
                    Surname = "Surname1",
                    FirstName = "FirstName1",
                    Created = DateTime.Now
                };
               
                //Act
                await using (var context = new CustomerMngtContext(options))
                {
                    var repository = new CustomerRepository(context);
                    await repository.AddAsync(customer);
                    
                    //Assert
                    var exception = await Assert.ThrowsAsync<DbUpdateException>(() => repository.SaveChangesAsync());
                    Assert.NotNull(exception);
                }
            }
            finally
            {
                connection.Close();
            }
        }

        [Fact]
        public async Task CreateInvalidPasswordTest()
        {
            var connection = CreateSqliteConnection();
            connection.Open();

            try
            {
                var options = SetDbContextOptionsBuilder(connection);

                await using (var context = new CustomerMngtContext(options))
                {
                    context.Database.EnsureCreated();
                }

                //Arrange
                var customer = new CustomerMngt.Domain.Entities.Customer
                {
                    Password = null,
                    Email = "test@test.com",
                    Surname = "Surname1",
                    FirstName = "FirstName1",
                    Created = DateTime.Now
                };
               
                //Act
                await using (var context = new CustomerMngtContext(options))
                {
                    var repository = new CustomerRepository(context);
                    await repository.AddAsync(customer);
                    
                    //Assert
                    var exception = await Assert.ThrowsAsync<DbUpdateException>(() => repository.SaveChangesAsync());
                    Assert.NotNull(exception);
                }
            }
            finally
            {
                connection.Close();
            }
        }

        [Fact]
        public async Task CreateInvalidFirstNameTest()
        {
            var connection = CreateSqliteConnection();
            connection.Open();

            try
            {
                var options = SetDbContextOptionsBuilder(connection);

                await using (var context = new CustomerMngtContext(options))
                {
                    context.Database.EnsureCreated();
                }

                //Arrange
                var customer = new CustomerMngt.Domain.Entities.Customer
                {
                    Password = "Passw0rd1",
                    Email = "test@test.com",
                    Surname = "Surname1",
                    FirstName = null,
                    Created = DateTime.Now
                };
               
                //Act
                await using (var context = new CustomerMngtContext(options))
                {
                    var repository = new CustomerRepository(context);
                    await repository.AddAsync(customer);
                    
                    //Assert
                    var exception = await Assert.ThrowsAsync<DbUpdateException>(() => repository.SaveChangesAsync());
                    Assert.NotNull(exception);
                }
            }
            finally
            {
                connection.Close();
            }
        }

        [Fact]
        public async Task CreateInvalidSurnameTest()
        {
            var connection = CreateSqliteConnection();
            connection.Open();

            try
            {
                var options = SetDbContextOptionsBuilder(connection);

                await using (var context = new CustomerMngtContext(options))
                {
                    context.Database.EnsureCreated();
                }

                //Arrange
                var customer = new CustomerMngt.Domain.Entities.Customer
                {
                    Password = "Passw0rd1",
                    Email = "test@test.com",
                    Surname = null,
                    FirstName = "FirstName",
                    Created = DateTime.Now
                };
               
                //Act
                await using (var context = new CustomerMngtContext(options))
                {
                    var repository = new CustomerRepository(context);
                    await repository.AddAsync(customer);
                
                    //Assert
                    var exception = await Assert.ThrowsAsync<DbUpdateException>(() => repository.SaveChangesAsync());
                    Assert.NotNull(exception);
                }
            }
            finally
            {
                connection.Close();
            }
        }

        private static DbContextOptions<CustomerMngtContext> SetDbContextOptionsBuilder(DbConnection connection)
        {
            return new DbContextOptionsBuilder<CustomerMngtContext>()
                .UseSqlite(connection)
                .Options;
        }

        private static SqliteConnection CreateSqliteConnection()
        {
            return new SqliteConnection("DataSource=:memory:");
        }
    }
}
