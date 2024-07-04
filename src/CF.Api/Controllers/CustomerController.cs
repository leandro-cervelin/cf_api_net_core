using System.Net;
using CF.Api.Helpers;
using CF.Customer.Application.Dtos;
using CF.Customer.Application.Facades.Interfaces;
using CF.Customer.Domain.Exceptions;
using CorrelationId.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CF.Api.Controllers;

[ApiController]
[Route("api/v1/customer")]
public class CustomerController(
    ICorrelationContextAccessor correlationContext,
    ILogger<CustomerController> logger,
    ICustomerFacade customerFacade) : ControllerBase
{
    [HttpGet]
    [SwaggerResponse((int)HttpStatusCode.OK, "Customer successfully returned.",
        typeof(PaginationDto<CustomerResponseDto>))]
    public async Task<ActionResult<PaginationDto<CustomerResponseDto>>> Get(
        [FromQuery] CustomerFilterDto customerFilterDto, CancellationToken cancellationToken)
    {
        try
        {
            var result = await customerFacade.GetListByFilterAsync(customerFilterDto, cancellationToken);
            return result;
        }
        catch (ValidationException e)
        {
            logger.LogError(e, "Validation Exception Details. CorrelationId: {correlationId}",
                correlationContext.CorrelationContext.CorrelationId);

            return BadRequest(e.Message);
        }
    }

    [HttpGet("{id}")]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, "Invalid id.")]
    [SwaggerResponse((int)HttpStatusCode.NotFound, "Customer not found.")]
    [SwaggerResponse((int)HttpStatusCode.OK, "Customer successfully returned.")]
    public async Task<ActionResult<CustomerResponseDto>> Get(long id, CancellationToken cancellationToken)
    {
        try
        {
            if (id <= 0) return BadRequest(ControllerHelper.CreateProblemDetails("Id", "Invalid Id."));

            var filter = new CustomerFilterDto { Id = id };
            var result = await customerFacade.GetByFilterAsync(filter, cancellationToken);

            if (result == null) return NotFound();

            return result;
        }
        catch (ValidationException e)
        {
            logger.LogError(e, "Validation Exception. CorrelationId: {correlationId}",
                correlationContext.CorrelationContext.CorrelationId);

            return BadRequest(e.Message);
        }
    }

    [HttpPost]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, "Invalid Request.")]
    [SwaggerResponse((int)HttpStatusCode.Created, "Customer has been created successfully.")]
    public async Task<IActionResult> Post([FromBody] CustomerRequestDto customerRequestDto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var id = await customerFacade.CreateAsync(customerRequestDto, cancellationToken);

        return CreatedAtAction(nameof(Get), new { id }, new { id });
    }

    [HttpPut("{id}")]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, "Invalid id.")]
    [SwaggerResponse((int)HttpStatusCode.NotFound, "Customer not found")]
    [SwaggerResponse((int)HttpStatusCode.NoContent, "Customer has been updated successfully.")]
    public async Task<IActionResult> Put(long id, [FromBody] CustomerRequestDto customerRequestDto,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (id <= 0) return BadRequest(ControllerHelper.CreateProblemDetails("Id", "Invalid Id."));

            await customerFacade.UpdateAsync(id, customerRequestDto, cancellationToken);
            return NoContent();
        }
        catch (EntityNotFoundException e)
        {
            logger.LogError(e, "Entity Not Found Exception. CorrelationId: {correlationId}",
                correlationContext.CorrelationContext.CorrelationId);

            return NotFound();
        }
        catch (ValidationException e)
        {
            logger.LogError(e, "Validation Exception. CorrelationId: {correlationId}",
                correlationContext.CorrelationContext.CorrelationId);

            return BadRequest(e.Message);
        }
    }

    [HttpDelete("{id}")]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, "Invalid id.")]
    [SwaggerResponse((int)HttpStatusCode.NotFound, "Customer not found.")]
    [SwaggerResponse((int)HttpStatusCode.NoContent, "Customer has been deleted successfully.")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        try
        {
            if (id <= 0) return BadRequest(ControllerHelper.CreateProblemDetails("Id", "Invalid Id."));

            await customerFacade.DeleteAsync(id, cancellationToken);

            return NoContent();
        }
        catch (EntityNotFoundException e)
        {
            logger.LogError(e, "Entity Not Found Exception. CorrelationId: {correlationId}",
                correlationContext.CorrelationContext.CorrelationId);

            return NotFound();
        }
        catch (ValidationException e)
        {
            logger.LogError(e, "Validation Exception. CorrelationId: {correlationId}",
                correlationContext.CorrelationContext.CorrelationId);

            return BadRequest(e.Message);
        }
    }
}