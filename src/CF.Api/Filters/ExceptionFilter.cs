using CF.Customer.Domain.Exceptions;
using CorrelationId.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CF.Api.Filters;

public class ExceptionFilter(
    ILogger<ExceptionFilter> logger,
    ICorrelationContextAccessor correlationContext) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        switch (context.Exception)
        {
            case ValidationException:
                HandleValidationException(context);
                break;
            case EntityNotFoundException:
                HandleEntityNotFoundException(context);
                break;
            default:
                HandleException(context);
                break;
        }
    }

    private void HandleEntityNotFoundException(ExceptionContext context)
    {
        Log(context.Exception, "Entity Not Found Exception.");
        context.ExceptionHandled = true;
        context.Result = new NotFoundResult();
    }

    private void HandleValidationException(ExceptionContext context)
    {
        Log(context.Exception, "Validation Exception.");

        var error = new KeyValuePair<string, object?>("Errors", new Dictionary<string, List<string>>
            {
                { "Validation", [context.Exception.Message] }
            }
        );

        var details = new ProblemDetails
        {
            Extensions = { error },
            Title = "One validation error occurred.",
            Status = StatusCodes.Status400BadRequest,
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1"
        };
        context.ExceptionHandled = true;
        context.Result = new BadRequestObjectResult(details);
    }

    private void HandleException(ExceptionContext context)
    {
        Log(context.Exception, "Unexpected exception.");
        context.ExceptionHandled = true;
        context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
    }

    private void Log(Exception exception, string message)
    {
        if (logger.IsEnabled(LogLevel.Error))
            logger.LogError(exception, "{Message} CorrelationId: {CorrelationId}", message,
                correlationContext.CorrelationContext?.CorrelationId);
    }
}
