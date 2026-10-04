using System.IO.Compression;
using System.Net;
using System.Text.Json;
using System.Threading.RateLimiting;
using Asp.Versioning;
using CF.Api.Authentication;
using CF.Api.Filters;
using CF.Api.Middleware;
using CF.Api.OpenApi;
using CF.Customer.Infrastructure.DbContext;
using CF.Customer.Infrastructure.DependencyInjection;
using CorrelationId;
using CorrelationId.DependencyInjection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NLog.Web;
using Scalar.AspNetCore;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseNLog();
builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

builder.Services.AddControllers(x => x.Filters.Add<ExceptionFilter>());
builder.Services.AddProblemDetails();
builder.Services.AddDefaultCorrelationId(ConfigureCorrelationId());
builder.Services.AddCustomerCore(builder.Configuration.GetConnectionString("DbConnection")!);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Optimal);
builder.Services.AddResponseCompression(options => { options.Providers.Add<GzipCompressionProvider>(); });
builder.Services.AddMemoryCache();
AddForwardedHeaders();
AddRateLimiting();
AddApiVersioning();
AddHealthChecks();
await using var app = builder.Build();

RunMigration();
// First, so the rate limiter, HTTPS redirection and logs see the real client IP and scheme.
app.UseForwardedHeaders();
AddHsts();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseRateLimiter();
app.UseCorrelationId();
AddExceptionHandler();
AddOpenApi();
app.UseMiddleware<LogExceptionMiddleware>();
app.UseMiddleware<LogRequestMiddleware>();
app.UseMiddleware<LogResponseMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
MapHealthChecks();

await app.RunAsync();

void AddHsts()
{
    if (app.Environment.IsDevelopment()) return;
    app.UseHsts();
}

void AddForwardedHeaders()
{
    // X-Forwarded-* is trusted only from the proxies listed in config (plus loopback). Trusting it from anyone
    // would let clients pick their own IP and slip past the per-IP rate limit.
    builder.Services.AddOptions<ForwardedHeadersOptions>().Configure<IConfiguration>((options, configuration) =>
    {
        var section = configuration.GetSection("ForwardedHeaders");

        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        foreach (var proxy in section.GetSection("KnownProxies").Get<string[]>() ?? [])
            options.KnownProxies.Add(IPAddress.Parse(proxy));
        foreach (var network in section.GetSection("KnownNetworks").Get<string[]>() ?? [])
            options.KnownIPNetworks.Add(System.Net.IPNetwork.Parse(network));
    });
}

void AddExceptionHandler()
{
    if (app.Environment.IsDevelopment()) return;
    app.UseExceptionHandler(ConfigureExceptionHandler());
}

void AddOpenApi()
{
    if (!app.Environment.IsDevelopment()) return;
    app.MapOpenApi().WithDocumentPerVersion();
    app.MapScalarApiReference(options => options.AddPreferredSecuritySchemes(BearerSecurityTransformer.SchemeName));
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
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    }).AddMvc().AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    }).AddOpenApi(options =>
    {
        options.Document.AddDocumentTransformer<BearerSecurityTransformer>();
        options.Document.AddOperationTransformer<BearerSecurityTransformer>();
    });
}

void AddHealthChecks()
{
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
    }).RequireAuthorization(policy => policy.RequireRole(Roles.Admin)); // check names and timings are internal detail

    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("db")
    }).DisableRateLimiting(); // probes poll constantly; they must never be throttled

    app.MapHealthChecks("/health/live", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("api")
    }).DisableRateLimiting();
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
    // Off by default: with several instances, startup migrations race each other and the app login needs DDL
    // rights. Deployments run the migration bundle as a separate step instead (see README).
    if (!builder.Configuration.GetValue("Database:MigrateOnStartup", false)) return;

    using var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();

    var context = serviceScope.ServiceProvider.GetRequiredService<CustomerContext>();

    context.Database.Migrate();
}
