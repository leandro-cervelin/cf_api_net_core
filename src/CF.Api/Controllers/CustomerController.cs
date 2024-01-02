using System.Net;
using CF.Customer.Application.Dtos;
using CF.Customer.Application.Facades.Interfaces;
using CF.Customer.Domain.Exceptions;
using CorrelationId.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CF.Api.Controllers;

[ApiController]
[Route("api/v1/customer")]
public class CustomerController(ICorrelationContextAccessor correlationContext, ILogger<CustomerController> logger,
    ICustomerFacade customerFacade) : ControllerBase
{
    private readonly ICorrelationContextAccessor _correlationContext = correlationContext;
    private readonly ICustomerFacade _customerFacade = customerFacade;
    private readonly ILogger _logger = logger;

    [HttpGet]
    [SwaggerResponse((int) HttpStatusCode.OK, "Customer successfully returned.",
        typeof(PaginationDto<CustomerResponseDto>))]
    public async Task<ActionResult<PaginationDto<CustomerResponseDto>>> Get(
        [FromQuery] CustomerFilterDto customerFilterDto, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _customerFacade.GetListByFilterAsync(customerFilterDto, cancellationToken);
            return result;
        }
        catch (ValidationException e)
        {
            _logger.LogError(e, "Validation Exception Details. CorrelationId: {correlationId}",
                _correlationContext.CorrelationContext.CorrelationId);

            return BadRequest(e.Message);
        }
    }

    [HttpGet("{id}")]
    [SwaggerResponse((int) HttpStatusCode.BadRequest, "Invalid id.")]
    [SwaggerResponse((int) HttpStatusCode.NotFound, "Customer not found.")]
    [SwaggerResponse((int) HttpStatusCode.OK, "Customer successfully returned.")]
    public async Task<ActionResult<CustomerResponseDto>> Get(long id, CancellationToken cancellationToken)
    {
        try
        {
            if (id <= 0) return BadRequest(CreateProblemDetails("Id", "Invalid Id."));

            var filter = new CustomerFilterDto {Id = id};
            var result = await _customerFacade.GetByFilterAsync(filter, cancellationToken);

            if (result == null) return NotFound();

            return result;
        }
        catch (ValidationException e)
        {
            _logger.LogError(e, "Validation Exception. CorrelationId: {correlationId}",
                _correlationContext.CorrelationContext.CorrelationId);

            return BadRequest(e.Message);
        }
    }

    [HttpPost]
    [SwaggerResponse((int) HttpStatusCode.BadRequest, "Invalid Request.")]
    [SwaggerResponse((int) HttpStatusCode.Created, "Customer has been created successfully.")]
    public async Task<IActionResult> Post([FromBody] CustomerRequestDto customerRequestDto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var id = await _customerFacade.CreateAsync(customerRequestDto, cancellationToken);

        return CreatedAtAction(nameof(Get), new {id}, new {id});
    }

    [HttpPut("{id}")]
    [SwaggerResponse((int) HttpStatusCode.BadRequest, "Invalid id.")]
    [SwaggerResponse((int) HttpStatusCode.NotFound, "Customer not found")]
    [SwaggerResponse((int) HttpStatusCode.NoContent, "Customer has been updated successfully.")]
    public async Task<IActionResult> Put(long id, [FromBody] CustomerRequestDto customerRequestDto, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (id <= 0) return BadRequest(CreateProblemDetails("Id", "Invalid Id."));

            await _customerFacade.UpdateAsync(id, customerRequestDto, cancellationToken);
            return NoContent();
        }
        catch (EntityNotFoundException e)
        {
            _logger.LogError(e, "Entity Not Found Exception. CorrelationId: {correlationId}",
                _correlationContext.CorrelationContext.CorrelationId);

            return NotFound();
        }
        catch (ValidationException e)
        {
            _logger.LogError(e, "Validation Exception. CorrelationId: {correlationId}",
                _correlationContext.CorrelationContext.CorrelationId);

            return BadRequest(e.Message);
        }
    }

    [HttpDelete("{id}")]
    [SwaggerResponse((int) HttpStatusCode.BadRequest, "Invalid id.")]
    [SwaggerResponse((int) HttpStatusCode.NotFound, "Customer not found.")]
    [SwaggerResponse((int) HttpStatusCode.NoContent, "Customer has been deleted successfully.")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        try
        {
            if (id <= 0) return BadRequest(CreateProblemDetails("Id", "Invalid Id."));

            await _customerFacade.DeleteAsync(id, cancellationToken);

            return NoContent();
        }
        catch (EntityNotFoundException e)
        {
            _logger.LogError(e, "Entity Not Found Exception. CorrelationId: {correlationId}",
                _correlationContext.CorrelationContext.CorrelationId);

            return NotFound();
        }
        catch (ValidationException e)
        {
            _logger.LogError(e, "Validation Exception. CorrelationId: {correlationId}",
                _correlationContext.CorrelationContext.CorrelationId);

            return BadRequest(e.Message);
        }
    }

    private static ProblemDetails CreateProblemDetails(string property, string errorMessage)
    {
        var error = new KeyValuePair<string, object>("Errors", new Dictionary<string, List<string>>
            {
                {property, new List<string> {errorMessage}}
            }
        );

        return new ProblemDetails
        {
            Extensions = {error},
            Title = "One validation error occurred.",
            Status = StatusCodes.Status400BadRequest,
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1"
        };
    }
}