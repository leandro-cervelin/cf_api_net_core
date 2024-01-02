using CorrelationId.Abstractions;

namespace CF.Api.Middleware;

public class LogExceptionMiddleware(RequestDelegate next, ILogger<LogExceptionMiddleware> logger,
    ICorrelationContextAccessor correlationContext)
{
    private readonly ICorrelationContextAccessor _correlationContext = correlationContext;
    private readonly ILogger _logger = logger;
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception e)
        {
            var correlationId = _correlationContext.CorrelationContext.CorrelationId;
            _logger.LogError(e, "Unexpected exception. CorrelationId: {correlationId}", correlationId);
            throw;
        }
    }
}