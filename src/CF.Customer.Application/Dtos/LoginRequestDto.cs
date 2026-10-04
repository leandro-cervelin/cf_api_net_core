using System.ComponentModel.DataAnnotations;

namespace CF.Customer.Application.Dtos;

public record LoginRequestDto
{
    [Required(ErrorMessage = "The Email field is required.")]
    [EmailAddress(ErrorMessage = "The Email field is not a valid email address.")]
    [MaxLength(100, ErrorMessage = "The Email field must not exceed 100 characters.")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "The Password field is required.")]
    [DataType(DataType.Password)]
    public required string Password { get; set; }
}
