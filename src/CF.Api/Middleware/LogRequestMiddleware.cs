using CorrelationId.Abstractions;
using Microsoft.AspNetCore.Http.Extensions;

namespace CF.Api.Middleware;

public class LogRequestMiddleware(RequestDelegate next, ILogger<LogRequestMiddleware> logger,
    ICorrelationContextAccessor correlationContext)
{
    private readonly ICorrelationContextAccessor _correlationContext = correlationContext;
    private readonly ILogger _logger = logger;
    private readonly RequestDelegate _next = next;

    public async Task Invoke(HttpContext context)
    {
        var url = context.Request.GetDisplayUrl();

        var correlationId = _correlationContext.CorrelationContext.CorrelationId;

        _logger.LogInformation(
            "Scheme: {scheme}, Host: {host}, Path: {path}, Method: {method}, url: {url}, correlationId: {correlationId}",
            context.Request.Scheme, context.Request.Host, context.Request.Path, context.Request.Method, url,
            correlationId);

        await _next(context);
    }
}