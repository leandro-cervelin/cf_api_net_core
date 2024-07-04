using System.Text.RegularExpressions;
using CF.Customer.Domain.Exceptions;

namespace CF.Customer.Domain.Entities;

public static partial class CustomerExtensions
{
    public static string GetFullName(this Customer customer)
    {
        return $"{customer.FirstName} {customer.Surname}";
    }

    public static void SetCreatedDate(this Customer customer)
    {
        customer.Created = DateTime.Now;
    }

    public static void SetUpdatedDate(this Customer customer)
    {
        customer.Updated = DateTime.Now;
    }

    public static void ValidatePassword(this Customer customer)
    {
        if (string.IsNullOrEmpty(customer.Password))
            throw new ValidationException("The Password is required.");

        const string regex =
            @"^((?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])|(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[^a-zA-Z0-9])|(?=.*?[A-Z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])|(?=.*?[a-z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])).{8,}$";

        if (!Regex.IsMatch(customer.Password, regex))
            throw new ValidationException(
                "Password must be at least 8 characters and contain at 3 of the following: upper case (A-Z), lower case (a-z), number (0-9) and special character (e.g. !@#$%^&*).");
    }

    public static void ValidateEmail(this Customer customer)
    {
        if (string.IsNullOrEmpty(customer.Email))
            throw new ValidationException("The Email is required.");

        if (!EmailValidatorRegex().IsMatch(customer.Email))
            throw new ValidationException("The Email is not a valid e-mail address.");
    }

    public static void ValidateSurname(this Customer customer)
    {
        if (string.IsNullOrEmpty(customer.Surname))
            throw new ValidationException("The Surname is required.");

        if (customer.Surname.Length is < 2 or > 100)
            throw new ValidationException(
                "The Surname must be a string with a minimum length of 2 and a maximum length of 100.");
    }

    public static void ValidateFirstName(this Customer customer)
    {
        if (string.IsNullOrEmpty(customer.FirstName))
            throw new ValidationException("The First Name is required.");

        if (customer.FirstName.Length is < 2 or > 100)
            throw new ValidationException(
                "The First Name must be a string with a minimum length of 2 and a maximum length of 100.");
    }

    [GeneratedRegex(@"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex EmailValidatorRegex();
}