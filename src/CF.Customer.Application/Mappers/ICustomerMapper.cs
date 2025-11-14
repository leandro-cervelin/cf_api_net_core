using CF.Customer.Application.Dtos;
using CF.Customer.Domain.Models;

namespace CF.Customer.Application.Mappers;

public interface ICustomerMapper
{
    Domain.Entities.Customer MapToCustomer(CustomerRequestDto dto);
    CustomerResponseDto MapToCustomerResponseDto(Domain.Entities.Customer customer);
    CustomerFilter MapToCustomerFilter(CustomerFilterDto dto);
    PaginationDto<CustomerResponseDto> MapToPaginationDto(Pagination<Domain.Entities.Customer> pagination);
}