using CF.Customer.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CF.Api.Filters;

public class ExceptionFilter : IExceptionFilter
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

    private static void HandleEntityNotFoundException(ExceptionContext context)
    {
        context.ExceptionHandled = true;
        context.Result = new NotFoundResult();
    }

    private static void HandleValidationException(ExceptionContext context)
    {
        var error = new KeyValuePair<string, object>("Errors", new Dictionary<string, List<string>>
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

    private static void HandleException(ExceptionContext context)
    {
        context.ExceptionHandled = true;
        context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
    }
}