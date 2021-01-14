using Moq;
using Phonebook.Domain.Infrastructure.Abstractions.EntityPersistance;
using System;
using System.Threading;

namespace Phonebook.Api.Tests.TestFramework
{
    internal static class MockPhonebookDbContextExtensions
    { 
        public static void EnsureSaveChangesCalled(
            this Mock<IPhonebookDbContext> mockPhonebookDbContext, Func<Times> times)
        {
            mockPhonebookDbContext.Verify
                (x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), times);
        }

        public static void EnsureDisposeCalled(
            this Mock<IPhonebookDbContext> mockPhonebookDbContext, Func<Times> times)
        {
            mockPhonebookDbContext.Verify(x => x.Dispose(), times);
        }
    }
}
