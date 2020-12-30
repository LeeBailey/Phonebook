using Phonebook.Domain.Infrastructure.Abstractions.EntityPersistance;
using Phonebook.Domain.Model.Entities;
using Phonebook.Domain.Model.ValueObjects;
using System;
using System.Threading.Tasks;

namespace Phonebook.Domain.ApplicationServices.Commands
{
    public class CreateNewContactCommand
    {
        private readonly IPhonebookDbContextFactory _phonebookDbContextFactory;

        public CreateNewContactCommand(IPhonebookDbContextFactory phonebookDbContextFactory)
        {
            _phonebookDbContextFactory = phonebookDbContextFactory;
        }

        public async Task Execute(Request request)
        {
            if (string.IsNullOrWhiteSpace(request.ContactFullName))
            {
                throw new ArgumentException(
                    "Value must not be null or empty", nameof(request.ContactFullName));
            }

            if (string.IsNullOrWhiteSpace(request.ContactPhoneNumber.Value))
            {
                throw new ArgumentException(
                    "Value must not be null or empty", nameof(request.ContactPhoneNumber));
            }

            using var phonebookDbContext = _phonebookDbContextFactory.Create();

            var phonebook = await phonebookDbContext.GetUserPhonebook(request.OwnerUserId);

            if (phonebook is null)
            {
                throw new UserPhonebookNotFoundException(request.OwnerUserId);
            }

            phonebook.Contacts.Add(
                new Contact(request.ContactFullName, request.ContactPhoneNumber));

            await phonebookDbContext.SaveChangesAsync();
        }

        public class Request
        {
            public Request(
                Guid ownerUserId,
                string contactFullName,
                PhoneNumber contactPhoneNumber)
            {
                OwnerUserId = ownerUserId;
                ContactFullName = contactFullName;
                ContactPhoneNumber = contactPhoneNumber;
            }

            public Guid OwnerUserId { get; set; }

            public string ContactFullName { get; protected set; }

            public PhoneNumber ContactPhoneNumber { get; protected set; }
        }
    }
}
