﻿using System;
using System.Linq;
using CF.Api;
using CF.CustomerMngt.Infrastructure.DbContext;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CF.Test.Integration
{
    //for more about WebApplicationFactory: https://fullstackmark.com/post/20/painless-integration-testing-with-aspnet-core-web-api
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<Startup>
    {
        private readonly string _connectionString = $"DataSource={Guid.NewGuid()}.db";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Removing Existing Db Context
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<CustomerMngtContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                // Create a new service provider.
                var serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkSqlite()
                    .BuildServiceProvider();

                // Add a database context (AppDbContext) using an in-memory database for testing.
                services.AddDbContext<CustomerMngtContext>(options =>
                {
                    options.UseSqlite(_connectionString);
                    options.UseInternalServiceProvider(serviceProvider);
                });

                // Build the service provider.
                var buildServiceProvider = services.BuildServiceProvider();

                // Create a scope to obtain a reference to the database contexts
                using var scope = buildServiceProvider.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var appDb = scopedServices.GetRequiredService<CustomerMngtContext>();

                var logger = scopedServices.GetRequiredService<ILogger<CustomWebApplicationFactory<TStartup>>>();

                // Ensure the database is created.
                appDb.Database.EnsureCreated();

                try
                {
                    // Seed the database with some specific test data.
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"An error occurred seeding the database with test messages. Error: {ex.Message}");
                }
            });
        }
    }
}