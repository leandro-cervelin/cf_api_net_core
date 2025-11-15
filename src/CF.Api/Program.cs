using System.IO.Compression;
using System.Text.Json;
using System.Threading.RateLimiting;
using Asp.Versioning;
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
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
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
builder.Services.AddResponseCaching();
builder.Services.AddMemoryCache();
AddRateLimiting();
AddApiVersioning();
AddDbContext();
AddHealthChecks();
await using var app = builder.Build();

RunMigration();
app.UseRateLimiter();
app.UseCorrelationId();
AddExceptionHandler();
AddOpenApi();
app.UseMiddleware<LogExceptionMiddleware>();
app.UseMiddleware<LogRequestMiddleware>();
app.UseMiddleware<LogResponseMiddleware>();
app.UseResponseCaching();
app.UseHttpsRedirection();
app.MapControllers();
MapHealthChecks();

await app.RunAsync();

void AddExceptionHandler()
{
    if (app.Environment.IsDevelopment()) return;
    app.UseExceptionHandler(ConfigureExceptionHandler());
}

void AddOpenApi()
{
    if (!app.Environment.IsDevelopment()) return;
    app.MapOpenApi();
    app.MapScalarApiReference();
}

void AddRateLimiting()
{
    var rateLimitConfig = builder.Configuration.GetSection("RateLimiting");
    var permitLimit = rateLimitConfig.GetValue("PermitLimit", 60);
    var windowSeconds = rateLimitConfig.GetValue("WindowSeconds", 60);

    builder.Services.AddRateLimiter(options =>
    {
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        {
            var clientId = context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";

            return RateLimitPartition.GetFixedWindowLimiter(clientId, _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = TimeSpan.FromSeconds(windowSeconds),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
        });

        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.OnRejected = async (context, cancellationToken) =>
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.HttpContext.Response.ContentType = "application/json";

            var response = new
            {
                statusCode = 429,
                message = $"Rate limit exceeded. Maximum {permitLimit} requests per {windowSeconds} seconds allowed.",
                retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
                    ? retryAfter.TotalSeconds
                    : windowSeconds
            };

            await context.HttpContext.Response.WriteAsJsonAsync(response, cancellationToken);
        };
    });
}

void AddApiVersioning()
{
    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    }).AddMvc().AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });
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

void AddHealthChecks()
{
    builder.Services.AddHealthChecks();

    if (builder.Environment.EnvironmentName.Contains("Test")) return;

    builder.Services.AddHealthChecks()
        .AddDbContextCheck<CustomerContext>("database", HealthStatus.Unhealthy, ["db", "sql"])
        .AddCheck("self", () => HealthCheckResult.Healthy("API is running"), ["api"]);
}

void MapHealthChecks()
{
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        Predicate = _ => true,
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            var result = JsonSerializer.Serialize(new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    duration = e.Value.Duration.TotalMilliseconds
                }),
                totalDuration = report.TotalDuration.TotalMilliseconds
            });
            await context.Response.WriteAsync(result);
        }
    });

    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("db")
    });

    app.MapHealthChecks("/health/live", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("api")
    });
}

static Action<CorrelationIdOptions> ConfigureCorrelationId()
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

static Action<IApplicationBuilder> ConfigureExceptionHandler()
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

public partial class Program;