using CF.Customer.Application.Dtos;

namespace CF.Customer.Application.Facades.Interfaces;

public interface ICustomerFacade
{
    Task<CustomerResponseDto> GetByFilterAsync(CustomerFilterDto filterDto, CancellationToken cancellationToken);
    Task<PaginationDto<CustomerResponseDto>> GetListByFilterAsync(CustomerFilterDto filterDto,
        CancellationToken cancellationToken);
    Task<long> CreateAsync(CustomerRequestDto customerRequestDto, CancellationToken cancellationToken);
    Task UpdateAsync(long id, CustomerRequestDto customerRequestDto, CancellationToken cancellationToken);
    Task DeleteAsync(long id, CancellationToken cancellationToken);
}