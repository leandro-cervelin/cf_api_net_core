using CF.Customer.Domain.Entities;
using CF.Customer.Domain.Exceptions;
using Xunit;

namespace CF.Customer.UnitTest.Domain.Entities;

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
        // Arrange
        var customer = new Customer.Domain.Entities.Customer
        {
            Password = password
        };

        const string invalidPasswordErrorMessage =
            "Password must be at least 8 characters and contain at 3 of the following: upper case (A-Z), lower case (a-z), number (0-9) and special character (e.g. !@#$%^&*).";

        // Act
        var exception = Assert.Throws<ValidationException>(customer.ValidatePassword);

        // Assert
        Assert.Equal(invalidPasswordErrorMessage, exception.Message);
    }

    [Fact]
    public void ValidPasswordRequirementsTest()
    {
        //Arrange
        var customer = new Customer.Domain.Entities.Customer
        {
            Password = "P@ssWord1"
        };

        //Act
        var exception = Record.Exception(customer.ValidatePassword);

        //Assert
        Assert.Null(exception);
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
        //Arrange
        var customer = new Customer.Domain.Entities.Customer
        {
            Email = email
        };

        const string invalidEmailFormatErrorMessage = "The Email is not a valid e-mail address.";

        //Act
        var exception = Assert.Throws<ValidationException>(customer.ValidateEmail);

        //Assert
        Assert.Equal(invalidEmailFormatErrorMessage, exception.Message);
    }

    [Fact]
    public void InvalidEmailRequiredTest()
    {
        //Arrange
        var customer = new Customer.Domain.Entities.Customer
        {
            Email = string.Empty
        };

        const string invalidEmailFormatErrorMessage = "The Email is required.";

        //Act
        var exception = Assert.Throws<ValidationException>(customer.ValidateEmail);

        //Assert
        Assert.Equal(invalidEmailFormatErrorMessage, exception.Message);
    }

    [Fact]
    public void ValidEmailTest()
    {
        //Arrange
        var customer = new Customer.Domain.Entities.Customer
        {
            Email = "valdivia@gmail.com"
        };

        //Act
        var exception = Record.Exception(customer.ValidateEmail);

        //Assert
        Assert.Null(exception);
    }

    [Theory]
    [InlineData("a")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaawwwwwwwwwwwwwwwwwwwwwwwwwwwwwwewewe")]
    public void InvalidFirstNameTest(string firstName)
    {
        //Arrange
        var customer = new Customer.Domain.Entities.Customer
        {
            FirstName = firstName
        };

        const string invalidFirstName =
            "The First Name must be a string with a minimum length of 2 and a maximum length of 100.";

        //Act
        var exception = Assert.Throws<ValidationException>(customer.ValidateFirstName);

        //Assert
        Assert.Equal(invalidFirstName, exception.Message);
    }

    [Fact]
    public void ValidFirstNameTest()
    {
        //Arrange
        var customer = new Customer.Domain.Entities.Customer
        {
            FirstName = "Valdivia"
        };

        //Act
        var exception = Record.Exception(customer.ValidateFirstName);

        //Assert
        Assert.Null(exception);
    }

    [Fact]
    public void InvalidFirstNameRequiredTest()
    {
        //Arrange
        var customer = new Customer.Domain.Entities.Customer
        {
            FirstName = string.Empty
        };

        const string invalidEmailFormatErrorMessage = "The First Name is required.";

        //Act
        var exception = Assert.Throws<ValidationException>(customer.ValidateFirstName);

        //Assert
        Assert.Equal(invalidEmailFormatErrorMessage, exception.Message);
    }

    [Theory]
    [InlineData("a")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaawwwwwwwwwwwwwwwwwwwwwwwwwwwwwwewewe")]
    public void InvalidSurnameTest(string surname)
    {
        //Arrange
        var customer = new Customer.Domain.Entities.Customer
        {
            Surname = surname
        };

        const string invalidSurname =
            "The Surname must be a string with a minimum length of 2 and a maximum length of 100.";

        //Act
        var exception = Assert.Throws<ValidationException>(customer.ValidateSurname);

        //Assert
        Assert.Equal(invalidSurname, exception.Message);
    }

    [Fact]
    public void ValidSurnameTest()
    {
        //Arrange
        var customer = new Customer.Domain.Entities.Customer
        {
            Surname = "Valdivia"
        };

        //Act
        var exception = Record.Exception(customer.ValidateSurname);

        //Assert
        Assert.Null(exception);
    }

    [Fact]
    public void InvalidSurnameRequiredTest()
    {
        //Arrange
        var customer = new Customer.Domain.Entities.Customer
        {
            Surname = string.Empty
        };

        const string invalidEmailFormatErrorMessage = "The Surname is required.";

        //Act
        var exception = Assert.Throws<ValidationException>(customer.ValidateSurname);

        //Assert
        Assert.Equal(invalidEmailFormatErrorMessage, exception.Message);
    }

    [Fact]
    public void GetFullName()
    {
        //Arrange
        var customer = new Customer.Domain.Entities.Customer
        {
            FirstName = "Valdivia",
            Surname = "El Mago"
        };

        //Act
        var result = customer.GetFullName();

        //Assert
        Assert.Equal("Valdivia El Mago", result);
    }

    [Fact]
    public void SetCreatedDate()
    {
        //Arrange
        var customer = new Customer.Domain.Entities.Customer();
        var actualDate = DateTime.Now;

        //Act
        customer.SetCreatedDate();

        //Assert
        Assert.True(customer.Created >= actualDate);
    }

    [Fact]
    public void SetUpdatedDate()
    {
        //Arrange
        var customer = new Customer.Domain.Entities.Customer();

        //Act
        customer.SetUpdatedDate();

        //Assert
        Assert.NotNull(customer.Updated);
    }
}