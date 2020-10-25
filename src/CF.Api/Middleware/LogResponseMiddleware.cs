using System.Threading.Tasks;
using CorrelationId.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CF.Api.Middleware
{
    public class LogResponseMiddleware
    {
        private readonly ICorrelationContextAccessor _correlationContext;
        private readonly ILogger _logger;
        private readonly RequestDelegate _next;

        public LogResponseMiddleware(RequestDelegate next, ILogger<LogResponseMiddleware> logger,
            ICorrelationContextAccessor correlationContext)
        {
            _next = next;
            _logger = logger;
            _correlationContext = correlationContext;
        }

        public async Task Invoke(HttpContext context)
        {
            var correlationId = _correlationContext.CorrelationContext.CorrelationId;

            _logger.LogInformation("StatusCode: {statusCode}. (CorrelationId: {correlationId})",
                context?.Response?.StatusCode, correlationId);

            await _next(context);
        }
    }
}