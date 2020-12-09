using Microsoft.EntityFrameworkCore;
using Phonebook.Domain.Infrastructure.Abstractions.EntityPersistance;
using Phonebook.Domain.Model.Entities;
using System;
using System.Threading.Tasks;

namespace Phonebook.Infrastructure.EntityPersistance
{
    public class PhonebookDbContext : DbContext, IPhonebookDbContext
    {
        public PhonebookDbContext(DbContextOptions<PhonebookDbContext> options)
            : base(options)
        { }

        protected DbSet<UserPhonebook> UserPhonebooks { get; set; } = default!;

        public async Task<UserPhonebook?> GetUserPhonebook(Guid ownerUserId)
        {
            return await UserPhonebooks
                .Include(x => x.Contacts)
                .SingleOrDefaultAsync(x => x.OwnerUserId == ownerUserId);
        }

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
