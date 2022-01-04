namespace CF.Customer.Domain.Models;

public class CustomerFilter
{
    public long Id { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string Surname { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string OrderBy { get; set; } = "firstName";
    public string SortBy { get; set; } = "asc";
}