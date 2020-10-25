using System.Collections.Generic;

namespace CF.Customer.Application.Dtos
{
    public class PaginationDto<TDto> where TDto : class
    {
        public int CurrentPage { get; set; }
        public int Count { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public List<TDto> Result { get; set; }
    }
}