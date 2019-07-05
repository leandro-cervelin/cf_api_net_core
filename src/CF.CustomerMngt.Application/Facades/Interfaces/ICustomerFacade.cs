using System.Collections.Generic;
using System.Threading.Tasks;
using CF.CustomerMngt.Application.Dtos;

namespace CF.CustomerMngt.Application.Facades.Interfaces
{
    public interface ICustomerFacade
    {
        Task<CustomerResponseDto> GetByFilter(CustomerFilterDto filterDto);
        Task<PaginationDto<CustomerResponseDto>> GetListByFilter(CustomerFilterDto filterDto);
        Task<long> Create(CustomerRequestDto customerRequestDto);
        Task Update(long id, CustomerRequestDto customerRequestDto);
        Task Delete(long id);
    }
}
