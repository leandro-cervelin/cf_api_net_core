namespace CF.Api.Dtos;

public record TokenResponseDto
{
    public required string AccessToken { get; init; }
    public string TokenType { get; init; } = "Bearer";
    public DateTime ExpiresAt { get; init; }
}
