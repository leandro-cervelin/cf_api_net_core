namespace CF.Customer.Application.Dtos;

public record CustomerResponseDto
{
    public long Id { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string Surname { get; set; }
    public string FullName { get; set; }
}