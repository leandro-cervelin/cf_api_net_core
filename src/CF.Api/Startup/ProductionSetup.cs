using System.Net;
using System.Text.Json;
using System.Threading.RateLimiting;
using CF.Api.Authentication;
using CF.Api.Middleware;
using CF.Customer.Infrastructure.DbContext;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CF.Api.Startup;

/// <summary>
///     What the API needs to run safely in production: rate limiting, trusted proxies, HTTPS redirection and HSTS,
///     security headers, a generic error response and health checks. None of it is needed to follow how a request reaches the
///     business logic, so it's kept out of Program.cs. See "Going further" in the README.
/// </summary>
public static class ProductionSetup
{
    public static IServiceCollection AddProductionSetup(this IServiceCollection services,
        IConfiguration configuration)
    {
        AddForwardedHeaders(services);
        AddRateLimiting(services, configuration);
        AddHealthChecks(services);

        return services;
    }

    public static WebApplication UseProductionSetup(this WebApplication app)
    {
        // First, so the rate limiter, HTTPS redirection and logs see the real client IP and scheme.
        app.UseForwardedHeaders();

        if (!app.Environment.IsDevelopment())
            app.UseHsts();

        app.UseMiddleware<SecurityHeadersMiddleware>();
        app.UseRateLimiter();

        // Anything ExceptionFilter didn't handle: log it and answer with a generic 500 ProblemDetails, never the
        // exception itself. In Development the detailed developer exception page is shown instead.
        if (!app.Environment.IsDevelopment())
            app.UseExceptionHandler();

        // On by default. Turn it off (HttpsRedirection:Enabled=false) only where nothing serves HTTPS, such as the
        // local Docker Compose setup; there it has no port to redirect to and just logs a warning on every start.
        if (app.Configuration.GetValue("HttpsRedirection:Enabled", true))
            app.UseHttpsRedirection();

        return app;
    }

    public static WebApplication MapHealthEndpoints(this WebApplication app)
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

        return app;
    }

    private static void AddForwardedHeaders(IServiceCollection services)
    {
        // X-Forwarded-* is trusted only from the proxies listed in config (plus loopback). Trusting it from anyone
        // would let clients pick their own IP and slip past the per-IP rate limit.
        services.AddOptions<ForwardedHeadersOptions>().Configure<IConfiguration>((options, configuration) =>
        {
            var section = configuration.GetSection("ForwardedHeaders");

            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            foreach (var proxy in section.GetSection("KnownProxies").Get<string[]>() ?? [])
                options.KnownProxies.Add(IPAddress.Parse(proxy));
            foreach (var network in section.GetSection("KnownNetworks").Get<string[]>() ?? [])
                options.KnownIPNetworks.Add(System.Net.IPNetwork.Parse(network));
        });
    }

    private static void AddRateLimiting(IServiceCollection services, IConfiguration configuration)
    {
        var rateLimitConfig = configuration.GetSection("RateLimiting");
        var permitLimit = rateLimitConfig.GetValue("PermitLimit", 60);
        var windowSeconds = rateLimitConfig.GetValue("WindowSeconds", 60);

        services.AddRateLimiter(options =>
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

    private static void AddHealthChecks(IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddDbContextCheck<CustomerContext>("database", HealthStatus.Unhealthy, ["db", "sql"])
            .AddCheck("self", () => HealthCheckResult.Healthy("API is running"), ["api"]);
    }
}
