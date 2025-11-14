using CF.Customer.Application.Dtos;
using CF.Customer.Application.Mappers;
using CF.Customer.Domain.Entities;
using CF.Customer.Domain.Models;

namespace CF.Customer.Infrastructure.Mappers;

public class CustomerMapper : ICustomerMapper
{
    public Domain.Entities.Customer MapToCustomer(CustomerRequestDto dto)
    {
        return new Domain.Entities.Customer
        {
            Email = dto.Email,
            FirstName = dto.FirstName,
            Password = dto.Password,
            Surname = dto.Surname
        };
    }

    public CustomerResponseDto MapToCustomerResponseDto(Domain.Entities.Customer customer)
    {
        return new CustomerResponseDto
        {
            Id = customer.Id,
            Email = customer.Email,
            FirstName = customer.FirstName,
            Surname = customer.Surname,
            FullName = customer.GetFullName()
        };
    }

    public CustomerFilter MapToCustomerFilter(CustomerFilterDto dto)
    {
        return new CustomerFilter
        {
            Id = dto.Id,
            Email = dto.Email,
            FirstName = dto.FirstName,
            Surname = dto.Surname,
            CurrentPage = dto.CurrentPage,
            PageSize = dto.PageSize,
            OrderBy = dto.OrderBy,
            SortBy = dto.SortBy
        };
    }

    public PaginationDto<CustomerResponseDto> MapToPaginationDto(Pagination<Domain.Entities.Customer> pagination)
    {
        return new PaginationDto<CustomerResponseDto>
        {
            CurrentPage = pagination.CurrentPage,
            Count = pagination.Count,
            PageSize = pagination.PageSize,
            TotalPages = pagination.TotalPages,
            Result = pagination.Result.Select(MapToCustomerResponseDto).ToList()
        };
    }
}