﻿using System.Threading.Tasks;
using AutoMapper;
using CF.Customer.Application.Dtos;
using CF.Customer.Application.Facades.Interfaces;
using CF.Customer.Domain.Models;
using CF.Customer.Domain.Services.Interfaces;

namespace CF.Customer.Application.Facades
{
    public class CustomerFacade : ICustomerFacade
    {
        private readonly ICustomerService _customerService;
        private readonly IMapper _mapper;

        public CustomerFacade(ICustomerService customerService, IMapper mapper)
        {
            _customerService = customerService;
            _mapper = mapper;
        }

        public async Task<PaginationDto<CustomerResponseDto>> GetListByFilterAsync(CustomerFilterDto filterDto)
        {
            var filter = _mapper.Map<CustomerFilter>(filterDto);

            var result = await _customerService.GetListByFilterAsync(filter);

            var paginationDto = _mapper.Map<PaginationDto<CustomerResponseDto>>(result);

            return paginationDto;
        }

        public async Task<CustomerResponseDto> GetByFilterAsync(CustomerFilterDto filterDto)
        {
            var filter = _mapper.Map<CustomerFilter>(filterDto);

            var result = await _customerService.GetByFilterAsync(filter);

            var resultDto = _mapper.Map<CustomerResponseDto>(result);

            return resultDto;
        }

        public async Task UpdateAsync(long id, CustomerRequestDto customerRequestDto)
        {
            var customer = _mapper.Map<Domain.Entities.Customer>(customerRequestDto);

            await _customerService.UpdateAsync(id, customer);
        }

        public async Task<long> CreateAsync(CustomerRequestDto customerRequestDto)
        {
            var customer = _mapper.Map<Domain.Entities.Customer>(customerRequestDto);

            var id = await _customerService.CreateAsync(customer);

            return id;
        }

        public async Task DeleteAsync(long id)
        {
            await _customerService.DeleteAsync(id);
        }
    }
}