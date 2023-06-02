using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CF.Api.Controllers;
using CF.Customer.Application.Dtos;
using CF.Customer.Application.Facades.Interfaces;
using CorrelationId;
using CorrelationId.Abstractions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CF.Api.UnitTest;

public class CustomerControllerTest
{
    private readonly Mock<ICorrelationContextAccessor> _correlationContext = new();
    private readonly Mock<ICustomerFacade> _customerFacade = new();
    private readonly Mock<ILogger<CustomerController>> _logger = new();

    public CustomerControllerTest()
    {
        _correlationContext.Setup(x => x.CorrelationContext)
            .Returns(new CorrelationContext(Guid.NewGuid().ToString(), "HeaderValue"));
    }

    [Fact]
    public async Task GetListTest()
    {
        var facadeResult = new PaginationDto<CustomerResponseDto>
        {
            Count = 2,
            Result = new List<CustomerResponseDto>
            {
                new()
                {
                    Email = "tarnished@test.com",
                    FirstName = "Elden",
                    Surname = "Ring",
                    FullName = "Elden Ring",
                    Id = 1
                },
                new()
                {
                    Email = "nameless_king@test.com",
                    FirstName = "Elden",
                    Surname = "King",
                    FullName = "Elden King",
                    Id = 2
                }
            }
        };

        var cancellationTokenSource = new CancellationTokenSource();

        _customerFacade.Setup(x => x.GetListByFilterAsync(It.IsAny<CustomerFilterDto>(), cancellationTokenSource.Token)).ReturnsAsync(facadeResult);

        var controller = new CustomerController(_correlationContext.Object, _logger.Object, _customerFacade.Object);

        var requestDto = new CustomerFilterDto
        {
            FirstName = "Elden"
        };

        var actionResult = await controller.Get(requestDto, cancellationTokenSource.Token);

        Assert.NotNull(actionResult);
        Assert.Equal(2, actionResult?.Value?.Count);
        Assert.Equal(2, actionResult?.Value?.Result.Count(x => x.FirstName == "Elden"));
    }

    [Fact]
    public async Task GetByIdTest()
    {
        var facadeResult = new CustomerResponseDto
        {
            Email = "tarnished@test.com",
            FirstName = "Elden",
            Surname = "Ring",
            FullName = "Elden Ring",
            Id = 1
        };

        var cancellationTokenSource = new CancellationTokenSource();

        _customerFacade.Setup(x => x.GetByFilterAsync(It.IsAny<CustomerFilterDto>(), cancellationTokenSource.Token)).ReturnsAsync(facadeResult);

        var controller = new CustomerController(_correlationContext.Object, _logger.Object, _customerFacade.Object);

        var actionResult = await controller.Get(1, cancellationTokenSource.Token);

        Assert.NotNull(actionResult);
        Assert.Equal(1, actionResult?.Value?.Id);
        Assert.Equal("tarnished@test.com", actionResult?.Value?.Email);
    }

    [Fact]
    public async Task PostTest()
    {
        var cancellationTokenSource = new CancellationTokenSource();

        _customerFacade.Setup(x => x.CreateAsync(It.IsAny<CustomerRequestDto>(), cancellationTokenSource.Token)).ReturnsAsync(1);

        var controller = new CustomerController(_correlationContext.Object, _logger.Object, _customerFacade.Object);

        var requestDto = new CustomerRequestDto
        {
            ConfirmPassword = "123DarkSouls!",
            Password = "123DarkSouls!",
            Email = "chosen_one@test.com",
            FirstName = "Dark",
            Surname = "Souls"
        };

        var actionResult = await controller.Post(requestDto, cancellationTokenSource.Token);

        Assert.NotNull(actionResult);
    }

    [Fact]
    public async Task PutTest()
    {
        var cancellationTokenSource = new CancellationTokenSource();

        _customerFacade.Setup(x => x.UpdateAsync(It.IsAny<long>(), It.IsAny<CustomerRequestDto>(), cancellationTokenSource.Token));

        var controller = new CustomerController(_correlationContext.Object, _logger.Object, _customerFacade.Object);

        var requestDto = new CustomerRequestDto
        {
            ConfirmPassword = "123DarkSouls!",
            Password = "123DarkSouls!",
            Email = "chosen_one@test.com",
            FirstName = "Dark",
            Surname = "Souls"
        };

        var actionResult = await controller.Put(1, requestDto, cancellationTokenSource.Token);

        Assert.NotNull(actionResult);
    }

    [Fact]
    public async Task DeleteTest()
    {
        var cancellationTokenSource = new CancellationTokenSource();

        _customerFacade.Setup(x => x.DeleteAsync(It.IsAny<long>(), cancellationTokenSource.Token));

        var controller = new CustomerController(_correlationContext.Object, _logger.Object, _customerFacade.Object);

        var actionResult = await controller.Delete(1, cancellationTokenSource.Token);

        Assert.NotNull(actionResult);
    }
}