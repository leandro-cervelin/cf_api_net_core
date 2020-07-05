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
    }
}
