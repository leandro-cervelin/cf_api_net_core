using System.Threading.Tasks;
using CorrelationId.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CF.Api.Middlewares
{
    public class LogResponseMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly ICorrelationContextAccessor _correlationContext;

        public LogResponseMiddleware(RequestDelegate next, ILogger<LogResponseMiddleware> logger, ICorrelationContextAccessor correlationContext)
        {
            _next = next;
            _logger = logger;
            _correlationContext = correlationContext;
        }

        public async Task Invoke(HttpContext context)
        {
            var correlationId = _correlationContext.CorrelationContext.CorrelationId;

            _logger.LogInformation($"StatusCode: {context.Response.StatusCode}. (CorrelationId: {correlationId})");
            
            await _next(context);
        }
    }
}
