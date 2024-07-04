using CorrelationId.Abstractions;
using Microsoft.AspNetCore.Http.Extensions;

namespace CF.Api.Middleware;

public class LogRequestMiddleware(
    RequestDelegate next,
    ILogger<LogRequestMiddleware> logger,
    ICorrelationContextAccessor correlationContext)
{
    public async Task Invoke(HttpContext context)
    {
        var url = context.Request.GetDisplayUrl();

        var correlationId = correlationContext.CorrelationContext.CorrelationId;

        logger.LogInformation(
            "Scheme: {scheme}, Host: {host}, Path: {path}, Method: {method}, url: {url}, correlationId: {correlationId}",
            context.Request.Scheme, context.Request.Host, context.Request.Path, context.Request.Method, url,
            correlationId);

        await next(context);
    }
}