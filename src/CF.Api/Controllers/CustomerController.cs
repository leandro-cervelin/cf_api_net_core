using System.Net;
using Asp.Versioning;
using CF.Api.Helpers;
using CF.Customer.Application.Dtos;
using CF.Customer.Application.Facades.Interfaces;
using CF.Customer.Domain.Exceptions;
using CorrelationId.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace CF.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/customer")]
public class CustomerController(
    ICorrelationContextAccessor correlationContext,
    ILogger<CustomerController> logger,
    ICustomerFacade customerFacade) : ControllerBase
{
    [HttpGet]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByQueryKeys = ["*"])]
    [ProducesResponseType(typeof(PaginationDto<CustomerResponseDto>), (int)HttpStatusCode.OK)]
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
            if (logger.IsEnabled(LogLevel.Error))
                logger.LogError(e, "Validation Exception Details. CorrelationId: {correlationId}",
                    correlationContext.CorrelationContext.CorrelationId);

            return BadRequest(e.Message);
        }
    }

    [HttpGet("{id:long}")]
    [ResponseCache(Duration = 120, Location = ResponseCacheLocation.Any, VaryByQueryKeys = ["id"])]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(CustomerResponseDto), (int)HttpStatusCode.OK)]
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
            if (logger.IsEnabled(LogLevel.Error))
                logger.LogError(e, "Validation Exception. CorrelationId: {correlationId}",
                    correlationContext.CorrelationContext.CorrelationId);

            return BadRequest(e.Message);
        }
    }

    [HttpPost]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Created)]
    public async Task<IActionResult> Post([FromBody] CustomerRequestDto customerRequestDto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var id = await customerFacade.CreateAsync(customerRequestDto, cancellationToken);

        return CreatedAtAction(nameof(Get), new { id, version = HttpContext.GetRequestedApiVersion()?.ToString() },
            new { id });
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
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
            if (logger.IsEnabled(LogLevel.Error))
                logger.LogError(e, "Entity Not Found Exception. CorrelationId: {correlationId}",
                    correlationContext.CorrelationContext.CorrelationId);

            return NotFound();
        }
        catch (ValidationException e)
        {
            if (logger.IsEnabled(LogLevel.Error))
                logger.LogError(e, "Validation Exception. CorrelationId: {correlationId}",
                    correlationContext.CorrelationContext.CorrelationId);

            return BadRequest(e.Message);
        }
    }

    [HttpDelete("{id:long}")]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
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
            if (logger.IsEnabled(LogLevel.Error))
                logger.LogError(e, "Entity Not Found Exception. CorrelationId: {correlationId}",
                    correlationContext.CorrelationContext.CorrelationId);

            return NotFound();
        }
        catch (ValidationException e)
        {
            if (logger.IsEnabled(LogLevel.Error))
                logger.LogError(e, "Validation Exception. CorrelationId: {correlationId}",
                    correlationContext.CorrelationContext.CorrelationId);

            return BadRequest(e.Message);
        }
    }
}