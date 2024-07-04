using AutoMapper;
using CF.Customer.Application.Dtos;
using CF.Customer.Application.Facades.Interfaces;
using CF.Customer.Domain.Models;
using CF.Customer.Domain.Services.Interfaces;

namespace CF.Customer.Application.Facades;

public class CustomerFacade(ICustomerService customerService, IMapper mapper) : ICustomerFacade
{
    public async Task<PaginationDto<CustomerResponseDto>> GetListByFilterAsync(CustomerFilterDto filterDto,
        CancellationToken cancellationToken)
    {
        var filter = mapper.Map<CustomerFilter>(filterDto);

        var result = await customerService.GetListByFilterAsync(filter, cancellationToken);

        var paginationDto = mapper.Map<PaginationDto<CustomerResponseDto>>(result);

        return paginationDto;
    }

    public async Task<CustomerResponseDto> GetByFilterAsync(CustomerFilterDto filterDto,
        CancellationToken cancellationToken)
    {
        var filter = mapper.Map<CustomerFilter>(filterDto);

        var result = await customerService.GetByFilterAsync(filter, cancellationToken);

        var resultDto = mapper.Map<CustomerResponseDto>(result);

        return resultDto;
    }

    public async Task UpdateAsync(long id, CustomerRequestDto customerRequestDto, CancellationToken cancellationToken)
    {
        var customer = mapper.Map<Domain.Entities.Customer>(customerRequestDto);

        await customerService.UpdateAsync(id, customer, cancellationToken);
    }

    public async Task<long> CreateAsync(CustomerRequestDto customerRequestDto, CancellationToken cancellationToken)
    {
        var customer = mapper.Map<Domain.Entities.Customer>(customerRequestDto);

        var id = await customerService.CreateAsync(customer, cancellationToken);

        return id;
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken)
    {
        await customerService.DeleteAsync(id, cancellationToken);
    }
}