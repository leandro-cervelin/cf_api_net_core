using Microsoft.EntityFrameworkCore;

namespace CF.Customer.Infrastructure.DbContext
{
    public class CustomerContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public CustomerContext(DbContextOptions<CustomerContext> options) : base(options)
        {
        }

        public DbSet<Domain.Entities.Customer> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            CustomerModelBuilder(modelBuilder);
        }

        private static void CustomerModelBuilder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Domain.Entities.Customer>().ToTable("Customer");

            modelBuilder.Entity<Domain.Entities.Customer>()
                .Property(x => x.Email)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<Domain.Entities.Customer>()
                .HasIndex(x => x.Email)
                .IsUnique();

            modelBuilder.Entity<Domain.Entities.Customer>()
                .Property(x => x.FirstName)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<Domain.Entities.Customer>()
                .Property(x => x.Password)
                .HasMaxLength(2000)
                .IsRequired();

            modelBuilder.Entity<Domain.Entities.Customer>()
                .Property(x => x.Surname)
                .HasMaxLength(100)
                .IsRequired();
        }
    }
}