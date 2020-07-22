using System.Threading.Tasks;
using CF.CustomerMngt.Application.Dtos;

namespace CF.CustomerMngt.Application.Facades.Interfaces
{
    public interface ICustomerFacade
    {
        Task<CustomerResponseDto> GetByFilterAsync(CustomerFilterDto filterDto);
        Task<PaginationDto<CustomerResponseDto>> GetListByFilterAsync(CustomerFilterDto filterDto);
        Task<long> CreateAsync(CustomerRequestDto customerRequestDto);
        Task UpdateAsync(long id, CustomerRequestDto customerRequestDto);
        Task DeleteAsync(long id);
    }
}
