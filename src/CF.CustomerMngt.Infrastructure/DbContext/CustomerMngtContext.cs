using CF.CustomerMngt.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CF.CustomerMngt.Infrastructure.DbContext
{
    public class CustomerMngtContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public CustomerMngtContext(DbContextOptions<CustomerMngtContext> options) : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            CustomerModelBuilder(modelBuilder);
        }

        private void CustomerModelBuilder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>().ToTable("Customer");

            modelBuilder.Entity<Customer>()
                .Property(x => x.Email)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<Customer>()
                .HasIndex(x => x.Email)
                .IsUnique();

            modelBuilder.Entity<Customer>()
                .Property(x => x.FirstName)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<Customer>()
                .Property(x => x.Password)
                .HasMaxLength(2000)
                .IsRequired();

            modelBuilder.Entity<Customer>()
                .Property(x => x.Surname)
                .HasMaxLength(100)
                .IsRequired();
        }
    }
}
