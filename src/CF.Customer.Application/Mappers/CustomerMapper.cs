using CF.Customer.Application.Dtos;
using CF.Customer.Domain.Entities;
using CF.Customer.Domain.Models;

namespace CF.Customer.Application.Mappers;

/// <summary>
///     Converts between the DTOs the API sends and receives and the domain types the business rules work with.
///     Keeping this in one place means the entity (with its password hash) never leaves the Application layer by
///     accident: callers only ever get DTOs back.
/// </summary>
public static class CustomerMapper
{
    public static Domain.Entities.Customer MapToCustomer(CustomerRequestDto dto)
    {
        return new Domain.Entities.Customer
        {
            Email = dto.Email,
            FirstName = dto.FirstName,
            Password = dto.Password,
            Surname = dto.Surname
        };
    }

    public static CustomerResponseDto MapToCustomerResponseDto(Domain.Entities.Customer customer)
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

    public static AuthenticatedCustomerDto MapToAuthenticatedCustomerDto(Domain.Entities.Customer customer)
    {
        return new AuthenticatedCustomerDto
        {
            Id = customer.Id,
            Email = customer.Email,
            SecurityStamp = customer.SecurityStamp
        };
    }

    public static CustomerFilter MapToCustomerFilter(CustomerFilterDto dto)
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

    public static PaginationDto<CustomerResponseDto> MapToPaginationDto(
        Pagination<Domain.Entities.Customer> pagination)
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