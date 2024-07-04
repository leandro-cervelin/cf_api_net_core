using CorrelationId.Abstractions;

namespace CF.Api.Middleware;

public class LogExceptionMiddleware(
    RequestDelegate next,
    ILogger<LogExceptionMiddleware> logger,
    ICorrelationContextAccessor correlationContext)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (Exception e)
        {
            var correlationId = correlationContext.CorrelationContext.CorrelationId;
            logger.LogError(e, "Unexpected exception. CorrelationId: {correlationId}", correlationId);
            throw;
        }
    }
}