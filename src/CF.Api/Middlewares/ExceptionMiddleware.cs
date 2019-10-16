using System;
using System.Threading.Tasks;
using CorrelationId;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CF.Api.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly ICorrelationContextAccessor _correlationContext;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger,
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

                _logger.LogError(
                    $"Exception Details: {e.Message}, {e.InnerException}, {e.StackTrace}. CorrelationId: {correlationId}");
                throw;
            }
        }
    }
}

