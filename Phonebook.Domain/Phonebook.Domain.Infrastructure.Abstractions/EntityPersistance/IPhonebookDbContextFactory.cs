using System;

namespace Phonebook.Domain.Infrastructure.Abstractions.EntityPersistance
{
    public interface IPhonebookDbContextFactory
    {
        public IPhonebookDbContext Create();
    }
}
