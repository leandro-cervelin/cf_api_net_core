using CorrelationId.Abstractions;

namespace CF.Api.Middleware;

public class LogResponseMiddleware(RequestDelegate next, ILogger<LogResponseMiddleware> logger,
    ICorrelationContextAccessor correlationContext)
{
    private readonly ICorrelationContextAccessor _correlationContext = correlationContext;
    private readonly ILogger _logger = logger;
    private readonly RequestDelegate _next = next;

    public async Task Invoke(HttpContext context)
    {
        var correlationId = _correlationContext.CorrelationContext.CorrelationId;

        _logger.LogInformation("StatusCode: {statusCode}. (CorrelationId: {correlationId})",
            context.Response.StatusCode, correlationId);

        await _next(context);
    }
}