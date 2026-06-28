namespace CF.Customer.Application.Dtos;

public record CustomerResponseDto
{
    public long Id { get; set; }
    public string Email { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string Surname { get; set; } = null!;
    public string FullName { get; set; } = null!;
}