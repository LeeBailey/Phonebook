using Microsoft.EntityFrameworkCore;
using Phonebook.Domain.Infrastructure.Abstractions.EntityPersistance;
using Phonebook.Domain.Model.Entities;

namespace Phonebook.Infrastructure.EntityPersistance
{
    public class PhonebookDbContext : DbContext, IPhonebookDbContext
    {
        public PhonebookDbContext(DbContextOptions<PhonebookDbContext> options)
            : base(options)
        { }

        public DbSet<UserPhonebook> UserPhonebooks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserPhonebook>().ToTable(nameof(UserPhonebook));
            modelBuilder.Entity<Contact>().ToTable(nameof(Contact));

            modelBuilder.Entity<Contact>().OwnsOne(
                x => x.ContactPhoneNumber,
                x =>
                {
                    x.Property(y => y.Value)
                        .HasMaxLength(32)
                        .HasColumnName(nameof(Contact.ContactPhoneNumber));
                });
        }
    }
}
