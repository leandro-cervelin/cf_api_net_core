using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Asp.Versioning;
using CF.Api.Authentication;
using CF.Api.Controllers;
using CF.Customer.Application.Dtos;
using CF.Customer.Application.Facades.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Moq;
using Xunit;

namespace CF.Api.UnitTest;

public class CustomerControllerTest
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly Mock<ICustomerFacade> _customerFacade = new();
    private readonly Mock<ISecurityStampValidator> _securityStampValidator = new();

    [Fact]
    public async Task GetListTestAsync()
    {
        //Arrange
        var facadeResult = new PaginationDto<CustomerResponseDto>
        {
            Count = 2,
            Result =
            [
                new CustomerResponseDto
                {
                    Email = "tarnished@test.com",
                    FirstName = "Elden",
                    Surname = "Ring",
                    FullName = "Elden Ring",
                    Id = 1
                },
                new CustomerResponseDto
                {
                    Email = "nameless_king@test.com",
                    FirstName = "Elden",
                    Surname = "King",
                    FullName = "Elden King",
                    Id = 2
                }
            ]
        };

        _customerFacade
            .Setup(x => x.GetListByFilterAsync(It.IsAny<CustomerFilterDto>(), _cancellationTokenSource.Token))
            .ReturnsAsync(facadeResult);

        var controller = new CustomerController(_customerFacade.Object, _securityStampValidator.Object);

        var requestDto = new CustomerFilterDto
        {
            FirstName = "Elden"
        };

        //Act
        var actionResult = await controller.Get(requestDto, _cancellationTokenSource.Token);

        //Assert
        Assert.NotNull(actionResult);
        Assert.Equal(2, actionResult.Value?.Count);
        Assert.Equal(2, actionResult.Value?.Result.Count(x => x.FirstName == "Elden"));
    }

    [Fact]
    public async Task GetByIdTestAsync()
    {
        //Arrange
        var facadeResult = new CustomerResponseDto
        {
            Email = "tarnished@test.com",
            FirstName = "Elden",
            Surname = "Ring",
            FullName = "Elden Ring",
            Id = 1
        };

        _customerFacade.Setup(x => x.GetByFilterAsync(It.IsAny<CustomerFilterDto>(), _cancellationTokenSource.Token))
            .ReturnsAsync(facadeResult);

        var controller = CreateController(customerId: 1);

        //Act
        var actionResult = await controller.Get(1, _cancellationTokenSource.Token);

        //Assert
        Assert.NotNull(actionResult);
        Assert.Equal(1, actionResult.Value?.Id);
        Assert.Equal("tarnished@test.com", actionResult.Value?.Email);
    }

    [Fact]
    public async Task PostTestAsync()
    {
        //Arrange
        _customerFacade.Setup(x => x.CreateAsync(It.IsAny<CustomerRequestDto>(), _cancellationTokenSource.Token))
            .ReturnsAsync(1);

        var controller = new CustomerController(_customerFacade.Object, _securityStampValidator.Object);

        // Mock HttpContext for API versioning
        var httpContext = new DefaultHttpContext();
        var apiVersion = new ApiVersion(1, 0);
        httpContext.Features.Set<IApiVersioningFeature>(new ApiVersioningFeature(httpContext)
        {
            RequestedApiVersion = apiVersion
        });

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var requestDto = new CustomerRequestDto
        {
            ConfirmPassword = "123DarkSouls!",
            Password = "123DarkSouls!",
            Email = "chosen_one@test.com",
            FirstName = "Dark",
            Surname = "Souls"
        };

        //Act
        var actionResult = await controller.Post(requestDto, _cancellationTokenSource.Token);

        //Assert
        Assert.NotNull(actionResult);
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult);
        Assert.Equal(nameof(CustomerController.Get), createdAtActionResult.ActionName);
        Assert.NotNull(createdAtActionResult.RouteValues);
        Assert.True(createdAtActionResult.RouteValues.ContainsKey("id"));
        Assert.Equal(1L, createdAtActionResult.RouteValues["id"]);
        Assert.True(createdAtActionResult.RouteValues.ContainsKey("version"));
        Assert.Equal("1.0", createdAtActionResult.RouteValues["version"]);
    }

    [Theory]
    [InlineData(1, false, true)] // customer editing themselves
    [InlineData(1, true, true)] // admin editing their own account
    [InlineData(2, true, false)] // admin editing someone else: can't know their password
    public async Task Put_RequiresCurrentPasswordUnlessAdminEditsAnotherCustomer(long callerId, bool isAdmin,
        bool expectVerification)
    {
        //Arrange
        var controller = CreateController(customerId: callerId, isAdmin: isAdmin);
        var requestDto = new CustomerRequestDto
        {
            ConfirmPassword = "123DarkSouls!",
            Password = "123DarkSouls!",
            Email = "chosen_one@test.com",
            FirstName = "Dark",
            Surname = "Souls"
        };

        //Act
        await controller.Put(1, requestDto, _cancellationTokenSource.Token);

        //Assert
        _customerFacade.Verify(x => x.UpdateAsync(1, requestDto, expectVerification, _cancellationTokenSource.Token),
            Times.Once);
    }

    [Fact]
    public async Task PutTestAsync()
    {
        //Arrange
        _customerFacade.Setup(x =>
            x.UpdateAsync(It.IsAny<long>(), It.IsAny<CustomerRequestDto>(), It.IsAny<bool>(), _cancellationTokenSource.Token));

        var controller = CreateController(customerId: 1);

        var requestDto = new CustomerRequestDto
        {
            ConfirmPassword = "123DarkSouls!",
            Password = "123DarkSouls!",
            Email = "chosen_one@test.com",
            FirstName = "Dark",
            Surname = "Souls"
        };

        //Act
        var actionResult = await controller.Put(1, requestDto, _cancellationTokenSource.Token);

        //Assert
        Assert.NotNull(actionResult);
        Assert.IsType<NoContentResult>(actionResult);
        _securityStampValidator.Verify(x => x.Invalidate(1), Times.Once);
    }

    [Fact]
    public async Task DeleteTestAsync()
    {
        //Arrange
        _customerFacade.Setup(x => x.DeleteAsync(It.IsAny<long>(), _cancellationTokenSource.Token));

        var controller = CreateController(customerId: 1);

        //Act
        var actionResult = await controller.Delete(1, _cancellationTokenSource.Token);

        //Assert
        Assert.NotNull(actionResult);
        Assert.IsType<NoContentResult>(actionResult);
    }

    [Fact]
    public async Task GetById_OtherCustomer_ReturnsForbid()
    {
        //Arrange
        var controller = CreateController(customerId: 2);

        //Act
        var actionResult = await controller.Get(1, _cancellationTokenSource.Token);

        //Assert
        Assert.IsType<ForbidResult>(actionResult.Result);
        _customerFacade.Verify(
            x => x.GetByFilterAsync(It.IsAny<CustomerFilterDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Put_OtherCustomer_ReturnsForbid()
    {
        //Arrange
        var controller = CreateController(customerId: 2);
        var requestDto = new CustomerRequestDto
        {
            ConfirmPassword = "123DarkSouls!",
            Password = "123DarkSouls!",
            Email = "chosen_one@test.com",
            FirstName = "Dark",
            Surname = "Souls"
        };

        //Act
        var actionResult = await controller.Put(1, requestDto, _cancellationTokenSource.Token);

        //Assert
        Assert.IsType<ForbidResult>(actionResult);
        _customerFacade.Verify(
            x => x.UpdateAsync(It.IsAny<long>(), It.IsAny<CustomerRequestDto>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Delete_OtherCustomer_ReturnsForbid()
    {
        //Arrange
        var controller = CreateController(customerId: 2);

        //Act
        var actionResult = await controller.Delete(1, _cancellationTokenSource.Token);

        //Assert
        Assert.IsType<ForbidResult>(actionResult);
        _customerFacade.Verify(x => x.DeleteAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Delete_Admin_CanDeleteOtherCustomer()
    {
        //Arrange
        var controller = CreateController(customerId: 2, isAdmin: true);

        //Act
        var actionResult = await controller.Delete(1, _cancellationTokenSource.Token);

        //Assert
        Assert.IsType<NoContentResult>(actionResult);
        _customerFacade.Verify(x => x.DeleteAsync(1, _cancellationTokenSource.Token), Times.Once);
        _securityStampValidator.Verify(x => x.Invalidate(1), Times.Once);
    }

    private CustomerController CreateController(long customerId, bool isAdmin = false)
    {
        List<Claim> claims = [new(JwtRegisteredClaimNames.Sub, customerId.ToString())];
        if (isAdmin) claims.Add(new Claim(AuthenticationExtensions.RoleClaimType, Roles.Admin));

        var identity = new ClaimsIdentity(claims, "Test", JwtRegisteredClaimNames.Sub,
            AuthenticationExtensions.RoleClaimType);

        return new CustomerController(_customerFacade.Object, _securityStampValidator.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            }
        };
    }
}