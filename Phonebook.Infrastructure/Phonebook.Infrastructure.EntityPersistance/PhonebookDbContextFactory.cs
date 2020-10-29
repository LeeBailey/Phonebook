using Phonebook.Domain.Infrastructure.Abstractions.EntityPersistance;
using Microsoft.EntityFrameworkCore;

namespace Phonebook.Infrastructure.EntityPersistance
{
    public class PhonebookDbContextFactory : IPhonebookDbContextFactory
    {
        private readonly string _connectionString;

        public PhonebookDbContextFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IPhonebookDbContext Create()
        {
            var optionsBuilder = new DbContextOptionsBuilder<PhonebookDbContext>();
            optionsBuilder.UseSqlServer(_connectionString);

            return new PhonebookDbContext(optionsBuilder.Options);
        }
    }
}
