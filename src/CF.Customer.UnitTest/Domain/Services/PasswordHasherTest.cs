using CF.Customer.Domain.Services;
using Xunit;

namespace CF.Customer.UnitTest.Domain.Services;

public class PasswordHasherTest
{
    [Fact]
    public void HashOkTest()
    {
        //Arrange
        const string password = "Blah@!1894";
        var service = new PasswordHasherService();

        //Act
        var result = service.Hash(password);

        //Assert
        Assert.NotEqual(password, result);
    }

    [Fact]
    public void VerifyOkTest()
    {
        //Arrange
        const string password = "Blah@!1894";
        var service = new PasswordHasherService();

        //Act
        var hash = service.Hash(password);
        var isValid = service.Verify(password, hash);

        //Assert
        Assert.True(isValid);
    }

    [Fact]
    public void VerifyNotOkTest()
    {
        //Arrange
        const string password = "Blah@!1894";
        const string fakePassword = "Blah@!4981";
        var service = new PasswordHasherService();

        //Act
        var hash = service.Hash(password);
        var isValid = service.Verify(fakePassword, hash);

        //Assert
        Assert.False(isValid);
    }
}