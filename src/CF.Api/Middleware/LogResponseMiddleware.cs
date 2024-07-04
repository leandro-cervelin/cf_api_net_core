using CorrelationId.Abstractions;

namespace CF.Api.Middleware;

public class LogResponseMiddleware(
    RequestDelegate next,
    ILogger<LogResponseMiddleware> logger,
    ICorrelationContextAccessor correlationContext)
{
    public async Task Invoke(HttpContext context)
    {
        var correlationId = correlationContext.CorrelationContext.CorrelationId;

        logger.LogInformation("StatusCode: {statusCode}. (CorrelationId: {correlationId})",
            context.Response.StatusCode, correlationId);

        await next(context);
    }
}