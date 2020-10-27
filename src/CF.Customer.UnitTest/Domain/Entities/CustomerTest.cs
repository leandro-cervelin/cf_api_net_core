using System;
using CF.Customer.Domain.Entities;
using CF.Customer.Domain.Exceptions;
using Xunit;
using Xunit.Sdk;

namespace CF.Customer.UnitTest.Domain.Entities
{
    public class CustomerTest
    {
        [Theory]
        [InlineData("1894")]
        [InlineData("AA189412")]
        [InlineData("AABCSSDSD")]
        [InlineData("wedededwe")]
        [InlineData("wedeAAAAA")]
        [InlineData("aa189412")]
        [InlineData("@@189412")]
        [InlineData("@abcdefg")]
        [InlineData("@AASDFEF")]
        [InlineData("12345678")]
        [InlineData("@@@!@!@!@")]
        [InlineData("1123@aA")]
        [InlineData("1123aaswdewwpfomwfpo4")]
        public void InvalidPasswordRequirementsTest(string password)
        {
            var customer = new Customer.Domain.Entities.Customer
            {
                Password = password
            };

            const string invalidPasswordErrorMessage =
                "Password must be at least 8 characters and contain at 3 of the following: upper case (A-Z), lower case (a-z), number (0-9) and special character (e.g. !@#$%^&*).";

            var exception = Assert.Throws<ValidationException>(() => customer.ValidatePassword());
            Assert.Equal(invalidPasswordErrorMessage, exception.Message);
        }

        [Fact]
        public void ValidPasswordRequirementsTest()
        {
            var customer = new Customer.Domain.Entities.Customer
            {
                Password = "P@ssWord1"
            };
            try
            {
                Assert.Throws<Exception>(() => customer.ValidatePassword());
            }
            catch (AssertActualExpectedException exception)
            {
                Assert.Equal("(No exception was thrown)", exception.Actual);
            }
        }

        [Theory]
        [InlineData("1894@")]
        [InlineData("aaa@com.   ")]
        [InlineData("aaa@@.com   ")]
        [InlineData("aaa   @gmail.com")]
        [InlineData("aaa   @gmail")]
        [InlineData("aaa@gmail.com   ")]
        [InlineData("@gmail.com")]
        public void InvalidEmailFormatTest(string email)
        {
            var customer = new Customer.Domain.Entities.Customer
            {
                Email = email
            };

            const string invalidEmailFormatErrorMessage = "The Email is not a valid e-mail address.";

            var exception = Assert.Throws<ValidationException>(() => customer.ValidateEmail());
            Assert.Equal(invalidEmailFormatErrorMessage, exception.Message);
        }

        [Fact]
        public void InvalidEmailRequiredTest()
        {
            var customer = new Customer.Domain.Entities.Customer
            {
                Email = string.Empty
            };

            const string invalidEmailFormatErrorMessage = "The Email is required.";

            var exception = Assert.Throws<ValidationException>(() => customer.ValidateEmail());
            Assert.Equal(invalidEmailFormatErrorMessage, exception.Message);
        }

        [Fact]
        public void ValidEmailTest()
        {
            var customer = new Customer.Domain.Entities.Customer
            {
                Email = "valdivia@gmail.com"
            };
            try
            {
                Assert.Throws<Exception>(() => customer.ValidateEmail());
            }
            catch (AssertActualExpectedException exception)
            {
                Assert.Equal("(No exception was thrown)", exception.Actual);
            }
        }

        [Theory]
        [InlineData("a")]
        [InlineData(
            "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaawwwwwwwwwwwwwwwwwwwwwwwwwwwwwwewewe")]
        public void InvalidFirstNameTest(string firstName)
        {
            var customer = new Customer.Domain.Entities.Customer
            {
                FirstName = firstName
            };

            const string invalidFirstName =
                "The First Name must be a string with a minimum length of 2 and a maximum length of 100.";

            var exception = Assert.Throws<ValidationException>(() => customer.ValidateFirstName());
            Assert.Equal(invalidFirstName, exception.Message);
        }

        [Fact]
        public void ValidFirstNameTest()
        {
            var customer = new Customer.Domain.Entities.Customer
            {
                FirstName = "Valdivia"
            };
            try
            {
                Assert.Throws<Exception>(() => customer.ValidateFirstName());
            }
            catch (AssertActualExpectedException exception)
            {
                Assert.Equal("(No exception was thrown)", exception.Actual);
            }
        }

        [Fact]
        public void InvalidFirstNameRequiredTest()
        {
            var customer = new Customer.Domain.Entities.Customer
            {
                FirstName = string.Empty
            };

            const string invalidEmailFormatErrorMessage = "The First Name is required.";

            var exception = Assert.Throws<ValidationException>(() => customer.ValidateFirstName());
            Assert.Equal(invalidEmailFormatErrorMessage, exception.Message);
        }

        [Theory]
        [InlineData("a")]
        [InlineData(
            "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaawwwwwwwwwwwwwwwwwwwwwwwwwwwwwwewewe")]
        public void InvalidSurnameTest(string surname)
        {
            var customer = new Customer.Domain.Entities.Customer
            {
                Surname = surname
            };

            const string invalidSurname =
                "The Surname must be a string with a minimum length of 2 and a maximum length of 100.";

            var exception = Assert.Throws<ValidationException>(() => customer.ValidateSurname());
            Assert.Equal(invalidSurname, exception.Message);
        }

        [Fact]
        public void ValidSurnameTest()
        {
            var customer = new Customer.Domain.Entities.Customer
            {
                Surname = "Valdivia"
            };
            try
            {
                Assert.Throws<Exception>(() => customer.ValidateSurname());
            }
            catch (AssertActualExpectedException exception)
            {
                Assert.Equal("(No exception was thrown)", exception.Actual);
            }
        }

        [Fact]
        public void InvalidSurnameRequiredTest()
        {
            var customer = new Customer.Domain.Entities.Customer
            {
                Surname = string.Empty
            };

            const string invalidEmailFormatErrorMessage = "The Surname is required.";

            var exception = Assert.Throws<ValidationException>(() => customer.ValidateSurname());
            Assert.Equal(invalidEmailFormatErrorMessage, exception.Message);
        }

        [Fact]
        public void GetFullName()
        {
            var customer = new Customer.Domain.Entities.Customer
            {
                FirstName = "Valdivia",
                Surname = "El Mago"
            };

            Assert.Equal("Valdivia El Mago", customer.GetFullName());
        }

        [Fact]
        public void SetCreatedDate()
        {
            var customer = new Customer.Domain.Entities.Customer();
            var actualDate = DateTime.Now;
            customer.SetCreatedDate();

            Assert.True(customer.Created >= actualDate);
        }

        [Fact]
        public void SetUpdatedDate()
        {
            var customer = new Customer.Domain.Entities.Customer();
            customer.SetUpdatedDate();

            Assert.NotNull(customer.Updated);
        }
    }
}