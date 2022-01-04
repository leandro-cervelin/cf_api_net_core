using AutoMapper;
using CF.Customer.Application.Dtos;
using CF.Customer.Domain.Entities;
using CF.Customer.Domain.Models;

namespace CF.Customer.Infrastructure.Mappers;

public class CustomerProfile : Profile
{
    public CustomerProfile()
    {
        CreateCustomerProfile();
    }

    private void CreateCustomerProfile()
    {
        CreateMap<CustomerRequestDto, Domain.Entities.Customer>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Created, opt => opt.Ignore())
            .ForMember(dest => dest.Updated, opt => opt.Ignore());

        CreateMap<Domain.Entities.Customer, CustomerResponseDto>()
            .ForMember(dest => dest.FullName, opt => opt.Ignore())
            .AfterMap((source, destination) => { destination.FullName = source.GetFullName(); });

        CreateMap<CustomerFilterDto, CustomerFilter>();

        CreateMap<Pagination<Domain.Entities.Customer>, PaginationDto<CustomerResponseDto>>()
            .AfterMap((source, converted, context) =>
            {
                converted.Result = context.Mapper.Map<List<CustomerResponseDto>>(source.Result);
            });
    }
}