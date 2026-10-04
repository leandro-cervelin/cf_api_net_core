using System.Globalization;
using System.Net;
using Asp.Versioning;
using CF.Api.Authentication;
using CF.Api.Helpers;
using CF.Customer.Application.Dtos;
using CF.Customer.Application.Facades.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace CF.Api.Controllers;

[ApiController]
[Authorize]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/customer")]
[ProducesResponseType((int)HttpStatusCode.Unauthorized)]
public class CustomerController(ICustomerFacade customerFacade, ISecurityStampValidator securityStampValidator)
    : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType((int)HttpStatusCode.Forbidden)]
    [ProducesResponseType(typeof(PaginationDto<CustomerResponseDto>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<PaginationDto<CustomerResponseDto>>> Get(
        [FromQuery] CustomerFilterDto customerFilterDto, CancellationToken cancellationToken)
    {
        return await customerFacade.GetListByFilterAsync(customerFilterDto, cancellationToken);
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Forbidden)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(CustomerResponseDto), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<CustomerResponseDto>> Get(long id, CancellationToken cancellationToken)
    {
        if (id <= 0) return BadRequest(ControllerHelper.CreateProblemDetails("Id", "Invalid Id."));

        if (!IsOwnerOrAdmin(id)) return Forbid();

        var filter = new CustomerFilterDto { Id = id };
        var result = await customerFacade.GetByFilterAsync(filter, cancellationToken);

        if (result == null) return NotFound();

        return result;
    }

    // Sign-up: anyone may create an account.
    [HttpPost]
    [AllowAnonymous]
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
    [ProducesResponseType((int)HttpStatusCode.Forbidden)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    public async Task<IActionResult> Put(long id, [FromBody] CustomerRequestDto customerRequestDto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (id <= 0) return BadRequest(ControllerHelper.CreateProblemDetails("Id", "Invalid Id."));

        if (!IsOwnerOrAdmin(id)) return Forbid();

        // Customers must prove they know their current password to change it. Admins managing someone else's
        // account can't know it, so they're exempt (but an admin editing their own account is not).
        var verifyCurrentPassword = IsOwner(id) || !User.IsInRole(Roles.Admin);

        await customerFacade.UpdateAsync(id, customerRequestDto, verifyCurrentPassword, cancellationToken);

        // The update may have rotated the security stamp; drop the cached one so old tokens stop working now.
        securityStampValidator.Invalidate(id);

        return NoContent();
    }

    [HttpDelete("{id:long}")]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Forbidden)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        if (id <= 0) return BadRequest(ControllerHelper.CreateProblemDetails("Id", "Invalid Id."));

        if (!IsOwnerOrAdmin(id)) return Forbid();

        await customerFacade.DeleteAsync(id, cancellationToken);
        securityStampValidator.Invalidate(id);

        return NoContent();
    }

    private bool IsOwnerOrAdmin(long id)
    {
        return User.IsInRole(Roles.Admin) || IsOwner(id);
    }

    private bool IsOwner(long id)
    {
        return User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value == id.ToString(CultureInfo.InvariantCulture);
    }
}
