namespace CF.Customer.Domain.Entities;

public class Customer
{
    public long Id { get; set; }
    public DateTime Created { get; set; }
    public string Email { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Surname { get; set; } = null!;
    public DateTime? Updated { get; set; }
}