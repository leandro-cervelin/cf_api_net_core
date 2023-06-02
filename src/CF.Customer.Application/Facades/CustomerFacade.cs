using AutoMapper;
using CF.Customer.Application.Dtos;
using CF.Customer.Application.Facades.Interfaces;
using CF.Customer.Domain.Models;
using CF.Customer.Domain.Services.Interfaces;

namespace CF.Customer.Application.Facades;

public class CustomerFacade : ICustomerFacade
{
    private readonly ICustomerService _customerService;
    private readonly IMapper _mapper;

    public CustomerFacade(ICustomerService customerService, IMapper mapper)
    {
        _customerService = customerService;
        _mapper = mapper;
    }

    public async Task<PaginationDto<CustomerResponseDto>> GetListByFilterAsync(CustomerFilterDto filterDto, CancellationToken cancellationToken)
    {
        var filter = _mapper.Map<CustomerFilter>(filterDto);

        var result = await _customerService.GetListByFilterAsync(filter, cancellationToken);

        var paginationDto = _mapper.Map<PaginationDto<CustomerResponseDto>>(result);

        return paginationDto;
    }

    public async Task<CustomerResponseDto> GetByFilterAsync(CustomerFilterDto filterDto, CancellationToken cancellationToken)
    {
        var filter = _mapper.Map<CustomerFilter>(filterDto);

        var result = await _customerService.GetByFilterAsync(filter, cancellationToken);

        var resultDto = _mapper.Map<CustomerResponseDto>(result);

        return resultDto;
    }

    public async Task UpdateAsync(long id, CustomerRequestDto customerRequestDto, CancellationToken cancellationToken)
    {
        var customer = _mapper.Map<Domain.Entities.Customer>(customerRequestDto);

        await _customerService.UpdateAsync(id, customer, cancellationToken);
    }

    public async Task<long> CreateAsync(CustomerRequestDto customerRequestDto, CancellationToken cancellationToken)
    {
        var customer = _mapper.Map<Domain.Entities.Customer>(customerRequestDto);

        var id = await _customerService.CreateAsync(customer, cancellationToken);

        return id;
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken)
    {
        await _customerService.DeleteAsync(id, cancellationToken);
    }
}