using Phonebook.Domain.Model.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Phonebook.Domain.Infrastructure.Abstractions.EntityPersistance
{
    public interface IPhonebookDbContext : IDisposable
    {
        Task<UserPhonebook> GetUserPhonebook(Guid ownerUserId);

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
