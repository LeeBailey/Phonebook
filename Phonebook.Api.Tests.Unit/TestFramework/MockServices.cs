using Moq;
using Phonebook.Domain.Infrastructure.Abstractions.EntityPersistance;

namespace Phonebook.Api.Tests.Unit.TestFramework
{
    public class MockServices
    {
        public Mock<IPhonebookDbContext> MockPhonebookDbContext { get; set; }
    }
}
