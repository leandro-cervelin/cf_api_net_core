using CorrelationId.Abstractions;

namespace CF.Api.Middleware;

public class LogExceptionMiddleware
{
    private readonly ICorrelationContextAccessor _correlationContext;
    private readonly ILogger _logger;
    private readonly RequestDelegate _next;

    public LogExceptionMiddleware(RequestDelegate next, ILogger<LogExceptionMiddleware> logger,
        ICorrelationContextAccessor correlationContext)
    {
        _logger = logger;
        _next = next;
        _correlationContext = correlationContext;
    }

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