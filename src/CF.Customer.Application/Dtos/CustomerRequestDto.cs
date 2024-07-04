using System.ComponentModel.DataAnnotations;

namespace CF.Customer.Application.Dtos;

public record CustomerRequestDto
{
    [Required(ErrorMessage = "The Email field is required.")]
    [EmailAddress(ErrorMessage = "The Email field is not a valid email address.")]
    [MaxLength(100, ErrorMessage = "The Email field must not exceed 100 characters.")]
    [Display(Name = "Email")]
    public string Email { get; set; }

    [Required(ErrorMessage = "The First Name field is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "The First Name field must be between 2 and 100 characters.")]
    [Display(Name = "First Name")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "The Password field is required.")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    [RegularExpression(
        "^((?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])|(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[^a-zA-Z0-9])|(?=.*?[A-Z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])|(?=.*?[a-z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])).{8,}$",
        ErrorMessage =
            "Passwords must be at least 8 characters and contain at least 3 of the following: upper case (A-Z), lower case (a-z), number (0-9), and special character (e.g. !@#$%^&*).")]
    public string Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "The passwords do not match.")]
    public string ConfirmPassword { get; set; }

    [Required(ErrorMessage = "The Surname field is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "The Surname field must be between 2 and 100 characters.")]
    [Display(Name = "Surname")]
    public string Surname { get; set; }
}