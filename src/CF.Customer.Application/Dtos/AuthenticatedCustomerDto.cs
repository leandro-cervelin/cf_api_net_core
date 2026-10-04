namespace CF.Customer.Application.Dtos;

/// <summary>What the API needs to issue an access token. Never returned to clients.</summary>
public record AuthenticatedCustomerDto
{
    public long Id { get; init; }
    public required string Email { get; init; }
    public required string SecurityStamp { get; init; }
}
