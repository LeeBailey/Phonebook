using Phonebook.Domain.Infrastructure.Abstractions.EntityPersistance;
using Phonebook.Domain.Model.Entities;
using System;
using System.Linq;
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

        public async Task Execute(CreateNewContactDto createNewContactDto)
        {
            if (string.IsNullOrWhiteSpace(createNewContactDto.ContactFullName))
            {
                throw new ArgumentException(
                    "Value must not be null or empty", nameof(createNewContactDto.ContactFullName));
            }

            if (string.IsNullOrWhiteSpace(createNewContactDto.ContactPhoneNumber.Value))
            {
                throw new ArgumentException(
                    "Value must not be null or empty", nameof(createNewContactDto.ContactPhoneNumber));
            }

            using var phonebookDbContext = _phonebookDbContextFactory.Create();

            var phonebook = await phonebookDbContext.GetUserPhonebook(createNewContactDto.OwnerUserId);

            if (phonebook is null)
            {
                throw new UserPhonebookNotFoundException(createNewContactDto.OwnerUserId);
            }

            phonebook.Contacts.Add(
                new Contact(createNewContactDto.ContactFullName, createNewContactDto.ContactPhoneNumber));

            await phonebookDbContext.SaveChangesAsync();
        }
    }
}
