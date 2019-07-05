namespace CF.CustomerMngt.Domain.Helpers.PasswordHasher
{
    public sealed class HashingOptions
    {
        public int Iterations { get; set; } = 10000;
    }
}
