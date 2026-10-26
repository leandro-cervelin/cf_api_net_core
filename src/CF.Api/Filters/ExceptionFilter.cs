using CF.Customer.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CF.Api.Filters;

/// <summary>
///     The one place that turns exceptions thrown by controllers (and the layers below them) into HTTP responses:
///     ValidationException → 400, EntityNotFoundException → 404, a unique-index violation → 409, anything else → 500.
///     The first three are the client's mistake, so they're logged as one-line warnings; only the last is logged as an
///     error with its stack trace.
/// </summary>
public class ExceptionFilter(ILogger<ExceptionFilter> logger) : IExceptionFilter
{
    // SQL Server: 2601 = duplicate key in unique index, 2627 = unique/primary key constraint violation.
    private static readonly int[] UniqueViolationErrorNumbers = [2601, 2627];

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
            case DbUpdateException { InnerException: SqlException sqlException }
                when UniqueViolationErrorNumbers.Contains(sqlException.Number):
                HandleUniqueViolationException(context);
                break;
            default:
                HandleException(context);
                break;
        }
    }

    private void HandleEntityNotFoundException(ExceptionContext context)
    {
        logger.LogWarning("Entity not found: {Message}", context.Exception.Message);
        context.ExceptionHandled = true;
        context.Result = new NotFoundResult();
    }

    private void HandleValidationException(ExceptionContext context)
    {
        logger.LogWarning("Validation failed: {Message}", context.Exception.Message);

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

    // A concurrent request won the race past the service-level uniqueness check (e.g. two sign-ups
    // with the same email); the unique index rejected the second write.
    private void HandleUniqueViolationException(ExceptionContext context)
    {
        logger.LogWarning("Unique constraint violation.");

        var details = new ProblemDetails
        {
            Title = "The resource conflicts with an existing one.",
            Detail = "A record with the same unique value already exists.",
            Status = StatusCodes.Status409Conflict,
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.8"
        };
        context.ExceptionHandled = true;
        context.Result = new ConflictObjectResult(details);
    }

    private void HandleException(ExceptionContext context)
    {
        // A real bug: log the whole exception, stack trace included, so it can be fixed.
        logger.LogError(context.Exception, "Unexpected exception.");
        context.ExceptionHandled = true;
        context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
    }
}
