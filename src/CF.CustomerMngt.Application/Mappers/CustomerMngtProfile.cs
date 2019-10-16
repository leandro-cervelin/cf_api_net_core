using System.Collections.Generic;
using AutoMapper;
using CF.CustomerMngt.Application.Dtos;
using CF.CustomerMngt.Domain.Entities;
using CF.CustomerMngt.Domain.Models;

namespace CF.CustomerMngt.Application.Mappers
{
    public class CustomerMngtProfile : Profile
    {
        public CustomerMngtProfile()
        {
            CustomerProfile();
        }

        private void CustomerProfile()
        {
            CreateMap<CustomerRequestDto, Customer>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Created, opt => opt.Ignore())
                .ForMember(dest => dest.Updated, opt => opt.Ignore());

            CreateMap<Customer, CustomerResponseDto>()
                .AfterMap((source, destination) => { destination.FullName = source.GetFullName(); });

            CreateMap<CustomerFilterDto, CustomerFilter>();

            CreateMap<Pagination<Customer>, PaginationDto<CustomerResponseDto>>()
                .AfterMap((source, converted, context) =>
                {
                    converted.Result = context.Mapper.Map<List<CustomerResponseDto>>(source.Result);
                });
        }
    }
}
