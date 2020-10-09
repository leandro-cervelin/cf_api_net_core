using System;
using System.Text.RegularExpressions;
using CF.CustomerMngt.Domain.Exceptions;

namespace CF.CustomerMngt.Domain.Entities
{
    public static class CustomerExtensions
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

            if (customer.Password.Length < 8 || customer.Password.Length > 20)
                throw new ValidationException(
                    "The Surname must be a string with a minimum length of 8 and a maximum length of 20.");
        }

        public static void ValidateEmail(this Customer customer)
        {
            if (string.IsNullOrEmpty(customer.Email))
                throw new ValidationException("The Email is required.");

            if (!Regex.IsMatch(customer.Email,
                @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z",
                RegexOptions.IgnoreCase))
                throw new ValidationException("The Email is not a valid e-mail address.");
        }

        public static void ValidateSurname(this Customer customer)
        {
            if (string.IsNullOrEmpty(customer.Surname))
                throw new ValidationException("The Surname is required.");

            if (customer.Surname.Length < 2 || customer.Surname.Length > 100)
                throw new ValidationException(
                    "The Surname must be a string with a minimum length of 2 and a maximum length of 100.");
        }

        public static void ValidateFistName(this Customer customer)
        {
            if (string.IsNullOrEmpty(customer.FirstName))
                throw new ValidationException("The First Name is required.");

            if (customer.FirstName.Length < 2 || customer.FirstName.Length > 100)
                throw new ValidationException(
                    "The First Name must be a string with a minimum length of 2 and a maximum length of 100.");
        }
    }
}