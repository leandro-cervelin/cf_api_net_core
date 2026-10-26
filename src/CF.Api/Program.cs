using CF.Api.Authentication;
using CF.Api.Filters;
using CF.Api.Middleware;
using CF.Api.Startup;
using CF.Customer.Infrastructure.DependencyInjection;
using CorrelationId;
using CorrelationId.DependencyInjection;
using NLog.Web;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseNLog();
builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

// The customer core: business rules and database access, the same registration the console app uses.
builder.Services.AddCustomerCore(builder.Configuration.GetConnectionString("DbConnection")!);

// Controllers. ExceptionFilter turns domain exceptions (validation, not found) into HTTP responses.
builder.Services.AddControllers(x => x.Filters.Add<ExceptionFilter>());
builder.Services.AddProblemDetails();
builder.Services.AddDefaultCorrelationId(options =>
{
    options.LogLevelOptions = new CorrelationIdLogLevelOptions
    {
        FoundCorrelationIdHeader = LogLevel.Debug,
        MissingCorrelationIdHeader = LogLevel.Debug
    };
});

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddApiDocumentation();
builder.Services.AddProductionSetup(builder.Configuration);

await using var app = builder.Build();

app.MigrateDatabaseIfEnabled();

app.UseProductionSetup();
app.UseCorrelationId();
app.UseMiddleware<LogExceptionMiddleware>();
app.UseMiddleware<LogRequestMiddleware>();
app.UseMiddleware<LogResponseMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapApiDocumentation();
app.MapControllers();
app.MapHealthEndpoints();

await app.RunAsync();
