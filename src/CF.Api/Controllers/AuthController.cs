using System.Net;
using Asp.Versioning;
using CF.Api.Authentication;
using CF.Api.Dtos;
using CF.Customer.Application.Dtos;
using CF.Customer.Application.Facades.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CF.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public class AuthController(
    ICustomerFacade customerFacade,
    ITokenService tokenService,
    IOptions<JwtOptions> jwtOptions) : ControllerBase
{
    [HttpPost("token")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenResponseDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public async Task<ActionResult<TokenResponseDto>> Token([FromBody] LoginRequestDto loginRequestDto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var customer = await customerFacade.AuthenticateAsync(loginRequestDto, cancellationToken);

        // Same response for unknown email and wrong password, so the endpoint can't be used to probe accounts.
        if (customer is null)
            return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Invalid email or password.");

        string[] roles = jwtOptions.Value.AdminCustomerIds.Contains(customer.Id) ? [Roles.Admin] : [];
        var token = tokenService.CreateToken(customer.Id, customer.Email, roles);

        return new TokenResponseDto { AccessToken = token.Token, ExpiresAt = token.ExpiresAt };
    }
}
