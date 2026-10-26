using CF.Customer.Application.Dtos;
using CF.Customer.Application.Facades.Interfaces;
using CF.Customer.Application.Mappers;
using CF.Customer.Domain.Services.Interfaces;

namespace CF.Customer.Application.Facades;

/// <summary>
///     The entry point into the customer core for any host (the API controllers, the console app). It speaks DTOs
///     to the outside and domain types to CustomerService, so hosts never handle the Customer entity directly. Most
///     methods are short on purpose: map the input, call the service, map the result.
/// </summary>
public class CustomerFacade(ICustomerService customerService) : ICustomerFacade
{
    public async Task<PaginationDto<CustomerResponseDto>> GetListByFilterAsync(CustomerFilterDto filterDto,
        CancellationToken cancellationToken)
    {
        var filter = CustomerMapper.MapToCustomerFilter(filterDto);

        var result = await customerService.GetListByFilterAsync(filter, cancellationToken);

        var paginationDto = CustomerMapper.MapToPaginationDto(result);

        return paginationDto;
    }

    public async Task<CustomerResponseDto?> GetByFilterAsync(CustomerFilterDto filterDto,
        CancellationToken cancellationToken)
    {
        var filter = CustomerMapper.MapToCustomerFilter(filterDto);

        var result = await customerService.GetByFilterAsync(filter, cancellationToken);

        return result is null ? null : CustomerMapper.MapToCustomerResponseDto(result);
    }

    public async Task UpdateAsync(long id, CustomerRequestDto customerRequestDto, bool verifyCurrentPassword,
        CancellationToken cancellationToken)
    {
        var customer = CustomerMapper.MapToCustomer(customerRequestDto);

        await customerService.UpdateAsync(id, customer, customerRequestDto.CurrentPassword, verifyCurrentPassword,
            cancellationToken);
    }

    public async Task<long> CreateAsync(CustomerRequestDto customerRequestDto, CancellationToken cancellationToken)
    {
        var customer = CustomerMapper.MapToCustomer(customerRequestDto);

        var id = await customerService.CreateAsync(customer, cancellationToken);

        return id;
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken)
    {
        await customerService.DeleteAsync(id, cancellationToken);
    }

    public async Task<AuthenticatedCustomerDto?> AuthenticateAsync(LoginRequestDto loginRequestDto,
        CancellationToken cancellationToken)
    {
        var customer = await customerService.AuthenticateAsync(loginRequestDto.Email, loginRequestDto.Password,
            cancellationToken);

        return customer is null ? null : CustomerMapper.MapToAuthenticatedCustomerDto(customer);
    }

    public async Task<string?> GetSecurityStampAsync(long id, CancellationToken cancellationToken)
    {
        return await customerService.GetSecurityStampAsync(id, cancellationToken);
    }
}
