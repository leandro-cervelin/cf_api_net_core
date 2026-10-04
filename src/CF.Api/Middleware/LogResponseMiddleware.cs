using CorrelationId.Abstractions;

namespace CF.Api.Middleware;

public class LogResponseMiddleware(
    RequestDelegate next,
    ILogger<LogResponseMiddleware> logger,
    ICorrelationContextAccessor correlationContext)
{
    public async Task Invoke(HttpContext context)
    {
        await next(context);

        // Logged after the pipeline has run; before it, the status code is always the default 200.
        var correlationId = correlationContext.CorrelationContext?.CorrelationId;

        logger.LogInformation("StatusCode: {StatusCode}. (CorrelationId: {CorrelationId})",
            context.Response.StatusCode, correlationId);
    }
}