using System.Text;
using System.Text.RegularExpressions;
using CF.Customer.Domain.Exceptions;

namespace CF.Customer.Domain.Entities;

/// <summary>
///     A customer and the rules that keep one valid. CustomerService calls the Validate* methods before saving,
///     so an invalid customer never reaches the database.
/// </summary>
public partial class Customer
{
    public const int MaxPasswordBytes = 72;

    public long Id { get; set; }
    public DateTime Created { get; set; }
    public string Email { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Surname { get; set; } = null!;
    public DateTime? Updated { get; set; }

    /// <summary>
    ///     Random value embedded in access tokens. Rotating it (on password or email change) invalidates every
    ///     token issued before.
    /// </summary>
    public string SecurityStamp { get; set; } = null!;

    public string GetFullName()
    {
        return $"{FirstName} {Surname}";
    }

    public void SetCreatedDate()
    {
        Created = DateTime.UtcNow;
    }

    public void SetUpdatedDate()
    {
        Updated = DateTime.UtcNow;
    }

    public void RotateSecurityStamp()
    {
        SecurityStamp = Guid.NewGuid().ToString("N");
    }

    public void ValidatePassword()
    {
        if (string.IsNullOrEmpty(Password))
            throw new ValidationException("The Password is required.");

        // bcrypt ignores everything past 72 bytes, so longer passwords would match any value sharing that prefix.
        if (Encoding.UTF8.GetByteCount(Password) > MaxPasswordBytes)
            throw new ValidationException($"The Password must not exceed {MaxPasswordBytes} bytes.");

        const string regex =
            @"^((?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])|(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[^a-zA-Z0-9])|(?=.*?[A-Z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])|(?=.*?[a-z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])).{8,}$";

        if (!Regex.IsMatch(Password, regex))
            throw new ValidationException(
                "Password must be at least 8 characters and contain at least 3 of the following: upper case (A-Z), lower case (a-z), number (0-9) and special character (e.g. !@#$%^&*).");
    }

    public void ValidateEmail()
    {
        if (string.IsNullOrEmpty(Email))
            throw new ValidationException("The Email is required.");

        if (!EmailValidatorRegex().IsMatch(Email))
            throw new ValidationException("The Email is not a valid e-mail address.");
    }

    public void ValidateSurname()
    {
        if (string.IsNullOrEmpty(Surname))
            throw new ValidationException("The Surname is required.");

        if (Surname.Length is < 2 or > 100)
            throw new ValidationException(
                "The Surname must be a string with a minimum length of 2 and a maximum length of 100.");
    }

    public void ValidateFirstName()
    {
        if (string.IsNullOrEmpty(FirstName))
            throw new ValidationException("The First Name is required.");

        if (FirstName.Length is < 2 or > 100)
            throw new ValidationException(
                "The First Name must be a string with a minimum length of 2 and a maximum length of 100.");
    }

    [GeneratedRegex(
        @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z",
        RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex EmailValidatorRegex();
}
