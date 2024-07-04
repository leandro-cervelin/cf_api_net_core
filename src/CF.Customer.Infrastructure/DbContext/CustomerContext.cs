using Microsoft.EntityFrameworkCore;

namespace CF.Customer.Infrastructure.DbContext;

public class CustomerContext(DbContextOptions<CustomerContext> options)
    : Microsoft.EntityFrameworkCore.DbContext(options)
{
    public DbSet<Domain.Entities.Customer> Customers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        CustomerModelBuilder(modelBuilder);
    }

    private static void CustomerModelBuilder(ModelBuilder modelBuilder)
    {
        var model = modelBuilder.Entity<Domain.Entities.Customer>();

        model.ToTable("Customer");

        model.Property(x => x.Email)
            .HasMaxLength(100)
            .IsRequired();

        model.HasIndex(x => x.Email)
            .IsUnique();

        model.Property(x => x.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        model.Property(x => x.Password)
            .HasMaxLength(2000)
            .IsRequired();

        model.Property(x => x.Surname)
            .HasMaxLength(100)
            .IsRequired();
    }
}