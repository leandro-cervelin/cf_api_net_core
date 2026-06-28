using System.Net;
using Asp.Versioning;
using CF.Api.Helpers;
using CF.Customer.Application.Dtos;
using CF.Customer.Application.Facades.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CF.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/customer")]
public class CustomerController(ICustomerFacade customerFacade) : ControllerBase
{
    [HttpGet]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByQueryKeys = ["*"])]
    [ProducesResponseType(typeof(PaginationDto<CustomerResponseDto>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<PaginationDto<CustomerResponseDto>>> Get(
        [FromQuery] CustomerFilterDto customerFilterDto, CancellationToken cancellationToken)
    {
        return await customerFacade.GetListByFilterAsync(customerFilterDto, cancellationToken);
    }

    [HttpGet("{id:long}")]
    [ResponseCache(Duration = 120, Location = ResponseCacheLocation.Any, VaryByQueryKeys = ["id"])]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(CustomerResponseDto), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<CustomerResponseDto>> Get(long id, CancellationToken cancellationToken)
    {
        if (id <= 0) return BadRequest(ControllerHelper.CreateProblemDetails("Id", "Invalid Id."));

        var filter = new CustomerFilterDto { Id = id };
        var result = await customerFacade.GetByFilterAsync(filter, cancellationToken);

        if (result == null) return NotFound();

        return result;
    }

    [HttpPost]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Created)]
    public async Task<IActionResult> Post([FromBody] CustomerRequestDto customerRequestDto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var id = await customerFacade.CreateAsync(customerRequestDto, cancellationToken);
        var version = HttpContext.Features.Get<IApiVersioningFeature>()?.RequestedApiVersion?.ToString();

        return CreatedAtAction(nameof(Get), new { id, version }, new { id });
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    public async Task<IActionResult> Put(long id, [FromBody] CustomerRequestDto customerRequestDto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (id <= 0) return BadRequest(ControllerHelper.CreateProblemDetails("Id", "Invalid Id."));

        await customerFacade.UpdateAsync(id, customerRequestDto, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:long}")]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        if (id <= 0) return BadRequest(ControllerHelper.CreateProblemDetails("Id", "Invalid Id."));

        await customerFacade.DeleteAsync(id, cancellationToken);

        return NoContent();
    }
}
