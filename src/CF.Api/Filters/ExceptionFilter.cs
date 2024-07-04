using CF.Customer.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CF.Api.Filters;

public class ExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        context.ExceptionHandled = context.Exception switch
        {
            ValidationException => HandleValidationException(context),
            EntityNotFoundException => HandleEntityNotFoundException(context),
            _ => HandleException(context)
        };
    }

    private static bool HandleEntityNotFoundException(ExceptionContext context)
    {
        context.ExceptionHandled = true;
        context.Result = new NotFoundResult();
        return true;
    }

    private static bool HandleValidationException(ExceptionContext context)
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
        return true;
    }

    private static bool HandleException(ExceptionContext context)
    {
        context.ExceptionHandled = true;
        context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
        return true;
    }
}