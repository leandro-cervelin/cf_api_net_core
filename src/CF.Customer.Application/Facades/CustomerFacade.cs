using CF.Customer.Application.Dtos;
using CF.Customer.Application.Facades.Interfaces;
using CF.Customer.Application.Mappers;
using CF.Customer.Domain.Services.Interfaces;

namespace CF.Customer.Application.Facades;

public class CustomerFacade(ICustomerService customerService, ICustomerMapper mapper) : ICustomerFacade
{
    public async Task<PaginationDto<CustomerResponseDto>> GetListByFilterAsync(CustomerFilterDto filterDto,
        CancellationToken cancellationToken)
    {
        var filter = mapper.MapToCustomerFilter(filterDto);

        var result = await customerService.GetListByFilterAsync(filter, cancellationToken);

        var paginationDto = mapper.MapToPaginationDto(result);

        return paginationDto;
    }

    public async Task<CustomerResponseDto> GetByFilterAsync(CustomerFilterDto filterDto,
        CancellationToken cancellationToken)
    {
        var filter = mapper.MapToCustomerFilter(filterDto);

        var result = await customerService.GetByFilterAsync(filter, cancellationToken);

        var resultDto = mapper.MapToCustomerResponseDto(result);

        return resultDto;
    }

    public async Task UpdateAsync(long id, CustomerRequestDto customerRequestDto, CancellationToken cancellationToken)
    {
        var customer = mapper.MapToCustomer(customerRequestDto);

        await customerService.UpdateAsync(id, customer, cancellationToken);
    }

    public async Task<long> CreateAsync(CustomerRequestDto customerRequestDto, CancellationToken cancellationToken)
    {
        var customer = mapper.MapToCustomer(customerRequestDto);

        var id = await customerService.CreateAsync(customer, cancellationToken);

        return id;
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken)
    {
        await customerService.DeleteAsync(id, cancellationToken);
    }
}