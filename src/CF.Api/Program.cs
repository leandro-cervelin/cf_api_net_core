using System.IO.Compression;
using CF.Api.Filters;
using CF.Api.Middleware;
using CF.Customer.Application.Facades;
using CF.Customer.Application.Facades.Interfaces;
using CF.Customer.Application.Mappers;
using CF.Customer.Domain.Repositories;
using CF.Customer.Domain.Services;
using CF.Customer.Domain.Services.Interfaces;
using CF.Customer.Infrastructure.DbContext;
using CF.Customer.Infrastructure.Mappers;
using CF.Customer.Infrastructure.Repositories;
using CorrelationId;
using CorrelationId.DependencyInjection;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using NLog.Web;
using Scalar.AspNetCore;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseNLog();

builder.Services.AddControllers(x => x.Filters.Add<ExceptionFilter>());
builder.Services.AddProblemDetails();
builder.Services.AddDefaultCorrelationId(ConfigureCorrelationId());
builder.Services.AddTransient<ICustomerFacade, CustomerFacade>();
builder.Services.AddTransient<ICustomerService, CustomerService>();
builder.Services.AddTransient<ICustomerRepository, CustomerRepository>();
builder.Services.AddTransient<IPasswordHasherService, PasswordHasherService>();
builder.Services.AddSingleton<ICustomerMapper, CustomerMapper>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Optimal);
builder.Services.AddResponseCompression(options => { options.Providers.Add<GzipCompressionProvider>(); });
AddDbContext();

AddNLog();

await using var app = builder.Build();

RunMigration();
app.UseCorrelationId();
AddExceptionHandler();
AddOpenApi();
app.UseMiddleware<LogExceptionMiddleware>();
app.UseMiddleware<LogRequestMiddleware>();
app.UseMiddleware<LogResponseMiddleware>();
app.UseHttpsRedirection();
app.MapControllers();

await app.RunAsync();

void AddExceptionHandler()
{
    if (app.Environment.IsDevelopment()) return;
    app.UseExceptionHandler(ConfigureExceptionHandler());
}

void AddOpenApi()
{
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
    }
}

void AddNLog()
{
    // NLog configuration is loaded automatically via UseNLog() and appsettings.json
    // No additional setup needed for .NET 10
}

void AddDbContext()
{
    if (builder.Environment.EnvironmentName.Contains("Test")) return;

    builder.Services.AddDbContext<CustomerContext>(options =>
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DbConnection"),
            a => { a.MigrationsAssembly("CF.Migrations"); });
    });
}

Action<CorrelationIdOptions> ConfigureCorrelationId()
{
    return options =>
    {
        options.LogLevelOptions = new CorrelationIdLogLevelOptions
        {
            FoundCorrelationIdHeader = LogLevel.Debug,
            MissingCorrelationIdHeader = LogLevel.Debug
        };
    };
}

Action<IApplicationBuilder> ConfigureExceptionHandler()
{
    return exceptionHandlerApp =>
    {
        exceptionHandlerApp.Run(async context =>
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            await context.Response.WriteAsJsonAsync(new
            {
                Message = "An unexpected internal exception occurred."
            });
        });
    };
}

void RunMigration()
{
    using var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();

    if (!serviceScope.ServiceProvider.GetRequiredService<CustomerContext>().Database.GetPendingMigrations()
            .Any()) return;

    serviceScope.ServiceProvider.GetRequiredService<CustomerContext>().Database.Migrate();
}

public partial class Program
{
}