using CorrelationId.Abstractions;

namespace CF.Api.Middleware;

public class LogRequestMiddleware(
    RequestDelegate next,
    ILogger<LogRequestMiddleware> logger,
    ICorrelationContextAccessor correlationContext)
{
    public async Task Invoke(HttpContext context)
    {
        var correlationId = correlationContext.CorrelationContext?.CorrelationId;

        // The query string is deliberately not logged: filters such as ?email= carry personal data.
        logger.LogInformation(
            "Scheme: {Scheme}, Host: {Host}, Path: {Path}, Method: {Method}, CorrelationId: {CorrelationId}",
            context.Request.Scheme, context.Request.Host, context.Request.Path, context.Request.Method,
            correlationId);

        await next(context);
    }
}