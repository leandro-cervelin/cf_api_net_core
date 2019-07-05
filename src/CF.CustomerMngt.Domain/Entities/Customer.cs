using System;
using System.Text.RegularExpressions;
using CF.CustomerMngt.Domain.Exceptions;

namespace CF.CustomerMngt.Domain.Entities
{
    public class Customer
    {
        public long Id { get; set; }
        public DateTime Created { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string Password { get; set; }
        public string Surname { get; set; }
        public DateTime? Updated { get; set; }

        public string GetFullName()
        {
            return $"{FirstName} {Surname}";
        }

        public void SetCreatedDate()
        {
            Created = DateTime.Now;
        }

        public void SetUpdatedDate()
        {
            Updated = DateTime.Now;
        }

        public void Validate()
        {
            if (string.IsNullOrEmpty(FirstName))
                throw new ValidationException("The First Name is required."); 
            
            if (FirstName.Length < 2 || FirstName.Length > 100)
                throw new ValidationException("The First Name must be a string with a minimum length of 2 and a maximum length of 100.");

            if (string.IsNullOrEmpty(Surname))
                throw new ValidationException("The Surname is required."); 
            
            if (Surname.Length < 2 || Surname.Length > 100)
                throw new ValidationException("The Surname must be a string with a minimum length of 2 and a maximum length of 100.");

            if (string.IsNullOrEmpty(Email))
                throw new ValidationException("The Email is required."); 

            if (!Regex.IsMatch(Email, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase))
                throw new ValidationException( "The Email is not a valid e-mail address."); 

            if (string.IsNullOrEmpty(Password))
                throw new ValidationException("The Password is required."); 

            if (Password.Length < 8 || Password.Length > 20)
                throw new ValidationException("The Surname must be a string with a minimum length of 8 and a maximum length of 20.");
        }
    }
}
