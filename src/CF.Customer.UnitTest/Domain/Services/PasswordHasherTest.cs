using System;
using CF.Customer.Domain.Services;
using Xunit;

namespace CF.Customer.UnitTest.Domain.Services
{
    public class PasswordHasherTest
    {
        [Fact]
        public void HashOkTest()
        {
            //Arrange
            const string password = "Blah@!1894";
            var service = new PasswordHasherServiceService();

            //Act
            var result = service.Hash(password);
            var hashSplit = result.Split(".");
            var saltBytes = new Span<byte>(new byte[128]);
            var keyBytes = new Span<byte>(new byte[256]);

            //Assert
            Assert.Equal("1000", hashSplit[0]);
            Assert.True(Convert.TryFromBase64String(hashSplit[1], saltBytes, out _));
            Assert.True(Convert.TryFromBase64String(hashSplit[2], keyBytes, out _));
        }

        [Fact]
        public void CheckOkTest()
        {
            const string password = "Blah@!1894";
            var service = new PasswordHasherServiceService();
            var hash = service.Hash(password);
            var (verified, needsUpgrade) = service.Check(hash, password);
            Assert.True(verified);
            Assert.False(needsUpgrade);
        }

        [Fact]
        public void CheckNotVerifiedTest()
        {
            const string password = "Blah@!1894";
            const string fakePassword = "Blah@!4981";
            var service = new PasswordHasherServiceService();
            var hash = service.Hash(password);
            var (verified, _) = service.Check(hash, fakePassword);
            Assert.False(verified);
        }

        [Fact]
        public void CheckNeedsUpgradeTest()
        {
            const string password = "Blah@!1894";
            var service = new PasswordHasherServiceService();
            var hash = service.Hash(password).Replace("1000", "900");
            var (_, needsUpgrade) = service.Check(hash, password);
            Assert.True(needsUpgrade);
        }
    }
}