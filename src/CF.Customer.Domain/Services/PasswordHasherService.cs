using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using CF.Customer.Domain.Services.Interfaces;

namespace CF.Customer.Domain.Services
{
    /// <summary>
    ///     https://gist.github.com/edamtoft
    /// </summary>
    public sealed class PasswordHasherServiceService : IPasswordHasherService
    {
        private const int SaltSize = 16; // 128 bit 
        private const int KeySize = 32; // 256 bit
        private const int Iterations = 1000;

        public string Hash(string password)
        {
            using var algorithm = new Rfc2898DeriveBytes(password, SaltSize, Iterations, HashAlgorithmName.SHA512);
            var key = Convert.ToBase64String(algorithm.GetBytes(KeySize));
            var salt = Convert.ToBase64String(algorithm.Salt);

            return $"{Iterations}.{salt}.{key}";
        }

        public (bool Verified, bool NeedsUpgrade) Check(string hash, string password)
        {
            var parts = hash.Split('.', 3);

            if (parts.Length != 3)
                throw new FormatException(
                    "Unexpected hash format. Should be formatted as '{iterations}.{salt}.{hash}'");

            var iterations = Convert.ToInt32(parts[0]);
            var salt = Convert.FromBase64String(parts[1]);
            var key = Convert.FromBase64String(parts[2]);

            var needsUpgrade = iterations != Iterations;

            using var algorithm = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA512);
            var keyToCheck = algorithm.GetBytes(KeySize);

            var verified = keyToCheck.SequenceEqual(key);

            return (verified, needsUpgrade);
        }

        public static bool ValidatePasswordRequirements(string password)
        {
            const string regex =
                @"^((?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])|(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[^a-zA-Z0-9])|(?=.*?[A-Z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])|(?=.*?[a-z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])).{8,}$";
            return Regex.IsMatch(password, regex);
        }
    }
}