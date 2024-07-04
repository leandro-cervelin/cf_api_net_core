using CF.Customer.Domain.Services;
using Xunit;

namespace CF.Customer.UnitTest.Domain.Services;

public class PasswordHasherTest
{
    [Fact]
    public async Task HashOkTestAsync()
    {
        //Arrange
        const string password = "Blah@!1894";
        var service = new PasswordHasherService();

        //Act
        var result = await service.HashAsync(password);

        //Assert
        Assert.NotEqual(password, result);
    }

    [Fact]
    public async Task VerifyOkTestAsync()
    {
        //Arrange
        const string password = "Blah@!1894";
        var service = new PasswordHasherService();

        //Act
        var hash = await service.HashAsync(password);
        var isValid = await service.VerifyAsync(password, hash);

        //Assert
        Assert.True(isValid);
    }

    [Fact]
    public async Task VerifyNotOkTestAsync()
    {
        //Arrange
        const string password = "Blah@!1894";
        const string fakePassword = "Blah@!4981";
        var service = new PasswordHasherService();

        //Act
        var hash = await service.HashAsync(password);
        var isValid = await service.VerifyAsync(fakePassword, hash);

        //Assert
        Assert.False(isValid);
    }
}