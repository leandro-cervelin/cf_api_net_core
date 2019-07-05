using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using CF.CustomerMngt.Application.Dtos;
using CF.CustomerMngt.Application.Facades.Interfaces;
using CF.CustomerMngt.Domain.Entities;
using CF.CustomerMngt.Domain.Models;
using CF.CustomerMngt.Domain.Services.Interfaces;

namespace CF.CustomerMngt.Application.Facades
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

        public async Task<PaginationDto<CustomerResponseDto>> GetListByFilter(CustomerFilterDto filterDto)
        {
            var filter = _mapper.Map<CustomerFilter>(filterDto);

            var result = await _customerService.GetListByFilter(filter);

            var paginationDto = _mapper.Map<PaginationDto<CustomerResponseDto>>(result);

            return paginationDto;
        }

        public async Task<CustomerResponseDto> GetByFilter(CustomerFilterDto filterDto)
        {
            var filter = _mapper.Map<CustomerFilter>(filterDto);

            var result = await _customerService.GetByFilter(filter);

            var resultDto = _mapper.Map<CustomerResponseDto>(result);

            return resultDto;
        }

        public async Task Update(long id, CustomerRequestDto customerRequestDto)
        {
            var customer = _mapper.Map<Customer>(customerRequestDto);

            await _customerService.Update(id, customer);
        }

        public async Task<long> Create(CustomerRequestDto customerRequestDto)
        {
            var customer = _mapper.Map<Customer>(customerRequestDto);

            var id = await _customerService.Create(customer);

            return id;
        }

        public async Task Delete(long id)
        {
            await _customerService.Delete(id);
        }
    }
}

