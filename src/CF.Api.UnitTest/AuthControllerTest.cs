using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CF.Api.Authentication;
using CF.Api.Controllers;
using CF.Customer.Application.Dtos;
using CF.Customer.Application.Facades.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit;

namespace CF.Api.UnitTest;

public class AuthControllerTest
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly Mock<ICustomerFacade> _customerFacade = new();

    private readonly JwtOptions _jwtOptions = new()
    {
        Issuer = "test-issuer",
        Audience = "test-audience",
        SigningKey = "unit-test-signing-key-that-is-at-least-32-bytes",
        ExpiryMinutes = 15,
        AdminCustomerIds = [42]
    };

    [Fact]
    public async Task Token_ValidCredentials_ReturnsVerifiableTokenForCustomer()
    {
        //Arrange
        var login = new LoginRequestDto { Email = "tarnished@test.com", Password = "123DarkSouls!" };
        _customerFacade.Setup(x => x.AuthenticateAsync(login, _cancellationTokenSource.Token))
            .ReturnsAsync(CreateCustomer(7, login.Email));

        var controller = CreateController();

        //Act
        var actionResult = await controller.Token(login, _cancellationTokenSource.Token);

        //Assert
        Assert.NotNull(actionResult.Value);
        Assert.Equal("Bearer", actionResult.Value.TokenType);

        var validation = await ValidateAsync(actionResult.Value.AccessToken);
        Assert.True(validation.IsValid);
        Assert.Equal("7", validation.Claims[JwtRegisteredClaimNames.Sub]);
        Assert.Equal(login.Email, validation.Claims[JwtRegisteredClaimNames.Email]);
        Assert.False(validation.Claims.ContainsKey(AuthenticationExtensions.RoleClaimType));
    }

    [Fact]
    public async Task Token_AdminCustomer_IncludesAdminRole()
    {
        //Arrange
        var login = new LoginRequestDto { Email = "admin@test.com", Password = "123DarkSouls!" };
        _customerFacade.Setup(x => x.AuthenticateAsync(login, _cancellationTokenSource.Token))
            .ReturnsAsync(CreateCustomer(42, login.Email));

        var controller = CreateController();

        //Act
        var actionResult = await controller.Token(login, _cancellationTokenSource.Token);

        //Assert
        var validation = await ValidateAsync(actionResult.Value!.AccessToken);
        Assert.True(validation.IsValid);
        Assert.Equal(Roles.Admin, validation.Claims[AuthenticationExtensions.RoleClaimType]);
    }

    [Fact]
    public async Task Token_InvalidCredentials_ReturnsUnauthorized()
    {
        //Arrange
        var login = new LoginRequestDto { Email = "tarnished@test.com", Password = "wrong" };
        var controller = CreateController();

        //Act
        var actionResult = await controller.Token(login, _cancellationTokenSource.Token);

        //Assert
        var result = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(401, result.StatusCode);
    }

    [Fact]
    public async Task Token_SignedWithDifferentKey_IsRejected()
    {
        //Arrange
        var otherOptions = new JwtOptions
        {
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience,
            SigningKey = "a-completely-different-key-also-32-bytes-long"
        };
        var forged = new TokenService(Options.Create(otherOptions)).CreateToken(1, "x@test.com", [Roles.Admin]);

        //Act
        var validation = await ValidateAsync(forged.Token);

        //Assert
        Assert.False(validation.IsValid);
    }

    private AuthController CreateController()
    {
        var options = Options.Create(_jwtOptions);
        return new AuthController(_customerFacade.Object, new TokenService(options), options);
    }

    private Task<TokenValidationResult> ValidateAsync(string token)
    {
        return new JsonWebTokenHandler().ValidateTokenAsync(token, new TokenValidationParameters
        {
            ValidIssuer = _jwtOptions.Issuer,
            ValidAudience = _jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey))
        });
    }

    private static CustomerResponseDto CreateCustomer(long id, string email)
    {
        return new CustomerResponseDto
        {
            Id = id, Email = email, FirstName = "Elden", Surname = "Ring", FullName = "Elden Ring"
        };
    }
}
