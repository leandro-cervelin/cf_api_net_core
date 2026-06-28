namespace CF.Customer.Application.Dtos;

public record CustomerResponseDto
{
    public long Id { get; set; }
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string Surname { get; set; }
    public required string FullName { get; set; }
}