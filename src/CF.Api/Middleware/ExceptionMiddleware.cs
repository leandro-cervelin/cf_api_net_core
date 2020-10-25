using System;
using System.Threading.Tasks;
using CorrelationId.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CF.Api.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly ICorrelationContextAccessor _correlationContext;
        private readonly ILogger _logger;
        private readonly RequestDelegate _next;

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
                    "Exception Details: {message}, {innerException}, {stackTrace}, {fullException}. CorrelationId: {correlationId}",
                e.Message, e.InnerException, e.StackTrace, e.ToString(), correlationId);
                throw;
            }
        }
    }
}