using CF.Api.Authentication;
using CF.Api.Filters;
using CF.Api.Startup;
using CF.Customer.Infrastructure.DependencyInjection;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog writes all the logs, configured in the "Serilog" section of appsettings.json.
builder.Services.AddSerilog((services, logger) => logger
    .ReadFrom.Configuration(builder.Configuration)
    .ReadFrom.Services(services));
builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

// The customer core: business rules and database access, the same registration the console app uses.
builder.Services.AddCustomerCore(builder.Configuration.GetConnectionString("DbConnection")!);

// Controllers. ExceptionFilter turns domain exceptions (validation, not found) into HTTP responses.
builder.Services.AddControllers(x => x.Filters.Add<ExceptionFilter>());
builder.Services.AddProblemDetails();

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddApiDocumentation();
builder.Services.AddProductionSetup(builder.Configuration);

await using var app = builder.Build();

app.MigrateDatabaseIfEnabled();

// One log line per request: "HTTP GET /api/v1/customer/1 responded 200 in 12.3 ms". It leaves out the query string,
// since filters such as ?email= carry personal data. First, so it sees the final status code, including errors.
app.UseSerilogRequestLogging();
app.UseProductionSetup();
app.UseAuthentication();
app.UseAuthorization();

app.MapApiDocumentation();
app.MapControllers();
app.MapHealthEndpoints();

await app.RunAsync();
