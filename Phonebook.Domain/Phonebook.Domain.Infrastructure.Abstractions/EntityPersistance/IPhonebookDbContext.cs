using Phonebook.Domain.Model.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Phonebook.Domain.Infrastructure.Abstractions.EntityPersistance
{
    public interface IPhonebookDbContext : IDisposable
    {
        DbSet<UserPhonebook> UserPhonebooks { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
