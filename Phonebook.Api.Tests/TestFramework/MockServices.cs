﻿using Moq;
using Phonebook.Domain.Infrastructure.Abstractions.EntityPersistance;

namespace Phonebook.Api.Tests.TestFramework
{
    internal class MockServices
    {
        public MockServices(Mock<IPhonebookDbContext> mockPhonebookDbContext)
        {
            MockPhonebookDbContext = mockPhonebookDbContext;
        }

        public Mock<IPhonebookDbContext> MockPhonebookDbContext { get; private set; }
    }
}
